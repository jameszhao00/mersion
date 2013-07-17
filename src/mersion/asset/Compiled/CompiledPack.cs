using System.IO;
using ProtoBuf;

namespace asset.Compiled
{
    [ProtoContract]
    public struct CompiledPack
    {
        [ProtoMember(1)]
        public CompiledMesh[] CompiledMeshes
        {
            get;
            set;
        }

        public void Serialize(string path)
        {
            CompilerSetup.Setup();
            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                Serializer.Serialize(fileStream, this);
            }
        }

        public static CompiledPack Deserialize(string path)
        {
            CompilerSetup.Setup();
            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                return Serializer.Deserialize<CompiledPack>(fileStream);
            }
        }
    }
}