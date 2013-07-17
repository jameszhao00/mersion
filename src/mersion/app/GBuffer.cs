using System.Diagnostics;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Toolkit.Graphics;

namespace app
{
    public class GBuffer
    {
        public int SsaaMultiplier { get; set; }

        public GBuffer(GraphicsDevice device, int width, int height, int ssaaMultiplier)
        {
            SsaaMultiplier = ssaaMultiplier;
            var ssaaHeight = height * SsaaMultiplier;
            var ssaaWidth = width * SsaaMultiplier;
            Depth = DepthStencilBuffer.New(device, ssaaWidth, ssaaHeight, DepthFormat.Depth32, true);
            Diffuse = RenderTarget2D.New(device, ssaaWidth, ssaaHeight, PixelFormat.R16G16B16A16.Float);
            Normal = RenderTarget2D.New(device, ssaaWidth, ssaaHeight, PixelFormat.R16G16B16A16.Float);
            RefWorldPosition = RenderTarget2D.New(device, ssaaWidth, ssaaHeight, PixelFormat.R32G32B32A32.Float);
        }
        public RenderTarget2D Diffuse { get; private set; }
        public RenderTarget2D Normal { get; private set; }
        public RenderTarget2D RefWorldPosition { get; private set; }
        public DepthStencilBuffer Depth { get; private set; }

    }
}