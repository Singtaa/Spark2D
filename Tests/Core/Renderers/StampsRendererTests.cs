using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using System.IO;

namespace Spark2D.Tests.Core.Renderers {
    public class StampsRendererTests {
        // Test resources
        Mesh _testMesh;
        Texture2D _testTexture;
        GameObject _testObject;

        [SetUp]
        public void Setup() {
            // Create a test GameObject
            _testObject = new GameObject("TestObject");

            // Create a simple quad mesh for testing
            _testMesh = new Mesh();
            _testMesh.vertices = new Vector3[] {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0)
            };
            _testMesh.uv = new Vector2[] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            _testMesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
            _testMesh.RecalculateBounds();

            // Create a simple test texture (white circle on transparent background)
            _testTexture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            Color[] colors = new Color[128 * 128];
            for (int y = 0; y < 128; y++) {
                for (int x = 0; x < 128; x++) {
                    float dx = x - 64;
                    float dy = y - 64;
                    float distSq = dx * dx + dy * dy;
                    if (distSq < 40 * 40) {
                        colors[y * 128 + x] = Color.white;
                    } else {
                        colors[y * 128 + x] = Color.clear;
                    }
                }
            }
            _testTexture.SetPixels(colors);
            _testTexture.Apply();
        }

        [TearDown]
        public void Teardown() {
            // Clean up test objects
            Object.DestroyImmediate(_testTexture);
            Object.DestroyImmediate(_testMesh);
            Object.DestroyImmediate(_testObject);
        }

        [Test]
        public void Constructor_ValidInputs_InitializesCorrectly() {
            // Arrange & Act
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);

