using SharpDX.Toolkit.Graphics;

namespace app
{
    public class ShadowBuffer
    {
        public ShadowBuffer(GraphicsDevice device, int width, int height)
        {
            Depth = DepthStencilBuffer.New(device, width, height, DepthFormat.Depth32, true);
        }
        public DepthStencilBuffer Depth { get; private set; }
    }
}