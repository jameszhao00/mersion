using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using asset;
using asset.Compiled;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DirectInput;
using SharpDX.DXGI;
using SharpDX.Toolkit;
using SharpDX.Toolkit.Graphics;
using Buffer = SharpDX.Toolkit.Graphics.Buffer;
using Color = SharpDX.Color;
using DepthStencilState = SharpDX.Toolkit.Graphics.DepthStencilState;
using Effect = SharpDX.Toolkit.Graphics.Effect;
using Image = SharpDX.Toolkit.Graphics.Image;
using Texture2D = SharpDX.Toolkit.Graphics.Texture2D;

namespace app
{
    public class StepOutput
    {
        public IComponentContainer[] Add { get; set; }
        public IComponentContainer[] Remove { get; set; }
        public IComponentState[] MyAdd { get; set; }
        public IComponentState[] MyRemove { get; set; }
        //maybe also add/remove other containers?
        public IComponentState NextState { get; set; }
    }
    public interface IComponentState
    {
    }
    public interface IComponentProcessor
    {
        StepOutput Step(IComponentState state);
        void ApplyInternal();
    }

    public interface IComponentContainer
    {

    }

    public class RenderComponentState : IComponentState
    {
        public CompiledMesh Mesh { get; set; }
    }

    public class TransformComponentState : IComponentState
    {
        
    }

    public class CameraComponentState : IComponentState
    {
        
    }

    public class ThirdPersonComponentState : IComponentState
    {
        
    }

    public class Head
    {
        public RenderComponentState Render { get; set; }
        public TransformComponentState Transform { get; set; }
        public ThirdPersonComponentState Input { get; set; }
    }

    public class Simulation
    {
        public List<RenderComponentState> RenderComponents { get; private set; }
        public CameraComponentState MainCamera { get; private set; }
    }

