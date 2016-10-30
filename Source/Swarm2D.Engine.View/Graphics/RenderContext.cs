/******************************************************************************
Copyright (c) 2016 Koray Kiyakoglu

http://www.swarm2d.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Swarm2D.Library;

namespace Swarm2D.Engine.View
{
    public class RenderContext
    {
        public IOSystem IOSystem { get; private set; }
        public int Order { get; private set; }

        private bool _viewMatrixSet;
        private Matrix4x4 _viewMatrix;

        public Matrix4x4 ViewMatrix
        {
            get { return _viewMatrix; }
            set
            {
                _viewMatrixSet = true;
                _viewMatrix = value;
            }
        }

        private Framework _framework;

        private List<GraphicsCommand> _graphicsCommands;
        private List<RenderContext> _renderContexts;

        private bool _scissorSet;
        private int _scissorX;
        private int _scissorY;
        private int _scissorWidth;
        private int _scissorHeight;

        private Dictionary<int, List<RenderJob>> _renderJobs;
        private List<int> _renderJobOrders;

        internal RenderContext(IOSystem ioSystem, Framework framework, int order)
        {
            Order = order;
            _viewMatrixSet = false;
            _viewMatrix = Matrix4x4.Identity;

            _renderContexts = new List<RenderContext>();
            _renderJobs = new Dictionary<int, List<RenderJob>>();
            _renderJobOrders = new List<int>();
            _renderJobOrders.Add(0);
            _renderJobs.Add(0, new List<RenderJob>());
            _graphicsCommands = new List<GraphicsCommand>(32);
            IOSystem = ioSystem;
            _framework = framework;
        }

        public void AddGraphicsCommand(GraphicsCommand graphicsCommand)
        {
            graphicsCommand.IOSystem = IOSystem;
            graphicsCommand.Framework = _framework;

            _graphicsCommands.Add(graphicsCommand);
        }

        public void AddDrawMeshJob(float x, float y, Mesh mesh, Material material)
        {
            AddRenderJob(new RenderJob(new Vector2(x, y), mesh, material));
        }

        public void AddDrawMeshJob(Matrix4x4 matrix, Mesh mesh, Material material)
        {
            AddRenderJob(new RenderJob(matrix, mesh, material));
        }

        public void AddDrawSpriteJob(float x, float y, Sprite sprite, float scale, float width, float height)
        {
            float[] vertices;
            float[] uvs;
            Texture texture;

            sprite.GetArrays(x, y, scale, width, height, out texture, out vertices, out uvs);
        
            //TODO: temporary
            vertices = new List<float>(vertices).ToArray();
            uvs = new List<float>(uvs).ToArray();

            AddRenderJob(new RenderJob(new Vector2(0, 0), new Mesh(MeshTopology.Quads, vertices, uvs, vertices.Length / 2), new SimpleMaterial(texture)));
        }

        public void AddDrawSpriteJob(Matrix4x4 matrix, Sprite sprite, float scale, float width, float height)
        {
            float[] vertices;
            float[] uvs;
            Texture texture;

            sprite.GetArrays(0, 0, scale, width, height, out texture, out vertices, out uvs);

            //TODO: temporary
            vertices = new List<float>(vertices).ToArray();
            uvs = new List<float>(uvs).ToArray();

            AddRenderJob(new RenderJob(matrix, new Mesh(MeshTopology.Quads, vertices, uvs, vertices.Length / 2), new SimpleMaterial(texture)));
        }

        private void AddRenderJob(RenderJob renderJob)
        {
            int renderOrder = renderJob.Material.RenderOrder;

            if (!_renderJobs.ContainsKey(renderOrder))
            {
                _renderJobs.Add(renderOrder, new List<RenderJob>());
                _renderJobOrders.Add(renderOrder);
                _renderJobOrders.Sort(new RenderJobSorter());
            }

            _renderJobs[renderOrder].Add(renderJob);
        }

        internal void Render()
        {
            if (_scissorSet)
            {
                Graphics.PushScissor(_scissorX, _scissorY, _scissorWidth, _scissorHeight);
            }

            Graphics.ViewMatrix = _viewMatrix;
            Graphics.ModelMatrix = Matrix4x4.Identity;

            for (int i = 0; i < _graphicsCommands.Count; i++)
            {
                GraphicsCommand graphicsCommand = _graphicsCommands[i];

                graphicsCommand.PrepareJob();
                graphicsCommand.DoJob();
            }

            Graphics.ViewMatrix = _viewMatrix;
            Graphics.ModelMatrix = Matrix4x4.Identity;

            for (int i = 0; i < _renderJobOrders.Count; i++)
            {
                int renderOrder = _renderJobOrders[i];
                var renderJobList = _renderJobs[renderOrder];

                for (int j = 0; j < renderJobList.Count; j++)
                {
                    RenderJob renderJob = renderJobList[j];

                    if (renderJob.JobMode == RenderJob.Mode.Position)
                    {
                        Graphics.ModelMatrix = Matrix4x4.Identity;
                        Graphics.DrawArrays(renderJob.Position.X, renderJob.Position.Y, renderJob.Material, renderJob.Mesh);
                    }
                    else if (renderJob.JobMode == RenderJob.Mode.Matrix)
                    {
                        Graphics.ModelMatrix = renderJob.Matrix;
                        Graphics.DrawArrays(renderJob.Material, renderJob.Mesh);
                        Graphics.ModelMatrix = Matrix4x4.Identity;
                    }
                    else if (renderJob.JobMode == RenderJob.Mode.Simple)
                    {
                        Graphics.ModelMatrix = Matrix4x4.Identity;
                        Graphics.DrawArrays(renderJob.Material, renderJob.Mesh);
                    }
                }
            }

            for (int i = 0; i < _renderContexts.Count; i++)
            {
                var childRenderContext = _renderContexts[i];
                childRenderContext.Render();
            }

            if (_scissorSet)
            {
                Graphics.PopScissor();
            }
        }

        public void SetScissor(int x, int y, int width, int height)
        {
            _scissorSet = true;
            _scissorX = x;
            _scissorY = y;
            _scissorWidth = width;
            _scissorHeight = height;
        }

        public RenderContext AddChildRenderContext(int order)
        {
            var renderContext = new RenderContext(IOSystem, _framework, order);
            _renderContexts.Add(renderContext);

            _renderContexts.Sort(new RenderContextSorter());

            return renderContext;
        }
    }

    class RenderJob
    {
        public enum Mode
        {
            Position,
            Matrix,
            Simple
        }

        public Vector2 Position { get; private set; }
        public Matrix4x4 Matrix { get; private set; }

        public Mode JobMode { get; private set; }

        public Material Material { get; private set; }
        public Mesh Mesh { get; private set; }

        public RenderJob(Vector2 position, Mesh mesh, Material material)
        {
            JobMode = Mode.Position;
            Position = position;
            Mesh = mesh;
            Material = material;
        }

        public RenderJob(Matrix4x4 matrix, Mesh mesh, Material material)
        {
            JobMode = Mode.Matrix;
            Matrix = matrix;
            Mesh = mesh;
            Material = material;
        }

        public RenderJob(Mesh mesh, Material material)
        {
            JobMode = Mode.Simple;
            Mesh = mesh;
            Material = material;
        }
    }

    class BatchedDrawTextureContext
    {
        internal Texture Texture { get; private set; }

        internal List<float> Vertices { get; private set; }
        internal List<float> Uvs { get; private set; }

        internal BatchedDrawTextureContext(Texture texture)
        {
            Texture = texture;

            Vertices = new List<float>(8192);
            Uvs = new List<float>(8192);
        }

        internal void AddRange(float[] vertices, float[] uvs)
        {
            Vertices.AddRange(vertices);
            Uvs.AddRange(uvs);
        }
    }

    class RenderContextSorter : IComparer<RenderContext>
    {
        public int Compare(RenderContext renderContextA, RenderContext renderContextB)
        {
            return renderContextA.Order.CompareTo(renderContextB.Order);
        }
    }

    class RenderJobSorter : IComparer<int>
    {
        public int Compare(int a, int b)
        {
            return a.CompareTo(b);
        }
    }
}
