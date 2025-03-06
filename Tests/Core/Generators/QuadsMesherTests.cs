using System;
using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.TestTools;

namespace Spark2D.Tests.Core.Generators {
    public class QuadsMesherTests {
        private QuadsMesher _quadsMesher;
        private float2[] _testPositions;
        private float[] _testRotations;
        private float[] _testScales;
        private float4[] _testColors;
        private int _capacity = 10;

        [SetUp]
        public void Setup() {
            // Create test positions for each test
            _testPositions = new float2[] {
                new float2(0, 0),
                new float2(1, 1),
                new float2(2, 0),
                new float2(3, 1),
                new float2(4, 0)
            };

            // Create test rotations
            _testRotations = new float[] {
                0, 30, 60, 90, 120
            };

            // Create test scales
            _testScales = new float[] {
                1, 1.5f, 0.8f, 2f, 0.5f
            };

            // Create test colors
            _testColors = new float4[] {
                new float4(1, 0, 0, 1), // Red
                new float4(0, 1, 0, 1), // Green
                new float4(0, 0, 1, 1), // Blue
                new float4(1, 1, 0, 1), // Yellow
                new float4(1, 0, 1, 1) // Magenta
            };

            // Create a new QuadsMesher with the test data
            _quadsMesher = new QuadsMesher(_capacity, _testPositions, _testRotations, _testScales, _testColors);
        }

        [TearDown]
        public void Teardown() {
            // Clean up by destroying the mesh
            if (_quadsMesher != null && _quadsMesher.Mesh != null) {
                UnityEngine.Object.DestroyImmediate(_quadsMesher.Mesh);
            }
        }

        [Test]
        public void QuadsMesher_InitializesCorrectly() {
            // Verify the Mesh is created
            Assert.IsNotNull(_quadsMesher.Mesh);

            // Verify proper initialization of properties
            Assert.AreEqual(_testPositions, _quadsMesher.Positions);
            Assert.AreEqual(_testRotations, _quadsMesher.Rotations);
            Assert.AreEqual(_testScales, _quadsMesher.Scales);
            Assert.AreEqual(_testColors, _quadsMesher.Colors);

            // Verify mesh has correct vertex count (4 vertices per quad * capacity)
            Assert.AreEqual(_capacity * 4, _quadsMesher.Mesh.vertices.Length);

            // Verify mesh has correct triangle count (6 indices per quad * capacity)
            Assert.AreEqual(_capacity * 6, _quadsMesher.Mesh.triangles.Length);
        }

        [Test]
        public void Generate_CreatesCorrectNumberOfQuads() {
            // Generate the mesh
            _quadsMesher.Generate();

            // The number of active quads should be the minimum of capacity and positions length
            int expectedActiveQuads = Mathf.Min(_capacity, _testPositions.Length);
            int activeVertices = 0;

            // Count non-zero vertices (active vertices)
            foreach (Vector3 vertex in _quadsMesher.Mesh.vertices) {
                if (vertex != Vector3.zero) {
                    activeVertices++;
                }
            }

            // Each quad has 4 vertices
            Assert.AreEqual(expectedActiveQuads * 4, activeVertices);
        }

        [Test]
        public void Generate_AppliesCorrectTransformations() {
            // Generate the mesh
            _quadsMesher.Generate();

            // Check the first quad (4 vertices)
            Vector3[] vertices = _quadsMesher.Mesh.vertices;

            // Get the expected position, rotation, and scale for the first quad
            float2 position = _testPositions[0];
            float rotation = _testRotations[0] * Mathf.Deg2Rad;
            float scale = _testScales[0];

            // Calculate expected corner positions after transformation
            float halfSize = 0.5f * scale;
            float sin = Mathf.Sin(rotation);
            float cos = Mathf.Cos(rotation);

            // Bottom-left corner calculation
            float2 bl = new float2(-halfSize, -halfSize);
            bl = new float2(bl.x * cos - bl.y * sin, bl.x * sin + bl.y * cos);
            Vector3 expectedBL = new Vector3(position.x + bl.x, position.y + bl.y, 0);

            // Check if the actual vertex position matches the expected
            Assert.AreEqual(expectedBL, vertices[0], "Bottom-left vertex position does not match expected transformation");
        }

