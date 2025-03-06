using System;
using NUnit.Framework;
using Unity.Mathematics;

namespace Spark2D.Tests.Core.Generators {
    public class CurveMakerTests {
        private CurveMaker _curveMaker;
        private float2[] _testControlPoints;
        private int _pointCount = 20;

        [SetUp]
        public void Setup() {
            // Create test control points for each test
            _testControlPoints = new float2[] {
                new float2(0, 0),
                new float2(1, 2),
                new float2(3, 1),
                new float2(4, 0)
            };

            _curveMaker = new CurveMaker(_testControlPoints, _pointCount);
        }

        [Test]
        public void CurveMaker_InitializesCorrectly() {
            // Verify constructor properly initializes properties
            Assert.AreEqual(_pointCount, _curveMaker.Count);
            Assert.AreEqual(_testControlPoints.Length, _curveMaker.ControlPoints.Length);

            // Verify properties contain the correct values
            for (int i = 0; i < _testControlPoints.Length; i++) {
                Assert.AreEqual(_testControlPoints[i].x, _curveMaker.ControlPoints[i].x);
                Assert.AreEqual(_testControlPoints[i].y, _curveMaker.ControlPoints[i].y);
            }
        }

        [Test]
        public void Generate_ReturnsCorrectNumberOfPoints() {
            Array resultPoints = _curveMaker.Generate();
            Assert.AreEqual(_pointCount, resultPoints.Length);
        }

        [Test]
        public void Generate_FirstPointMatchesFirstControlPoint() {
            Array resultPoints = _curveMaker.Generate();
            float2 firstPoint = (float2)resultPoints.GetValue(0);

            Assert.AreEqual(_testControlPoints[0].x, firstPoint.x);
            Assert.AreEqual(_testControlPoints[0].y, firstPoint.y);
        }

        [Test]
        public void Generate_LastPointMatchesLastControlPoint() {
            Array resultPoints = _curveMaker.Generate();
            float2 lastPoint = (float2)resultPoints.GetValue(_pointCount - 1);
            float2 lastControlPoint = _testControlPoints[_testControlPoints.Length - 1];

            Assert.AreEqual(lastControlPoint.x, lastPoint.x);
            Assert.AreEqual(lastControlPoint.y, lastPoint.y);
        }

        [Test]
        public void ControlPoints_Setter_ThrowsExceptionForLessThanTwoPoints() {
            // Create an invalid control points array
            float2[] invalidPoints = new float2[] { new float2(0, 0) };

            // Expect exception when setting less than 2 points
            Exception ex = Assert.Throws<Exception>(() => _curveMaker.ControlPoints = invalidPoints);
            Assert.That(ex.Message, Is.EqualTo("Control points must have at least 2 points"));
        }

        [Test]
        public void Count_Setter_ThrowsExceptionForLessThanTwoPoints() {
            // Expect exception when setting count to less than 2
            Exception ex = Assert.Throws<Exception>(() => _curveMaker.Count = 1);
            Assert.That(ex.Message, Is.EqualTo("Count must be at least 2"));
        }

        [Test]
        public void Generate_HandlesMinimumValidInput() {
            // Setup minimum valid case - 2 control points and 2 count
            float2[] minPoints = new float2[] {
                new float2(0, 0),
                new float2(1, 1)
            };

            _curveMaker.ControlPoints = minPoints;
            _curveMaker.Count = 2;

            Array resultPoints = _curveMaker.Generate();

            // Verify we get exactly 2 points
            Assert.AreEqual(2, resultPoints.Length);

            // First point should match first control point
            float2 firstPoint = (float2)resultPoints.GetValue(0);
            Assert.AreEqual(minPoints[0].x, firstPoint.x);
            Assert.AreEqual(minPoints[0].y, firstPoint.y);

            // Last point should match last control point
            float2 lastPoint = (float2)resultPoints.GetValue(1);
            Assert.AreEqual(minPoints[1].x, lastPoint.x);
            Assert.AreEqual(minPoints[1].y, lastPoint.y);
        }
    }
}