using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using app;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;

namespace tests
{
    [TestClass]
    public class TransformTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var t = new Transform
            {
                Position = new Vector3(1, 2, 3),
                Rotation = Quaternion.Identity
            };
            Assert.AreEqual(t.Forward, new Vector3(0,0,1));
            Assert.AreEqual(t.Up, new Vector3(0,1,0));
            Assert.AreEqual(t.MoveLocal(new Vector3(0,0,5)).Position, new Vector3(1,2,8));
            var newTransform = t.RotateLocal(new YawPitchRoll {Yaw = new Radian((float) (Math.PI/2.0))})
                .MoveLocal(new Vector3(0, 0, 5));
            Assert.AreEqual(newTransform.Position, new Vector3(6, 2, 3));
        }
    }
}
