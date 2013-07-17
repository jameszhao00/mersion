using SharpDX;

namespace app
{
    public struct DirectionalLightData
    {
        public Vector4 ViewSpaceIncidentDirection;
        public Vector4 Value;

        public DirectionalLightData(DirectionalLight src, Matrix viewMatrix)
        {
            ViewSpaceIncidentDirection = new Vector4(Vector3.TransformNormal(src.IncidentDirection, viewMatrix), 0);
            Value = new Vector4(src.Value, 1);
        }

        public static int SizeInBytes
        {
            get { return (4 + 4) * 4; }
        }
    }
}