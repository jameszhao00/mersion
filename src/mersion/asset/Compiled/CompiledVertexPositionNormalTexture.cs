using ProtoBuf;
using SharpDX;
using SharpDX.Toolkit.Graphics;

namespace asset.Compiled
{
    [ProtoContract]
    public struct CompiledVertexPositionNormalTexture
    {
        [ProtoMember(1)]
        [VertexElement("SV_Position")]
        public Vector3 Position;
        [ProtoMember(2)]
        [VertexElement("NORMAL")]
        public Vector3 Normal;
        [ProtoMember(3)]
        [VertexElement("TEXCOORD0")]
        public Vector2 TextureCoordinate;
    }
}