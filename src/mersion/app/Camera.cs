using SharpDX;

namespace app
{
    public struct Camera
    {
        public Transform Transform { get; private set; }

        public Matrix ViewMatrix()
        {
            return Matrix.LookAtLH(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
        }

        public Camera MoveLocal(Vector3 local)
        {
            return new Camera
            {
                Projection = Projection,
                Transform = Transform.MoveLocal(local)
            };
        }
        public struct ProjectionParameters
        {
            public float ZNear { get; set; }
            public float ZFar { get; set; }
            public float AspectRatio { get; set; }
            public Degree Fov { get; set; }

            public Matrix ProjectionMatrix()
            {
                return Matrix.PerspectiveFovLH(Fov.Radian().Value, AspectRatio, ZNear, ZFar);
            }
        }
        public ProjectionParameters Projection { get; private set; }

        public static Camera Default(int width, int height)
        {
            return new Camera
            {
                Projection = new ProjectionParameters
                {
                    AspectRatio = (float) width/height,
                    Fov = new Degree(60),
                    ZFar = 10000,
                    ZNear = 1
                },
                Transform = new Transform()
                {
                    Position = new Vector3(0,0,-10)
                }
            };
        }

        public Camera RotateYawPitch(YawPitchRoll dp)
        {
            return new Camera
            {
                Projection = Projection,
                Transform = Transform
                    .RotateGlobal(new YawPitchRoll {Yaw = dp.Yaw})
                    .RotateLocal(new YawPitchRoll {Pitch = dp.Pitch})
            };
        }
    }
}