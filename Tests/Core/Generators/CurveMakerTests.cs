using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Spark2D.Tests.Core.Generators {
    public class CurveMakerTests {
        // Basic control points for a simple curve
        private Vector2[] _simpleLinePoints;
        private Vector2[] _quadraticCurvePoints;
        private Vector2[] _cubicCurvePoints;
        private Vector2[] _complexCurvePoints;

        [SetUp]
        public void Setup() {
            // Initialize various test cases
            _simpleLinePoints = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(10, 0)
            };

            _quadraticCurvePoints = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(5, 10),
                new Vector2(10, 0)
            };

            _cubicCurvePoints = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2, 10),
                new Vector2(8, 10),
                new Vector2(10, 0)
            };

            _complexCurvePoints = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(2, 5),
                new Vector2(4, -2),
                new Vector2(6, 5),
                new Vector2(8, -1),
                new Vector2(10, 0)
            };
        }

        #region Initialization Tests
        [Test]
        public void Constructor_WithValidParameters_InitializesCorrectly() {
            // Arrange & Act
            var curveMaker = new CurveMaker(_simpleLinePoints, 10, 0.1f, 0f, PointGenerationMode.Count, true);

            // Assert
            Assert.AreEqual(10, curveMaker.Count);
            Assert.AreEqual(0.1f, curveMaker.Spacing);
            Assert.AreEqual(PointGenerationMode.Count, curveMaker.PointGenerationMode);
            Assert.IsTrue(curveMaker.EvenSpacing);
            Assert.AreEqual(2, curveMaker.ControlPoints.Length);
            Assert.AreEqual(new Vector2(0, 0), curveMaker.ControlPoints[0]);
            Assert.AreEqual(new Vector2(10, 0), curveMaker.ControlPoints[1]);
        }

        [Test]
        public void Constructor_WithDefaults_UsesDefaultValues() {
            // Arrange & Act
            var curveMaker = new CurveMaker(_simpleLinePoints);

            // Assert
            Assert.AreEqual(20, curveMaker.Count); // Default count is 20
            Assert.AreEqual(0.1f, curveMaker.Spacing); // Default spacing is 0.1
            Assert.AreEqual(PointGenerationMode.Count, curveMaker.PointGenerationMode); // Default mode is Count
            Assert.IsTrue(curveMaker.EvenSpacing); // Default is true for even spacing
        }

        [Test]
        public void Constructor_WithInvalidControlPoints_ThrowsException() {
            // Arrange
            var invalidPoints = new[] { new Vector2(0, 0) }; // Only one point

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new CurveMaker(invalidPoints));
        }
        #endregion

        #region Control Points Tests
        [Test]
        public void ControlPoints_WhenChanged_InvalidatesCache() {
            // Arrange
            var curveMaker = new CurveMaker(_simpleLinePoints);

            // Generate points to build the cache
            curveMaker.Generate();
            float initialLength = curveMaker.GetTotalLength();

            // Act - Change control points to a curve with different length
            curveMaker.ControlPoints = _quadraticCurvePoints;

            // Assert
            float newLength = curveMaker.GetTotalLength();
            Assert.AreNotEqual(initialLength, newLength);
        }

        [Test]
        public void ControlPoints_WithInvalidInput_ThrowsException() {
            // Arrange
            var curveMaker = new CurveMaker(_simpleLinePoints);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => curveMaker.ControlPoints = null);
            Assert.Throws<ArgumentException>(() => curveMaker.ControlPoints = new[] { new Vector2(0, 0) });
        }
        #endregion

        #region Generation Mode Tests
        [Test]
        public void Generate_WithCountMode_ProducesCorrectNumberOfPoints() {
            // Arrange
            var curveMaker = new CurveMaker(_cubicCurvePoints);
            curveMaker.Count = 15;
            curveMaker.PointGenerationMode = PointGenerationMode.Count;

            // Act
            curveMaker.Generate();

            // Assert
            Assert.AreEqual(15, curveMaker.OutputPoints.Length);
        }

        [Test]
        public void Generate_WithSpacingMode_ProducesPointsAtCorrectDistance() {
            // Arrange
            var curveMaker = new CurveMaker(_cubicCurvePoints);
            curveMaker.Spacing = 2.0f;
            curveMaker.PointGenerationMode = PointGenerationMode.Spacing;
            curveMaker.EvenSpacing = true;

            // Act
            curveMaker.Generate();

            // Get total length to calculate expected number of points
            float totalLength = curveMaker.GetTotalLength();
            int expectedPoints = Math.Max(2, (int)Math.Ceiling(totalLength / 2.0f) + 1);

            // Assert
            Assert.AreEqual(expectedPoints, curveMaker.OutputPoints.Length);

            // Check spacing between points (with some tolerance for numeric issues)
            for (int i = 1; i < curveMaker.OutputPoints.Length - 1; i++) {
                float distance = Vector2.Distance(curveMaker.OutputPoints[i - 1], curveMaker.OutputPoints[i]);
                Assert.AreEqual(2.0f, distance, 0.1f);
            }
        }

        [Test]
        public void Generate_WhenSwitchingModes_UpdatesOutputPointsCount() {
            // Arrange
            var curveMaker = new CurveMaker(_quadraticCurvePoints);
            curveMaker.Count = 10;
            curveMaker.PointGenerationMode = PointGenerationMode.Count;
            curveMaker.Generate();

            // Act
            curveMaker.PointGenerationMode = PointGenerationMode.Spacing;
            curveMaker.Spacing = 1.0f;
            curveMaker.Generate();

            // Assert
            float totalLength = curveMaker.GetTotalLength();
            int expectedPoints = Math.Max(2, (int)Math.Ceiling(totalLength / 1.0f) + 1);
            Assert.AreEqual(expectedPoints, curveMaker.OutputPoints.Length);
        }
        #endregion

        #region Spacing Type Tests
        [Test]
        public void Generate_WithEvenSpacingTrue_ProducesEvenlyDistributedPoints() {
            // Arrange
            var curveMaker = new CurveMaker(_cubicCurvePoints);
            curveMaker.Count = 10;
            curveMaker.EvenSpacing = true;

            // Act
            curveMaker.Generate();

            // Assert
            var points = curveMaker.OutputPoints;

            // Calculate distances between consecutive points
            float[] distances = new float[points.Length - 1];
            for (int i = 0; i < points.Length - 1; i++) {
                distances[i] = Vector2.Distance(points[i], points[i + 1]);
            }

            // Calculate standard deviation of distances
            float mean = distances.Sum() / distances.Length;
            float variance = distances.Sum(d => (d - mean) * (d - mean)) / distances.Length;
            float stdDev = (float)Math.Sqrt(variance);

            // With even spacing, standard deviation should be close to zero
            Assert.Less(stdDev / mean, 0.05f); // Less than 5% variation
        }

        [Test]
        public void Generate_WithEvenSpacingFalse_ProducesUniformParameterSpacing() {
            // Arrange
            var curveMaker = new CurveMaker(_cubicCurvePoints);
            curveMaker.Count = 5; // Using 5 points for simplicity
            curveMaker.EvenSpacing = false;

            // Act
            curveMaker.Generate();

            // Assert
            var points = curveMaker.OutputPoints;

            // First and last points should match control points
            Assert.AreEqual(_cubicCurvePoints[0], points[0]);
            Assert.AreEqual(_cubicCurvePoints[_cubicCurvePoints.Length - 1], points[points.Length - 1]);

            // For uniform parameter spacing, the key property is that t values are evenly distributed
            // We can verify this by checking that a point calculated directly at t=0.5 matches 
            // the middle point generated by our curve maker
            var middlePointDirect = curveMaker.ComputePoint(0.5f);
            var middlePointGenerated = points[points.Length / 2];

            Assert.AreEqual(middlePointDirect.x, middlePointGenerated.x, 0.01f);
            Assert.AreEqual(middlePointDirect.y, middlePointGenerated.y, 0.01f);
        }
        #endregion

        #region Curve Properties Tests
        [Test]
        public void GetTotalLength_WithStraightLine_ReturnsCorrectLength() {
            // Arrange
            var curveMaker = new CurveMaker(_simpleLinePoints);

            // Act
            float length = curveMaker.GetTotalLength();

            // Assert
            Assert.AreEqual(10.0f, length, 0.1f); // Allow small tolerance for numeric precision
        }

        [Test]
        public void GetTotalLength_WithCurve_GivesConsistentResultsAfterRegeneration() {
            // Arrange
            var curveMaker = new CurveMaker(_quadraticCurvePoints);

            // Act
            float length1 = curveMaker.GetTotalLength();
            curveMaker.Generate();
            float length2 = curveMaker.GetTotalLength();

            // Assert
            Assert.AreEqual(length1, length2);
        }

        [Test]
        public void ComputePoint_WithParameterT_ReturnsCorrectPointOnCurve() {
            // Arrange
            var curveMaker = new CurveMaker(_simpleLinePoints);

            // Act
            var midPoint = curveMaker.ComputePoint(0.5f);

            // Assert
            Assert.AreEqual(5.0f, midPoint.x); // 50% along the x-axis
            Assert.AreEqual(0.0f, midPoint.y); // y should remain 0 for straight horizontal line
        }
        #endregion

        #region Complex Usage Examples
        [Test]
        public void Example_GeneratingEvenlySpacedPointsForPath() {
            // This test demonstrates how to generate evenly spaced points for a path

            // Arrange - Create a complex path with control points
            var curveMaker = new CurveMaker(_complexCurvePoints);

            // Configure for even spacing with 0.5 units between points
            curveMaker.PointGenerationMode = PointGenerationMode.Spacing;
            curveMaker.Spacing = 0.5f;
            curveMaker.EvenSpacing = true;

            // Act - Generate points
            curveMaker.Generate();

            // Assert - Check that we have an appropriate number of points
            float totalLength = curveMaker.GetTotalLength();
            int expectedPoints = Math.Max(2, (int)Math.Ceiling(totalLength / 0.5f) + 1);
            Assert.AreEqual(expectedPoints, curveMaker.OutputPoints.Length);

            // Check first and last points match control points
            Assert.AreEqual(_complexCurvePoints[0], curveMaker.OutputPoints[0]);
            Assert.AreEqual(_complexCurvePoints[_complexCurvePoints.Length - 1],
                curveMaker.OutputPoints[curveMaker.OutputPoints.Length - 1]);
        }

        [Test]
        public void Example_CreatingAnimationKeyframesWithUniformParameter() {
            // This test demonstrates generating points with uniform parameter spacing
            // which is useful for animation keyframes

            // Arrange - Create a curve for an animation path
            var curveMaker = new CurveMaker(_cubicCurvePoints);

            // Configure for count-based generation with non-uniform spacing
            curveMaker.PointGenerationMode = PointGenerationMode.Count;
            curveMaker.Count = 30; // 30 animation frames
            curveMaker.EvenSpacing = false; // Uniform parameter (better for certain animations)

            // Act - Generate animation keyframes
            curveMaker.Generate();

            // Assert - Verify we have exactly the requested number of points
            Assert.AreEqual(30, curveMaker.OutputPoints.Length);

            // Verify start and end points
            Assert.AreEqual(_cubicCurvePoints[0], curveMaker.OutputPoints[0]);
            Assert.AreEqual(_cubicCurvePoints[_cubicCurvePoints.Length - 1],
                curveMaker.OutputPoints[curveMaker.OutputPoints.Length - 1]);
        }

        [Test]
        public void Example_ChangeControlPointsDynamically() {
            // This test demonstrates dynamically changing control points

            // Arrange - Start with a simple line
            var curveMaker = new CurveMaker(_simpleLinePoints);
            curveMaker.Count = 10;
            curveMaker.Generate();

            // Remember the initial points
            var initialPoints = new Vector2[curveMaker.OutputPoints.Length];
            for (int i = 0; i < curveMaker.OutputPoints.Length; i++) {
                initialPoints[i] = curveMaker.OutputPoints[i];
            }

            // Act - Change control points to a curve
            curveMaker.ControlPoints = _quadraticCurvePoints;
            curveMaker.Generate();

            // Assert - Verify points have changed
            bool allPointsSame = true;
            for (int i = 0; i < curveMaker.OutputPoints.Length; i++) {
                if (curveMaker.OutputPoints[i] != initialPoints[i]) {
                    allPointsSame = false;
                    break;
                }
            }

            Assert.IsFalse(allPointsSame);
        }
        #endregion

        #region Jitter Tests
        [Test]
        public void Generate_WithJitter_DisplacesPointsWithinRadius() {
            // Arrange
            float jitterRadius = 0.5f;
            var curveMaker = new CurveMaker(_cubicCurvePoints, 20, 0.1f, jitterRadius, PointGenerationMode.Count, true);

            // Generate a set of points without jitter for comparison
            var nojitterCurveMaker = new CurveMaker(_cubicCurvePoints, 20, 0.1f, 0f, PointGenerationMode.Count, true);
            nojitterCurveMaker.Generate();
            var baselinePoints = nojitterCurveMaker.OutputPoints;

            // Act
            curveMaker.Generate();
            var jitteredPoints = curveMaker.OutputPoints;

            // Assert

            // First and last points should remain unaffected (endpoints preserved)
            Assert.AreEqual(baselinePoints[0], jitteredPoints[0], "First point should not be jittered");
            Assert.AreEqual(baselinePoints[baselinePoints.Length - 1], jitteredPoints[jitteredPoints.Length - 1],
                "Last point should not be jittered");

            // Check that middle points are displaced
            bool anyPointJittered = false;
            for (int i = 1; i < jitteredPoints.Length - 1; i++) {
                float distance = Vector2.Distance(baselinePoints[i], jitteredPoints[i]);

                // Each point should be displaced by no more than the jitter radius
                Assert.LessOrEqual(distance, jitterRadius,
                    $"Point {i} was displaced by {distance}, which exceeds jitter radius of {jitterRadius}");

                // At least some points should be displaced (though technically this could randomly fail)
                if (distance > 0.01f) {
                    anyPointJittered = true;
                }
            }

            // Ensure jitter was actually applied (at least one point was visibly displaced)
            Assert.IsTrue(anyPointJittered, "No points appear to have been jittered");
        }

        [Test]
        public void Generate_WithZeroJitter_ProducesIdenticalPointsToNoJitter() {
            // Arrange
            var curveMaker = new CurveMaker(_quadraticCurvePoints, 15, 0.1f, 0f, PointGenerationMode.Count, true);

            // Act
            curveMaker.Generate();
            var initialPoints = curveMaker.OutputPoints.Select(p => p).ToArray(); // Make a copy

            // Set jitter to zero and regenerate
            curveMaker.Jitter = 0f;
            curveMaker.Generate();

            // Assert
            for (int i = 0; i < initialPoints.Length; i++) {
                Assert.AreEqual(initialPoints[i], curveMaker.OutputPoints[i],
                    $"Point {i} should be identical when jitter is zero");
            }
        }
        #endregion
    }
}