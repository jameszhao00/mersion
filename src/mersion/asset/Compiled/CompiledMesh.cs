using ProtoBuf;

namespace asset.Compiled
{
    [ProtoContract]
    public class CompiledMesh
    {
        [ProtoMember(1)]
        public CompiledVertexPositionNormalTexture[] Vertices
        {
            get;
            set;
        }

        [ProtoMember(2)]
        public uint[] Indices
        {
            get;
            set;
        }

        [ProtoMember(3)]
        public Option<CompiledTexture> Texture
        {
            get;
            set;
        }
    }
}