        [Test]
        public void Generate_AppliesCorrectColors() {
            // Generate the mesh
            _quadsMesher.Generate();

            // Check if the colors were applied correctly
            Color[] colors = _quadsMesher.Mesh.colors;
            Assert.IsNotNull(colors, "Colors array should not be null");

            // Get the expected color for the first quad
            float4 expectedColor = _testColors[0];
            Color expected = new Color(expectedColor.x, expectedColor.y, expectedColor.z, expectedColor.w);

            // All four vertices of the first quad should have the same color
            Assert.AreEqual(expected, colors[0], "Vertex 0 color does not match expected");
            Assert.AreEqual(expected, colors[1], "Vertex 1 color does not match expected");
            Assert.AreEqual(expected, colors[2], "Vertex 2 color does not match expected");
            Assert.AreEqual(expected, colors[3], "Vertex 3 color does not match expected");
        }

        [Test]
        public void Constructor_ThrowsExceptionForInvalidCapacity() {
            // Test invalid capacity (0)
            Exception ex = Assert.Throws<Exception>(() => new QuadsMesher(0, _testPositions));
            Assert.That(ex.Message, Is.EqualTo("Capacity must be greater than 0"));

            // Test invalid capacity (negative)
            ex = Assert.Throws<Exception>(() => new QuadsMesher(-1, _testPositions));
            Assert.That(ex.Message, Is.EqualTo("Capacity must be greater than 0"));
        }

        [Test]
        public void Generate_HandlesNullRotations() {
            // Create a mesher with null rotations
            var mesher = new QuadsMesher(_capacity, _testPositions, null, _testScales, _testColors);

            // Should not throw an exception
            Assert.DoesNotThrow(() => mesher.Generate());

            // Cleanup
            UnityEngine.Object.DestroyImmediate(mesher.Mesh);
        }

        [Test]
        public void Generate_HandlesNullScales() {
            // Create a mesher with null scales
            var mesher = new QuadsMesher(_capacity, _testPositions, _testRotations, null, _testColors);

            // Should not throw an exception
            Assert.DoesNotThrow(() => mesher.Generate());

            // Cleanup
            UnityEngine.Object.DestroyImmediate(mesher.Mesh);
        }

        [Test]
        public void Generate_HandlesNullColors() {
            // Create a mesher with null colors
            var mesher = new QuadsMesher(_capacity, _testPositions, _testRotations, _testScales, null);

            // Should not throw an exception
            Assert.DoesNotThrow(() => mesher.Generate());

            // Cleanup
            UnityEngine.Object.DestroyImmediate(mesher.Mesh);
        }

        [Test]
        public void Generate_HandlesSmallerArrays() {
            // Create arrays with fewer elements than positions
            float[] shortRotations = new float[] { 45 };
            float[] shortScales = new float[] { 2 };
            float4[] shortColors = new float4[] { new float4(1, 1, 1, 1) };

            // Create a mesher with shorter arrays
            var mesher = new QuadsMesher(_capacity, _testPositions, shortRotations, shortScales, shortColors);

            // Should not throw an exception
            Assert.DoesNotThrow(() => mesher.Generate());

            // Cleanup
            UnityEngine.Object.DestroyImmediate(mesher.Mesh);
        }

        [Test]
        public void Generate_HandlesCapacityLargerThanPositions() {
            // Create a mesher with capacity larger than positions array
            var largeCapacity = _testPositions.Length * 2;
            var mesher = new QuadsMesher(largeCapacity, _testPositions, _testRotations, _testScales, _testColors);

            // Generate mesh
            mesher.Generate();

            // Count non-zero vertices
            int activeVertices = 0;
            foreach (Vector3 vertex in mesher.Mesh.vertices) {
                if (vertex != Vector3.zero) {
                    activeVertices++;
                }
            }

            // Should have vertices equal to positions length * 4
            Assert.AreEqual(_testPositions.Length * 4, activeVertices);

            // Cleanup
            UnityEngine.Object.DestroyImmediate(mesher.Mesh);
        }
    }
}