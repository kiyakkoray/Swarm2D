/******************************************************************************
Copyright (c) 2015 Koray Kiyakoglu

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
using Swarm2D.Engine.Core;
using Swarm2D.Engine.Logic;
using Swarm2D.Engine.View;
using Swarm2D.Game.Components;
using Swarm2D.Library;

namespace Swarm2D.Game
{
    public class SceneExtensions
    {
        internal static void CreateCharacters(Scene scene)
        {
            PhysicsMaterial physicsMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
            physicsMaterial.Density = 5.0f;
            physicsMaterial.Restutition = 0.0f;

            {
                Entity characterObject = scene.CreateChildEntity("Character1");

                characterObject.GetComponent<SceneEntity>().LocalPosition = new Vector2(0.0f, -260.0f);
                //characterObject->AssignTransform(SxBase::Vector2(300.0f, 0.0f), 0.0f);

                SpriteRenderer spriteRenderer = characterObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("ball");

                Component testComponent = characterObject.AddComponent("CharacterController");

                BoxShapeFilter boxShapeFilter = characterObject.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 30;
                boxShapeFilter.Height = 30;

                //characterObject.AddComponent<DebugRenderer>();
                PhysicsObject rigidBody = characterObject.AddComponent<PhysicsObject>();
                rigidBody.Material = physicsMaterial;

                characterObject.AddComponent("Character");
            }
        }

        internal static void Reset1(Scene scene)
        {
            {
                Entity backgroundObject = scene.CreateChildEntity("BackgroundObject");

                SpriteRenderer spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("bg02");
            }

            PhysicsMaterial physicsMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
            physicsMaterial.Density = 5.0f;
            physicsMaterial.Restutition = 0.0f;

            {
                Entity cameraObject = scene.CreateChildEntity("Camera");
                //cameraObject->AssignTransform(SxBase::Vector2(0.0f, 0.0f), 0.0f);
                Camera camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent("CharacterCamera");
            }

            {
                for (int i = 0; i < 10; i++)
                {
                    int startValue = -i / 2;
                    int endValue = i / 2;

                    for (int j = startValue; j < endValue; j++)
                    {
                        Entity dynamicBox = scene.CreateChildEntity("dynamicBox");
                        dynamicBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(j * 60.0f, i * 70.0f - 300);

                        dynamicBox.AddComponent<DebugRenderer>();
                        AddRandomShape(dynamicBox);

                        PhysicsObject rigidBody = dynamicBox.AddComponent<PhysicsObject>();
                        rigidBody.Material = physicsMaterial;
                    }
                }
            }

            {
                Entity rotatingBox = scene.CreateChildEntity("rotatingBox");
                rotatingBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(100.0f, -300);

                rotatingBox.AddComponent<DebugRenderer>();
                AddCircle(rotatingBox);
                PhysicsObject rigidBody = rotatingBox.AddComponent<PhysicsObject>();
                rigidBody.Material = physicsMaterial;

                rotatingBox.AddComponent<ObjectRotater>();
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();
                
                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 1030;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, 400.0f);
                //staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                for (int i = -10; i <= 10; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(i * 50.0f, -400.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(-500, i * 50.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(500, i * 50.0f);
                }
            }

            CreateCharacters(scene);
        }

        internal static void Reset2(Scene scene)
        {
            {
                Entity backgroundObject = scene.CreateChildEntity("BackgroundObject");

                SpriteRenderer spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("bg02");
            }

            PhysicsMaterial physicsMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
            physicsMaterial.Density = 5.0f;
            physicsMaterial.Restutition = 0.0f;

            {
                Entity cameraObject = scene.CreateChildEntity("Camera");
                //cameraObject->AssignTransform(SxBase::Vector2(0.0f, 0.0f), 0.0f);
                Camera camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent("CharacterCamera");
            }

            {
                for (int i = 0; i < 10; i++)
                {
                    int startValue = -i / 2;
                    int endValue = i / 2;

                    for (int j = startValue; j < endValue; j++)
                    {
                        Entity dynamicBox = scene.CreateChildEntity("dynamicBox");
                        dynamicBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(j * 64.0f, i * 70.0f - 300);

                        BoxShapeFilter boxShapeFilter = dynamicBox.AddComponent<BoxShapeFilter>();
                        boxShapeFilter.Width = 64;
                        boxShapeFilter.Height = 64;

                        //dynamicBox.AddComponent<DebugRenderer>();
                        PhysicsObject rigidBody = dynamicBox.AddComponent<PhysicsObject>();
                        rigidBody.Material = physicsMaterial;

                        SpriteRenderer spriteRenderer = dynamicBox.AddComponent<SpriteRenderer>();
                        spriteRenderer.Sprite = Resource.GetResource<Sprite>("box01");
                    }
                }
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 1030;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, 400.0f);
                //staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                for (int i = -10; i <= 10; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(i * 50.0f, -400.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(-500, i * 50.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(500, i * 50.0f);
                }
            }

            CreateCharacters(scene);
        }

        internal static void Reset3(Scene scene)
        {
            {
                Entity backgroundObject = scene.CreateChildEntity("BackgroundObject");

                SpriteRenderer spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("bg1");
            }

            PhysicsMaterial physicsMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
            physicsMaterial.Density = 5.0f;
            physicsMaterial.Restutition = 0.0f;

            {
                Entity cameraObject = scene.CreateChildEntity("Camera");
                //cameraObject->AssignTransform(SxBase::Vector2(0.0f, 0.0f), 0.0f);
                Camera camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent("CharacterCamera");
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                const float length = 400.0f;

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = length;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, -400.0f);
                staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                const float length = 400.0f;

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = length;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(100, -100.0f);
                staticBox.GetComponent<SceneEntity>().LocalRotation = -45.0f;
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                const float length = 400.0f;

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = length;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, 200.0f);
                staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 1303;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, 400.0f);
                //staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                for (int i = -10; i <= 10; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("topStaticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(i * 50.0f, -800.0f);
                }

                for (int i = -15; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("leftStaticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(-500, i * 50.0f);
                }

                for (int i = -15; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("rightStaticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(500, i * 50.0f);
                }
            }

            {
                Entity dynamicBox = scene.CreateChildEntity("Character1");
                dynamicBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(-110, -600.0f);

                dynamicBox.AddComponent<DebugRenderer>();
                AddCircle(dynamicBox);
                PhysicsObject rigidBody = dynamicBox.AddComponent<PhysicsObject>();
                rigidBody.Material = physicsMaterial;
            }
        }

        internal static void Reset4(Scene scene)
        {
            {
                Entity backgroundObject = scene.CreateChildEntity("BackgroundObject");

                SpriteRenderer spriteRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("bg1");
            }

            PhysicsMaterial physicsMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
            physicsMaterial.Density = 5.0f;
            physicsMaterial.Restutition = 0.0f;

            {
                Entity cameraObject = scene.CreateChildEntity("Camera");
                //cameraObject->AssignTransform(SxBase::Vector2(0.0f, 0.0f), 0.0f);
                Camera camera = cameraObject.AddComponent<Camera>();
                cameraObject.AddComponent("CharacterCamera");
            }

            {
                Entity staticBox = scene.CreateChildEntity("bottomBox");

                staticBox.AddComponent<DebugRenderer>();

                BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 1030;
                boxShapeFilter.Height = 30;

                PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();

                rigidBody.Material = physicsMaterial;
                rigidBody.Type = PhysicsObject.PhysicsType.Static;

                staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(0, 400.0f);
                //staticBox.GetComponent<SceneEntity>().LocalRotation = 45.0f;
            }

            {
                for (int i = -10; i <= 10; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(i * 50.0f, -400.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(-500, i * 50.0f);
                }

                for (int i = -7; i < 8; i++)
                {
                    Entity staticBox = scene.CreateChildEntity("staticBox");

                    BoxShapeFilter boxShapeFilter = staticBox.AddComponent<BoxShapeFilter>();
                    boxShapeFilter.Width = 30;
                    boxShapeFilter.Height = 30;

                    staticBox.AddComponent<DebugRenderer>();
                    PhysicsObject rigidBody = staticBox.AddComponent<PhysicsObject>();
                    rigidBody.Material = physicsMaterial;
                    rigidBody.Type = PhysicsObject.PhysicsType.Static;

                    staticBox.GetComponent<SceneEntity>().LocalPosition = new Vector2(500, i * 50.0f);
                }
            }

            {
                PhysicsMaterial ballMaterial = new PhysicsMaterial(Resource.GenerateName<PhysicsMaterial>());
                ballMaterial.Density = 5.0f;
                ballMaterial.Restutition = 0.8f;

                Entity characterObject = scene.CreateChildEntity("Character1");

                characterObject.GetComponent<SceneEntity>().LocalPosition = new Vector2(0.0f, -260.0f);
                //characterObject->AssignTransform(SxBase::Vector2(300.0f, 0.0f), 0.0f);

                BoxShapeFilter boxShapeFilter = characterObject.AddComponent<BoxShapeFilter>();
                boxShapeFilter.Width = 30.0f;
                boxShapeFilter.Height = 30.0f;

                SpriteRenderer spriteRenderer = characterObject.AddComponent<SpriteRenderer>();
                spriteRenderer.Sprite = Resource.GetResource<Sprite>("ball");

                PhysicsObject rigidBody = characterObject.AddComponent<PhysicsObject>();
                rigidBody.Material = ballMaterial;
            }
        }

        private static void AddCube(Entity entity)
        {
            BoxShapeFilter boxShapeFilter = entity.AddComponent<BoxShapeFilter>();
            boxShapeFilter.Width = 30;
            boxShapeFilter.Height = 30;
        }

        private static void AddBox(Entity entity)
        {
            BoxShapeFilter boxShapeFilter = entity.AddComponent<BoxShapeFilter>();
            boxShapeFilter.Width = 60;
            boxShapeFilter.Height = 30;
        }

        private static void AddRoche(Entity entity)
        {
            Polygon polygon = new Polygon(Resource.GenerateName<Polygon>());

            polygon.Vertices.Add(new Vector2(15, 15));
            polygon.Vertices.Add(new Vector2(25, 0));
            polygon.Vertices.Add(new Vector2(15, -15));
            polygon.Vertices.Add(new Vector2(-15, -15));
            polygon.Vertices.Add(new Vector2(-25, 0));
            polygon.Vertices.Add(new Vector2(-15, 15));

            ResourceShapeFilter resourceShapeFilter = entity.AddComponent<ResourceShapeFilter>();

            resourceShapeFilter.Shape = polygon;
        }

        private static void AddCircle(Entity entity)
        {
            CircleShapeFilter circleShapeFilter = entity.AddComponent<CircleShapeFilter>();
            circleShapeFilter.Radius = 15;
        }

        private static void AddRandomShape(Entity entity)
        {
            int shapeNo = Swarm2D.Library.Random.Range(0, 4);

            if (shapeNo == 0)
            {
                AddCube(entity);
            }
            else if (shapeNo == 1)
            {
                AddBox(entity);
            }
            else if (shapeNo == 2)
            {
                AddRoche(entity);
            }
            else if (shapeNo == 3)
            {
                AddCircle(entity);
            }
        }
    }
}