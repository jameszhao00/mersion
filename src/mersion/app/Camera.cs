using System;
using SharpDX;

namespace app
{
    public interface ILens
    {
        Matrix ProjectionMatrix();
        float ZNear { get; set; }
        float ZFar { get; set; }
    }
    public struct PerspectiveLens : ILens
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
    public struct OrthographicLens : ILens
    {
        public float Width { get; set; }
        public float Height { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }

        public Matrix ProjectionMatrix()
        {
            return Matrix.OrthoLH(Width, Height, ZNear, ZFar);
        }
    }

    public struct Camera<TLens> where TLens : ILens
    {
        public Transform Transform { get; set; }

        public Camera(Transform transform, TLens lens) : this()
        {
            Transform = transform;
            Lens = lens;
        }

        public Camera<TLens> MoveLocal(Vector3 local)
        {
            return new Camera<TLens>
            {
                Lens = Lens,
                Transform = Transform.MoveLocal(local)
            };
        }

        public Matrix ViewProjectionMatrix()
        {
            return ViewMatrix()*Lens.ProjectionMatrix();
        }
        public Matrix ViewMatrix()
        {
            return Matrix.LookAtLH(Transform.Position, Transform.Position + Transform.Forward, Transform.Up);
        }

        public TLens Lens { get; set; }


        public Camera<TLens> RotateYawPitch(YawPitchRoll dp)
        {
            return new Camera<TLens>
            {
                Lens = Lens,
                Transform = Transform
                    .RotateGlobal(new YawPitchRoll {Yaw = dp.Yaw})
                    .RotateLocal(new YawPitchRoll {Pitch = dp.Pitch})
            };
        }
        public Camera<TLens> LookAt(Vector3 pos)
        {
            var targetForward = Vector3.Normalize(pos - Transform.Position);
            var angle = (float) Math.Acos(MathUtil.Clamp(Vector3.Dot(targetForward, Transform.Forward), 0, 1));
            var newTransform =
                Transform.RotateGlobal(Quaternion.RotationAxis(Vector3.Cross(Transform.Forward, targetForward),
                    angle));
            return new Camera<TLens>
            {
                Transform =
                    newTransform,
                Lens = Lens
            };
        }
    }

    public static class Camera
    {
        public static Camera<PerspectiveLens> DefaultPerspective(int width, int height)
        {
            return new Camera<PerspectiveLens>
            {
                Lens = new PerspectiveLens
                {
                    AspectRatio = (float)width / height,
                    Fov = new Degree(60),
                    ZFar = 10000,
                    ZNear = .1f
                },
                Transform = new Transform
                {
                    Position = new Vector3(0, 0, -1)
                }
            };
        }
    }
}