using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using SharpDX;
using SharpDX.DirectInput;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using PrimitiveType = SharpDX.Toolkit.Graphics.PrimitiveType;

namespace app
{
    public class Main : Game
    {
        private Camera _camera;
        private GraphicsDeviceManager _graphicsDeviceManager;
        private BasicEffect _basicEffect;
        private readonly Keyboard _keyboard;
        private readonly Mouse _mouse;

        public Main()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                //SynchronizeWithVerticalRetrace = false,
                PreferredBackBufferWidth = 1600,
                PreferredBackBufferHeight = 1200
            };
            //IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
            var directInput = new DirectInput();
            _keyboard = new Keyboard(directInput);
            _keyboard.Acquire();
            _mouse=new Mouse(directInput);
            _mouse.Acquire();
        }

        public static Vector3 Convert(Vector3D v3D)
        {
            return new Vector3(v3D.X, v3D.Y, v3D.Z);
        }

        public static Vector2 ToVec2(Vector3 v3)
        {
            return new Vector2(v3.X, v3.Y);
        }

        public struct AssetMesh
        {
            public VertexPositionColor[] Vertices { get; set; }
            public uint[] Indices { get; set; }
        }

        public struct GfxMesh
        {
            public Buffer<VertexPositionColor> Vertices { get; set; }
            public Buffer<uint> Indices { get; set; }
            public VertexInputLayout InputLayout { get; set; }
        }

        private AssetMesh[] _meshes;
        private GfxMesh[] _gfxMeshes;
        protected override void LoadContent()
        {
            var importer = new AssimpImporter();

            var scene = importer.ImportFile("assets/obj/crytek-sponza/sponza.obj",
                PostProcessSteps.PreTransformVertices 
                | PostProcessSteps.Triangulate);

            var vertices = scene.Meshes.Select(mesh => mesh.Vertices
                .Zip(mesh.Normals,
                    (pos, normalUv) =>
                        new VertexPositionColor(Convert(pos), new Color(normalUv.X, normalUv.Y, normalUv.Z, 1))));
            //,ToVec2(Convert(normalUv.uv) Convert(normalUv.normal
            /*
             * .Zip(mesh.GetTextureCoords(0), (normal, uv) => new
                {
                    normal,
                    uv
                })
             */
            var indices = scene.Meshes.Select(mesh => mesh.GetIndices());

            _meshes = vertices.Zip(indices, (v, i) => new AssetMesh
            {
                Vertices = v.ToArray(),
                Indices = i.ToArray()
            }).ToArray();

            // Creates a basic effect
            _basicEffect = new BasicEffect(GraphicsDevice)
            {
                VertexColorEnabled = true,
                View = Matrix.LookAtLH(new Vector3(0, 0, -5), new Vector3(0, 0, 0), Vector3.UnitY),
                Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f),
                World = Matrix.Identity
            };

            _gfxMeshes = _meshes.Select(x =>
            {
                var vb = Buffer.Vertex.New(GraphicsDevice, x.Vertices);
                return new GfxMesh
                {
                    Vertices = vb,
                    Indices = Buffer.Index.New(GraphicsDevice, x.Indices),
                    InputLayout = VertexInputLayout.FromBuffer(0, vb)
                };
            }).ToArray();

            base.LoadContent();
        }

        protected override void Initialize()
        {
            Window.Title = "MiniCube demo";
            _camera = Camera.Default(Window.ClientBounds.Width, Window.ClientBounds.Height);
            base.Initialize();
        }

        private static readonly Dictionary<Key, Vector3> KeyMap = new Dictionary<Key, Vector3>
        {
            {Key.W, Vector3.UnitZ},
            {Key.S, -Vector3.UnitZ},
            {Key.A, -Vector3.UnitX},
            {Key.D, Vector3.UnitX}
        };
        protected override void Update(GameTime gameTime)
        {
            _basicEffect.View = _camera.ViewMatrix();
            _basicEffect.Projection = _camera.Projection.ProjectionMatrix();

            HandleInput(gameTime.ElapsedGameTime);
            base.Update(gameTime);
        }

        private void HandleInput(TimeSpan time)
        {
            _keyboard.Poll();
            var keyboardState = _keyboard.GetCurrentState();
            var multipler = keyboardState.IsPressed(Key.LeftShift)
                ? 10
                : 1;
            foreach (var kv in KeyMap)
            {
                if (keyboardState.IsPressed(kv.Key))
                {
                    _camera = _camera.MoveLocal(multipler * kv.Value * 50 * (float)time.TotalSeconds);
                }
            }
            _mouse.Poll();
            var state = _mouse.GetCurrentState();
            if (state.Buttons[1])
            {
                var current = new Vector2(state.X, state.Y);
                if (current.LengthSquared() > 0)
                {
                    _camera = _camera.RotateYawPitch(new YawPitchRoll
                    {
                        Pitch = new Radian(current.Y / 1000.0f),
                        Yaw = new Radian(current.X / 1000.0f)
                    });
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            // Clears the screen with the Color.CornflowerBlue
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            foreach (var gfxMesh in _gfxMeshes)
            {
                GraphicsDevice.SetVertexBuffer(gfxMesh.Vertices);
                GraphicsDevice.SetIndexBuffer(gfxMesh.Indices, true);
                GraphicsDevice.SetVertexInputLayout(gfxMesh.InputLayout);
                _basicEffect.CurrentTechnique.Passes[0].Apply();
                GraphicsDevice.DrawIndexed(PrimitiveType.TriangleList, gfxMesh.Indices.ElementCount);
            }

            // Handle base.Draw
            base.Draw(gameTime);
        }
    }
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            using (var program = new Main())
            {
                program.Run();
            }
        }
    }
}
