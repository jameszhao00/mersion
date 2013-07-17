using SharpDX.Toolkit.Graphics;

namespace asset
{
    public struct AssetMesh
    {
        public VertexPositionNormalTexture[] Vertices { get; set; }
        public uint[] Indices { get; set; }
        public Option<AssetTexture> Texture { get; set; }
    }

}