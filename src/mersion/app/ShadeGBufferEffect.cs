using System.Linq;
using asset;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace app
{
    public class ShadeGBufferEffect
    {
        public ShadeGBufferEffect(GraphicsDevice device)
        {
            Effect = new Effect(device, VectorUtils.CompileEffect("shaders/shadeGBuffer.hlsl"));
        }
        public Effect Effect { get; private set; }

        public void Apply()
        {
            Effect.CurrentTechnique.Passes[0].Apply();
        }

        public void Update<T>(GBuffer gbuffer, ShadowBuffer shadowBuffer, DirectionalLight[] lights, Camera<T> camera, int sceneRadius, Vector3 sceneCenter)
            where T : ILens
        {
            //TOOD: split the update out so we can pipeline it?
            Effect.Parameters["diffuse"].SetResource(gbuffer.Diffuse);
            Effect.Parameters["normal"].SetResource(gbuffer.Normal);
            Effect.Parameters["depth"].SetResource(gbuffer.Depth);
            Effect.Parameters["shadowMap"].SetResource(shadowBuffer.Depth);
            //Effect.Parameters["ref_pos"].SetResource(gbuffer.RefWorldPosition);

            Effect.Parameters["invProj"].SetValue(Matrix.Invert(camera.Lens.ProjectionMatrix()));
            Effect.Parameters["invViewProj"].SetValue(Matrix.Invert(camera.ViewProjectionMatrix()));
            Effect.Parameters["lightWorldView"].SetValue(lights[0].RenderCamera(sceneCenter, sceneRadius).ViewMatrix());
            Effect.Parameters["lightWorldViewProj"].SetValue(lights[0].RenderCamera(sceneCenter, sceneRadius).ViewProjectionMatrix());

            Effect.Parameters["ssaaMultiplier"].SetValue(gbuffer.SsaaMultiplier);
            Effect.Parameters["numLights"].SetValue(lights.Length);
            var lightDatas = lights.Select(x => new DirectionalLightData(x, camera.ViewMatrix())).ToArray();
            Effect.Parameters["directionalLights"].SetValue(lightDatas);
            var lightsCBuffer = Effect.ConstantBuffers["lights"];
            lightsCBuffer.Update();
        }
    }
}