            // Assert
            Assert.IsNotNull(renderer);
            Assert.AreEqual(_testMesh, renderer.Mesh);
            Assert.AreEqual(_testTexture, renderer.StampTexture);
        }

        [Test]
        public void SetupRenderTextures_ValidParameters_CreatesRenderTexture() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);

            // Act
            renderer.SetupRenderTextures(256, 256);

            // Assert
            Assert.IsNotNull(renderer.RenderTexture);
            Assert.AreEqual(256, renderer.RenderTexture.width);
            Assert.AreEqual(256, renderer.RenderTexture.height);
            Assert.IsTrue(renderer.RenderTexture.IsCreated());

            // Cleanup
            renderer.Dispose();
        }

        [Test]
        public void Properties_SetAndGet_WorksCorrectly() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);

            // Act & Assert - Test each property

            // Mesh property
            Mesh newMesh = new Mesh();
            renderer.Mesh = newMesh;
            Assert.AreEqual(newMesh, renderer.Mesh);

            // StampTexture property
            Texture2D newTexture = new Texture2D(64, 64);
            renderer.StampTexture = newTexture;
            Assert.AreEqual(newTexture, renderer.StampTexture);

            // ClearColor property
            Color newColor = Color.red;
            renderer.ClearColor = newColor;
            Assert.AreEqual(newColor, renderer.ClearColor);

            // AutoClear property
            renderer.AutoClear = false;
            Assert.IsFalse(renderer.AutoClear);

            // OrthoSize property
            var newSize = Vector2.one * 2.5f;
            renderer.OrthoSize = newSize;
            Assert.AreEqual(newSize, renderer.OrthoSize);

            // CameraPosition property
            Vector3 newPosition = new Vector3(1, 2, 3);
            renderer.CameraPosition = newPosition;
            Assert.AreEqual(newPosition, renderer.CameraPosition);

            // TransformMatrix property
            Matrix4x4 newMatrix = Matrix4x4.Translate(new Vector3(5, 0, 0));
            renderer.TransformMatrix = newMatrix;
            Assert.AreEqual(newMatrix, renderer.TransformMatrix);

            // Cleanup
            Object.DestroyImmediate(newMesh);
            Object.DestroyImmediate(newTexture);
            renderer.Dispose();
        }

        [Test]
        public void OrthoSize_ZeroOrNegative_ThrowsException() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);

            // Act & Assert
            Assert.Throws<System.Exception>(() => renderer.OrthoSize = Vector2.zero);
            Assert.Throws<System.Exception>(() => renderer.OrthoSize = -1 * Vector2.one);

            // Cleanup
            renderer.Dispose();
        }

        [UnityTest]
        public IEnumerator Render_ValidInputs_RendersToTexture() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);
            renderer.SetupRenderTextures(256, 256);

            // Act
            renderer.Render(Color.clear);

            // Wait for rendering to complete
            yield return null;

            // Assert - Check that the render texture is not empty
            // Create a temporary RenderTexture to read from
            RenderTexture tempRT = RenderTexture.GetTemporary(256, 256);
            Graphics.Blit(renderer.RenderTexture, tempRT);

            // Read pixels from the temporary RT
            RenderTexture.active = tempRT;
            Texture2D readTexture = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            readTexture.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            readTexture.Apply();
            RenderTexture.active = null;

            // Check if there are non-transparent pixels (indicating something was rendered)
            Color[] pixels = readTexture.GetPixels();
            bool hasContent = false;
            foreach (Color pixel in pixels) {
                if (pixel.a > 0.01f) {
                    hasContent = true;
                    break;
                }
            }

            Assert.IsTrue(hasContent, "Render texture should contain non-transparent pixels after rendering");

            // Cleanup
            RenderTexture.ReleaseTemporary(tempRT);
            Object.DestroyImmediate(readTexture);
            renderer.Dispose();
        }

        [Test]
        public void Dispose_AfterUse_ReleasesResources() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(mesh: _testMesh, stampTexture: _testTexture);
            renderer.SetupRenderTextures(256, 256);

            // Act - use the renderer then dispose it
            renderer.Render();
            renderer.Dispose();

            // Assert - RenderTexture should be null after disposal
            Assert.IsNull(renderer.RenderTexture);
        }

        [Test]
        public void Render_NullResources_LogsError() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer();

            // Act & Assert - Should log an error but not throw an exception
            LogAssert.Expect(LogType.Error, "Cannot render: missing required resources.");
            renderer.Render();

            // Cleanup
            renderer.Dispose();
        }

        [UnityTest]
        public IEnumerator Render_DifferentBlendModes_ProduceDifferentResults() {
            // Arrange
            int textureSize = 256;
            StampsRenderer renderer = new StampsRenderer(textureSize, textureSize, _testMesh, _testTexture);

            // Create textures to store results for each blend mode
            Texture2D normalResult = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            Texture2D additiveResult = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            Texture2D multiplyResult = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            Texture2D screenResult = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);

            // Setup test scenario - use distinct colors for clear and mesh
            Color backgroundColor = new Color(0.2f, 0.2f, 0.5f, 1.0f);
            Color stampColor = new Color(0.8f, 0.2f, 0.2f, 0.7f);
            renderer.SetColor("_Color", stampColor);
            renderer.ClearColor = backgroundColor;

            try {
                // Test each blend mode independently
                // 1. Normal blend
                renderer.SetBlendMode(StampsRenderer.BlendMode.Normal);
                renderer.Render(true); // Clear and render
                yield return null; // Wait for render to complete
                CaptureRenderTexture(renderer.RenderTexture, normalResult);

                // 2. Additive blend
                renderer.SetBlendMode(StampsRenderer.BlendMode.Additive);
                renderer.Render(true);
                yield return null;
                CaptureRenderTexture(renderer.RenderTexture, additiveResult);

                // 3. Multiply blend
                renderer.SetBlendMode(StampsRenderer.BlendMode.Multiply);
                renderer.Render(true);
                yield return null;
                CaptureRenderTexture(renderer.RenderTexture, multiplyResult);

                // 4. Screen blend
                renderer.SetBlendMode(StampsRenderer.BlendMode.Screen);
                renderer.Render(true);
                yield return null;
                CaptureRenderTexture(renderer.RenderTexture, screenResult);

                // Verify results - compare each blend mode against others
                Assert.IsTrue(ImagesAreDifferent(normalResult, additiveResult),
                    "Normal and Additive blend modes should produce different results");

                Assert.IsTrue(ImagesAreDifferent(normalResult, multiplyResult),
                    "Normal and Multiply blend modes should produce different results");

                Assert.IsTrue(ImagesAreDifferent(normalResult, screenResult),
                    "Normal and Screen blend modes should produce different results");

                Assert.IsTrue(ImagesAreDifferent(additiveResult, multiplyResult),
                    "Additive and Multiply blend modes should produce different results");

                Assert.IsTrue(ImagesAreDifferent(additiveResult, screenResult),
                    "Additive and Screen blend modes should produce different results");

                Assert.IsTrue(ImagesAreDifferent(multiplyResult, screenResult),
                    "Multiply and Screen blend modes should produce different results");
            } finally {
                // Cleanup
                Object.DestroyImmediate(normalResult);
                Object.DestroyImmediate(additiveResult);
                Object.DestroyImmediate(multiplyResult);
                Object.DestroyImmediate(screenResult);
                renderer.Dispose();
            }
        }

        // Helper method to copy from RenderTexture to Texture2D
        private void CaptureRenderTexture(RenderTexture source, Texture2D destination) {
            RenderTexture prevActive = RenderTexture.active;
            RenderTexture.active = source;

            destination.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            destination.Apply();

            RenderTexture.active = prevActive;
        }

        // Helper method to determine if two textures have significantly different pixels
        private bool ImagesAreDifferent(Texture2D a, Texture2D b) {
            if (a.width != b.width || a.height != b.height)
                return true;

            Color[] pixelsA = a.GetPixels();
            Color[] pixelsB = b.GetPixels();

            // Check the center area of the texture where differences are most likely to be visible
            int centerX = a.width / 2;
            int centerY = a.height / 2;
            int checkRadius = Mathf.Min(a.width, a.height) / 4;
            float threshold = 0.05f; // Minimum difference to consider pixels different
            int significantDifferenceCount = 0;
            int requiredDifferentPixels = 10; // Require this many pixels to be different

            // Sample a grid of pixels in the center area
            for (int y = centerY - checkRadius; y < centerY + checkRadius; y += 4) {
                for (int x = centerX - checkRadius; x < centerX + checkRadius; x += 4) {
                    int index = y * a.width + x;
                    if (index >= 0 && index < pixelsA.Length) {
                        float diffR = Mathf.Abs(pixelsA[index].r - pixelsB[index].r);
                        float diffG = Mathf.Abs(pixelsA[index].g - pixelsB[index].g);
                        float diffB = Mathf.Abs(pixelsA[index].b - pixelsB[index].b);

                        // Consider different if any channel differs significantly
                        if (diffR > threshold || diffG > threshold || diffB > threshold) {
                            significantDifferenceCount++;

                            if (significantDifferenceCount >= requiredDifferentPixels)
                                return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}