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
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);

            // Assert
            Assert.IsNotNull(renderer);
            Assert.AreEqual(_testMesh, renderer.Mesh);
            Assert.AreEqual(_testTexture, renderer.StampTexture);
        }

        [Test]
        public void SetupRenderTexture_ValidParameters_CreatesRenderTexture() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);

            // Act
            renderer.SetupRenderTexture(256, 256);

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
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);

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
            float newSize = 2.5f;
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
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);

            // Act & Assert
            Assert.Throws<System.Exception>(() => renderer.OrthoSize = 0);
            Assert.Throws<System.Exception>(() => renderer.OrthoSize = -1);

            // Cleanup
            renderer.Dispose();
        }

        [UnityTest]
        public IEnumerator Render_ValidInputs_RendersToTexture() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);
            renderer.SetupRenderTexture(256, 256);

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
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);
            renderer.SetupRenderTexture(256, 256);

            // Act - use the renderer then dispose it
            renderer.Render();
            renderer.Dispose();

            // Assert - RenderTexture should be null after disposal
            Assert.IsNull(renderer.RenderTexture);
        }

        [Test]
        public void Render_NullResources_LogsError() {
            // Arrange
            StampsRenderer renderer = new StampsRenderer(null, null);

            // Act & Assert - Should log an error but not throw an exception
            LogAssert.Expect(LogType.Error, "Cannot render: missing required resources.");
            renderer.Render();

            // Cleanup
            renderer.Dispose();
        }

        [UnityTest]
        public IEnumerator Render_DifferentBlendModes_ProduceDifferentResults() {
            // This test verifies that different blend modes actually produce different visual results

            // Arrange
            StampsRenderer renderer = new StampsRenderer(_testMesh, _testTexture);
            renderer.SetupRenderTexture(256, 256);

            // Set up for the first render (Normal blend)
            renderer.SetBlendMode(StampsRenderer.BlendMode.Normal);
            renderer.Render(Color.clear);

            // Wait for rendering to complete
            yield return null;

            // Capture the result of the first render
            RenderTexture tempRT1 = RenderTexture.GetTemporary(256, 256);
            Graphics.Blit(renderer.RenderTexture, tempRT1);
            RenderTexture.active = tempRT1;
            Texture2D normalBlendResult = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            normalBlendResult.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            normalBlendResult.Apply();
            RenderTexture.active = null;

            // Set up for the second render (Additive blend)
            renderer.SetBlendMode(StampsRenderer.BlendMode.Additive);
            renderer.Render(Color.clear);

            // Wait for rendering to complete
            yield return null;

            // Capture the result of the second render
            RenderTexture tempRT2 = RenderTexture.GetTemporary(256, 256);
            Graphics.Blit(renderer.RenderTexture, tempRT2);
            RenderTexture.active = tempRT2;
            Texture2D additiveBlendResult = new Texture2D(256, 256, TextureFormat.RGBA32, false);
            additiveBlendResult.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
            additiveBlendResult.Apply();
            RenderTexture.active = null;

            // Compare the results - they should be different
            Color[] normalPixels = normalBlendResult.GetPixels();
            Color[] additivePixels = additiveBlendResult.GetPixels();

            bool pixelsDiffer = false;
            for (int i = 0; i < normalPixels.Length; i++) {
                if (Mathf.Abs(normalPixels[i].r - additivePixels[i].r) > 0.01f ||
                    Mathf.Abs(normalPixels[i].g - additivePixels[i].g) > 0.01f ||
                    Mathf.Abs(normalPixels[i].b - additivePixels[i].b) > 0.01f ||
                    Mathf.Abs(normalPixels[i].a - additivePixels[i].a) > 0.01f) {
                    pixelsDiffer = true;
                    break;
                }
            }

            Assert.IsTrue(pixelsDiffer, "Different blend modes should produce different visual results");

            // Cleanup
            RenderTexture.ReleaseTemporary(tempRT1);
            RenderTexture.ReleaseTemporary(tempRT2);
            Object.DestroyImmediate(normalBlendResult);
            Object.DestroyImmediate(additiveBlendResult);
            renderer.Dispose();
        }
    }
}