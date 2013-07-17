using asset;
using asset.Compiled;
using SharpDX.Toolkit.Graphics;

namespace app
{
    public struct GfxMesh
    {
        public Buffer<CompiledVertexPositionNormalTexture> Vertices { get; set; }
        public Buffer<uint> Indices { get; set; }
        public VertexInputLayout InputLayout { get; set; }
        public Option<Texture2D> Texture { get; set; }

        public void Draw(GraphicsDevice device)
        {
            device.SetVertexBuffer(Vertices);
            device.SetIndexBuffer(Indices, true);
            device.SetVertexInputLayout(InputLayout);
            device.DrawIndexed(PrimitiveType.TriangleList, Indices.ElementCount);
        }
    }
}