    public class Main : Game
    {
        private Camera<PerspectiveLens> _camera;
        private GraphicsDeviceManager _graphicsDeviceManager;
        private Effect _genGBuffer;
        private readonly Keyboard _keyboard;
        private readonly Mouse _mouse;
        private GBuffer _gBuffer;
        private ShadowBuffer _shadowBuffer;
        private static readonly Size PreferredSize = new Size(1800, 900);
        public Main()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                SynchronizeWithVerticalRetrace = false,
                PreferredBackBufferWidth = PreferredSize.Width,
                PreferredBackBufferHeight = PreferredSize.Height,
                PreferredBackBufferFormat = PixelFormat.R8G8B8A8.UNormSRgb,
                //DeviceCreationFlags = DeviceCreationFlags.Debug
            };
            IsFixedTimeStep = false;
            Content.RootDirectory = "Content";
            var directInput = new DirectInput();
            _keyboard = new Keyboard(directInput);
            _keyboard.Acquire();
            _mouse = new Mouse(directInput);
            _mouse.Acquire();
        }

        private GfxMesh[] _sponzaMeshes;

        public Texture2D LoadSrgbTexture(byte[] data)
        {
            var image = Image.Load(new MemoryStream(data));
            if (image.Description.Format == Format.BC1_UNorm)
            {
                Console.WriteLine(@"Changing image format to SRGB!");
                image.Description.Format = Format.BC1_UNorm_SRgb;
            }
            var tex = Texture2D.New(GraphicsDevice, image);
            return tex;
        }

        public CompiledPack LoadObj(string path)
        {
            string packPath = Path.ChangeExtension(Path.GetFileName(path), "pack");
            if (!File.Exists(packPath))
            {
                var assetRoot = new FileInfo(path).Directory.FullName;
                var meshes = Import.LoadAsset(path, assetRoot);
                var compiled = Compiler.Compile(meshes, assetRoot);
                compiled.Serialize(packPath);
            }
            Console.WriteLine(@"Deserializing pack from {0}", packPath);
            return CompiledPack.Deserialize(packPath);
        }

        public GfxMesh[] ToGfxMeshes(CompiledPack pack)
        {
            return pack.CompiledMeshes.Select(x =>
            {
                var vb = Buffer.Vertex.New(GraphicsDevice, x.Vertices);
                var texture = x.Texture.IsSome
                    ? LoadSrgbTexture(x.Texture.Value.Data)
                    : Option<Texture2D>.None;

                return new GfxMesh
                {
                    Vertices = vb,
                    Indices = Buffer.Index.New(GraphicsDevice, x.Indices),
                    InputLayout = VertexInputLayout.FromBuffer(0, vb),
                    Texture = texture
                };
            }).ToArray();
        }
        protected override void LoadContent()
        {
            _genGBuffer = new Effect(GraphicsDevice, VectorUtils.CompileEffect("shaders/genGBuffer.hlsl"));
            _genShadowBuffer = new Effect(GraphicsDevice, VectorUtils.CompileEffect("shaders/genShadowBuffer.hlsl"));
            _shadeGBuffer = new ShadeGBufferEffect(GraphicsDevice);
            _sponzaMeshes = ToGfxMeshes(LoadObj(@"assets/obj/crytek-sponza/sponza.obj"));
            _headMesh = ToGfxMeshes(LoadObj(@"assets/obj/head/head.obj"));
            base.LoadContent();
        }

        protected override void Initialize()
        {
            Window.Title = Environment.CurrentDirectory;
            _camera = Camera.DefaultPerspective(PreferredSize.Width, PreferredSize.Height)
                .MoveLocal(new Vector3(0, 100, -200));
            const int ssaaMultiplier = 2;
            _gBuffer = new GBuffer(GraphicsDevice, PreferredSize.Width, PreferredSize.Height, ssaaMultiplier);
            _shadowBuffer = new ShadowBuffer(GraphicsDevice, 512, 512);

            _shadowDss = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription
            {
                DepthComparison = Comparison.Less,
                IsDepthEnabled = true,
                IsStencilEnabled = false,
                DepthWriteMask = DepthWriteMask.All
            });
            _noShadowDss = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription
            {
                IsDepthEnabled = false,
                IsStencilEnabled = false,
            });
            base.Initialize();
        }

        private static readonly Dictionary<Key, Vector3> KeyMap = new Dictionary<Key, Vector3>
        {
            {Key.W, Vector3.UnitZ},
            {Key.S, -Vector3.UnitZ},
            {Key.A, -Vector3.UnitX},
            {Key.D, Vector3.UnitX}
        };

        private ShadeGBufferEffect _shadeGBuffer;
        private Effect _genShadowBuffer;

        private static readonly DirectionalLight[] Lights =
        {
            new DirectionalLight
            {
                IncidentDirection = Vector3.Normalize(new Vector3(0, -1, 0)),
                Value = new Vector3(1, 1, 1)
            },
            new DirectionalLight
            {
                IncidentDirection = Vector3.Normalize(new Vector3(-0.2f, -1, -0.2f)),
                Value = new Vector3(1, 1, 1)
            },
        };

        private GfxMesh[] _headMesh;
        private DepthStencilState _shadowDss;
        private DepthStencilState _noShadowDss;

        protected override void Update(GameTime gameTime)
        {
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
                    _camera = _camera.MoveLocal(multipler*kv.Value*50*(float) time.TotalSeconds);
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
                        Pitch = new Radian(current.Y/1000.0f),
                        Yaw = new Radian(current.X/1000.0f)
                    });
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            GraphicsDevice.Clear(_gBuffer.Depth, DepthStencilClearFlags.Depth, 1, 0);
            GraphicsDevice.Clear(_gBuffer.Diffuse, Color4.White);
            GraphicsDevice.Clear(_gBuffer.Normal, Color4.Black);
            GraphicsDevice.Clear(_shadowBuffer.Depth, DepthStencilClearFlags.Depth, 1, 0);

            GraphicsDevice.SetDepthStencilState(_shadowDss);

            //GraphicsDevice.SetRenderTargets(_gBuffer.Depth, _gBuffer.Diffuse, _gBuffer.Normal);
            GraphicsDevice.SetRenderTargets(_gBuffer.Depth, _gBuffer.Diffuse, _gBuffer.Normal, _gBuffer.RefWorldPosition);
            GraphicsDevice.SetViewports(new ViewportF(0, 0, _gBuffer.Depth.Width, _gBuffer.Depth.Height));
            foreach (var gfxMesh in _sponzaMeshes)
            {
                if (gfxMesh.Texture.IsSome)
                {
                    _genGBuffer.Parameters["albedo"].SetResource(gfxMesh.Texture.Value);
                }
                else
                {
                    _genGBuffer.Parameters["albedo"].SetResource((Texture2D) null);
                }
                _genGBuffer.Parameters["worldView"].SetValue(_camera.ViewMatrix());
                _genGBuffer.Parameters["worldViewProj"].SetValue(_camera.ViewProjectionMatrix());
                _genGBuffer.CurrentTechnique.Passes[0].Apply();
                gfxMesh.Draw(GraphicsDevice);
            }
            var moveLocal = _camera.Transform
                .MoveLocal(Vector3.UnitZ * 250)
                .RotateGlobal(new YawPitchRoll { Yaw = new Radian((float)Math.PI) })
                .UniformScale(500);
            foreach (var gfxMesh in _headMesh)
            {
                if (gfxMesh.Texture.IsSome)
                {
                    _genGBuffer.Parameters["albedo"].SetResource(gfxMesh.Texture.Value);
                }
                
                _genGBuffer.Parameters["worldView"].SetValue(moveLocal.Matrix() *_camera.ViewMatrix());
                _genGBuffer.Parameters["worldViewProj"].SetValue(moveLocal.Matrix()*
                                                                 _camera.ViewProjectionMatrix());
                _genGBuffer.CurrentTechnique.Passes[0].Apply();
                gfxMesh.Draw(GraphicsDevice);
            }
            _genShadowBuffer.CurrentTechnique.Passes[0].Apply();
            GraphicsDevice.SetRenderTargets(_shadowBuffer.Depth);
            GraphicsDevice.SetViewports(new ViewportF(0, 0, _shadowBuffer.Depth.Width, _shadowBuffer.Depth.Height));
            var sceneCenter = Vector3.Zero;
            const int sceneRadius = 2000;
            foreach (var gfxMesh in _sponzaMeshes)
            {
                _genShadowBuffer.Parameters["worldViewProj"].SetValue(
                    Lights[0].RenderCamera(sceneCenter, sceneRadius)
                        .ViewProjectionMatrix());
                _genShadowBuffer.CurrentTechnique.Passes[0].Apply();
                gfxMesh.Draw(GraphicsDevice);
            } 
            foreach (var gfxMesh in _headMesh)
            {
                _genShadowBuffer.Parameters["worldViewProj"].SetValue(
                    moveLocal.Matrix() *
                    Lights[0].RenderCamera(sceneCenter, sceneRadius)
                        .ViewProjectionMatrix());
                _genShadowBuffer.CurrentTechnique.Passes[0].Apply();
                gfxMesh.Draw(GraphicsDevice);
            }
            GraphicsDevice.SetDepthStencilState(_noShadowDss);
            GraphicsDevice.SetRenderTargets(GraphicsDevice.BackBuffer);
            GraphicsDevice.SetViewports(new ViewportF(0, 0, GraphicsDevice.BackBuffer.Width,
                GraphicsDevice.BackBuffer.Height));
            _shadeGBuffer.Update(_gBuffer,_shadowBuffer, Lights, _camera, sceneRadius, sceneCenter);
            _shadeGBuffer.Apply();
            GraphicsDevice.DrawQuad();
            base.Draw(gameTime);
        }
    }

    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            using (var program = new Main())
            {
                program.Run();
            }
        }
    }
}