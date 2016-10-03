﻿/******************************************************************************
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

namespace Swarm2D.Engine.View
{
    public class RenderContext
    {
        public IOSystem IOSystem { get; private set; }

        private Framework _framework;

        private List<GraphicsCommand> _graphicsCommands;
        private List<RenderContext> _renderContexts; 

        internal RenderContext(IOSystem ioSystem, Framework framework)
        {
            _renderContexts = new List<RenderContext>();
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

        internal void Render()
        {
            GraphicsCommand lastBatchedCommand = null;

            for (int i = 0; i < _graphicsCommands.Count; i++)
            {
                GraphicsCommand graphicsCommand = _graphicsCommands[i];

                if (lastBatchedCommand == null && graphicsCommand.Batchable)
                {
                    lastBatchedCommand = graphicsCommand;
                    graphicsCommand.DoJob();
                }
                else
                {
                    if (lastBatchedCommand != null)
                    {
                        if (!graphicsCommand.Batchable || !lastBatchedCommand.TryBatch(graphicsCommand))
                        {
                            lastBatchedCommand.DoBatchedJob();
                            lastBatchedCommand = null;

                            graphicsCommand.DoJob();
                            graphicsCommand.DoBatchedJob();
                        }
                        else
                        {
                            graphicsCommand.DoJob();
                        }
                    }
                    else
                    {
                        graphicsCommand.DoJob();
                        graphicsCommand.DoBatchedJob();
                    }
                }
            }

            if (lastBatchedCommand != null)
            {
                lastBatchedCommand.DoBatchedJob();
            }

            for (int i = 0; i < _renderContexts.Count; i++)
            {
                var childRenderContext = _renderContexts[i];
                childRenderContext.Render();
            }
        }

        public RenderContext AddChildRenderContext(int order)
        {
            var renderContext = new RenderContext(IOSystem, _framework);
            _renderContexts.Add(renderContext);

            return renderContext;
        }
    }
}
