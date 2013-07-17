using ProtoBuf;

namespace asset.Compiled
{
    [ProtoContract]
    public class CompiledTexture
    {
        [ProtoMember(1)]
        public byte[] Data
        {
            get;
            set;
        }
    }
}