using ProtoBuf.Meta;
using SharpDX;

namespace asset
{
    internal static class CompilerSetup
    {
        private static bool _hasSetup = false;

        internal static void Setup()
        {
            if (!_hasSetup)
            {
                RuntimeTypeModel.Default.Add(typeof (Vector2), false)
                    .Add(1, "X")
                    .Add(2, "Y");
                RuntimeTypeModel.Default.Add(typeof (Vector3), false)
                    .Add(1, "X")
                    .Add(2, "Y")
                    .Add(3, "Z");
            }
            _hasSetup = true;
        }
    }
}