using System.Diagnostics;
using SharpDX;

namespace app
{
    public class DirectionalLight
    {
        public Vector3 IncidentDirection;
        public Vector3 Value;

        public Camera<OrthographicLens> RenderCamera(Vector3 sceneCenter, int sceneRadius)
        {
            var eye = -IncidentDirection*4000;
            var rotation = Quaternion.RotationMatrix(Matrix.Invert(Matrix.LookAtLH(eye, eye + IncidentDirection, Vector3.UnitZ)));
            Debug.Assert(Vector3.NearEqual(Vector3.Transform(Vector3.UnitZ, rotation), IncidentDirection,
                new Vector3(0.0001f)));
            return new Camera<OrthographicLens>
            {
                Transform = new Transform
                {
                    Position = eye,
                    Rotation = rotation
                },
                Lens = new OrthographicLens
                {
                    Width = sceneRadius,
                    Height = sceneRadius,
                    ZFar = 4000,
                    ZNear = 1
                }
            }.LookAt(sceneCenter);
        }
    }
}