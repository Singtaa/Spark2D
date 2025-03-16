using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Spark2D {
    /// <summary>
    /// API is designed for zero-allocation during Render() calls.
    /// Two-stage rendering: accumulation then composition for proper alpha blending.
    /// </summary>
    public class StampsRenderer {
        Mesh _mesh;
        Texture2D _stampTexture;
        RenderTexture _rt;
        RenderTexture _accumRT;
        int _width;
        int _height;

        Material _accumulationMaterial;
        Material _compositingMaterial;
        CommandBuffer _cmd;
        Color _clearColor = Color.clear;
        bool _autoClear = true;

        Vector2 _orthoSize = Vector2.one;
        Vector3 _cameraPosition = Vector3.zero;
        Matrix4x4 _transformMatrix = Matrix4x4.identity;

        Matrix4x4 _projectionMatrix = Matrix4x4.identity;
        Matrix4x4 _viewMatrix = Matrix4x4.identity;
        bool _matricesDirty = true;

        Mesh _fullscreenQuad;

        /// <summary>
        /// Creates a new StampsRenderer with the specified mesh and stamp texture.
        /// </summary>
        public StampsRenderer(int rtWidth = 128, int rtHeight = 128, Mesh mesh = null, Texture2D stampTexture = null) {
            _mesh = mesh;
            _stampTexture = stampTexture;

            SetupRenderTextures(rtWidth, rtHeight);
            
            // Create fullscreen quad for compositing
            _fullscreenQuad = CreateFullscreenQuad();

            // Initialize shaders and materials
            var accumulationShader = Shader.Find("Spark2D/AccumulationShader");
            if (accumulationShader == null) {
                Debug.LogError("Failed to find Spark2D/AccumulationShader shader. Make sure it's included in the project.");
                return;
            }

            var compositingShader = Shader.Find("Spark2D/CompositingShader");
            if (compositingShader == null) {
                Debug.LogError("Failed to find Spark2D/CompositingShader shader. Make sure it's included in the project.");
                return;
            }

            _accumulationMaterial = new Material(accumulationShader);
            _accumulationMaterial.SetTexture("_MainTex", _stampTexture);
            
            _compositingMaterial = new Material(compositingShader);

            // Initialize command buffer
            _cmd = new CommandBuffer();
            _cmd.name = "StampsRenderer";
        }

        #region // MARK: - Properties
        /// <summary>
        /// Gets or sets the mesh to render.
        /// </summary>
        public Mesh Mesh {
            get { return _mesh; }
            set { _mesh = value; }
        }

        /// <summary>
        /// Gets or sets the stamp texture to use.
        /// </summary>
        public Texture2D StampTexture {
            get { return _stampTexture; }
            set {
                _stampTexture = value;
                if (_accumulationMaterial != null && _stampTexture != null) {
                    _accumulationMaterial.SetTexture("_MainTex", _stampTexture);
                }
            }
        }

        /// <summary>
        /// Gets or sets the target render texture.
        /// </summary>
        public RenderTexture RenderTexture {
            get { return _rt; }
            set { _rt = value; }
        }

        /// <summary>
        /// Gets or sets the accumulation render texture.
        /// </summary>
        public RenderTexture AccumulationRenderTexture {
            get { return _accumRT; }
            set { _accumRT = value; }
        }

        public int Width {
            get { return _width; }
            set {
                if (value <= 0)
                    throw new Exception("Width must be greater than 0");
                if (_width != value) {
                    _width = value;
                    SetupRenderTextures(_width, _height);
                }
            }
        }

        public int Height {
            get { return _height; }
            set {
                if (value <= 0)
                    throw new Exception("Height must be greater than 0");
                if (_height != value) {
                    _height = value;
                    SetupRenderTextures(_width, _height);
                }
            }
        }

        /// <summary>
        /// Gets or sets the clear color for the render texture.
        /// </summary>
        public Color ClearColor {
            get { return _clearColor; }
            set { _clearColor = value; }
        }

        /// <summary>
        /// Gets or sets whether to automatically clear the render texture before rendering.
        /// </summary>
        public bool AutoClear {
            get { return _autoClear; }
            set { _autoClear = value; }
        }

        /// <summary>
        /// Gets or sets the orthographic size for rendering (controls the camera zoom).
        /// </summary>
        public Vector2 OrthoSize {
            get { return _orthoSize; }
            set {
                if (value.x <= 0 || value.y <= 0)
                    throw new Exception("OrthoSize must be greater than 0");
                if (_orthoSize != value) {
                    _orthoSize = value;
                    _matricesDirty = true;
                }
            }
        }

        public Vector2 Dimension {
            get { return OrthoSize * 2f; }
            set {
                OrthoSize = value * 0.5f;
            }
        }

        /// <summary>
        /// Gets or sets the camera position for rendering.
        /// </summary>
        public Vector3 CameraPosition {
            get { return _cameraPosition; }
            set {
                if (_cameraPosition != value) {
                    _cameraPosition = value;
                    _matricesDirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the transform matrix for the mesh.
        /// </summary>
        public Matrix4x4 TransformMatrix {
            get { return _transformMatrix; }
            set { _transformMatrix = value; }
        }
        #endregion

        #region // MARK: - Public
        /// <summary>
        /// Sets up the RenderTextures with the specified dimensions and format.
        /// </summary>
        public void SetupRenderTextures(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32) {
            // Setup final render texture
            if (_rt == null || _rt.width != width || _rt.height != height || _rt.format != format) {
                if (_rt != null) {
                    _rt.Release();
                }

                _rt = new RenderTexture(width, height, 0, format);
                _rt.antiAliasing = 1;
                _rt.filterMode = FilterMode.Point;
                _rt.Create();
            }
            
            // Setup accumulation render texture
            if (_accumRT == null || _accumRT.width != width || _accumRT.height != height || _accumRT.format != format) {
                if (_accumRT != null) {
                    _accumRT.Release();
                }

                _accumRT = new RenderTexture(width, height, 0, format);
                _accumRT.antiAliasing = 1;
                _accumRT.filterMode = FilterMode.Point;
                _accumRT.Create();
            }
            
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Sets a texture property on the accumulation material.
        /// </summary>
        public void SetTexture(string name, Texture texture) {
            if (_accumulationMaterial != null) {
                _accumulationMaterial.SetTexture(name, texture);
            }
        }

        /// <summary>
        /// Sets a float property on the accumulation material.
        /// </summary>
        public void SetFloat(string name, float value) {
            if (_accumulationMaterial != null) {
                _accumulationMaterial.SetFloat(name, value);
            }
        }

        /// <summary>
        /// Sets a color property on the accumulation material.
        /// </summary>
        public void SetColor(string name, Color value) {
            if (_accumulationMaterial != null) {
                _accumulationMaterial.SetColor(name, value);
            }
        }

        public void SetBlendMode(BlendMode blendMode) {
            // In the two-stage approach, blend modes are handled differently
            // We keep this method for compatibility but with modified behavior
            
            // Adjust intensity or other parameters if needed based on blend mode
            switch (blendMode) {
                case BlendMode.Normal:
                    // Default behavior
                    break;
                case BlendMode.Additive:
                    // Could adjust intensity or other parameters
                    break;
                case BlendMode.Multiply:
                    // Could use a different accumulation technique
                    break;
                case BlendMode.Screen:
                    // Could use a different accumulation technique
                    break;
            }
        }

        public void SetIntensity(float intensity) {
            if (_accumulationMaterial != null) {
                _accumulationMaterial.SetFloat("_Intensity", intensity);
            }
        }

        /// <summary>
        /// Renders the mesh with the stamp texture using the two-stage approach.
        /// </summary>
        public void Render() {
            Render(_autoClear);
        }

        /// <summary>
        /// Renders using the two-stage approach, with a clear option.
        /// </summary>
        public void Render(bool clear) {
            if (_mesh == null || _stampTexture == null || _rt == null || _accumRT == null || 
                !_rt.IsCreated() || !_accumRT.IsCreated() || 
                _accumulationMaterial == null || _compositingMaterial == null) {
                Debug.LogError("Cannot render: missing required resources.");
                return;
            }

            // Update matrices if needed
            UpdateMatrices();

            // Clear the command buffer
            _cmd.Clear();

            // STAGE 1: Accumulation
            // Set accumulation render texture as the target
            _cmd.SetRenderTarget(_accumRT);

            // Clear the accumulation texture if requested
            if (clear) {
                _cmd.ClearRenderTarget(true, true, Color.clear);
            }

            _cmd.SetViewProjectionMatrices(_viewMatrix, _projectionMatrix);

            // Draw the mesh with the accumulation material
            _cmd.DrawMesh(_mesh, _transformMatrix, _accumulationMaterial);

            // STAGE 2: Compositing
            // Set final render texture as the target
            _cmd.SetRenderTarget(_rt);

            // Only clear if it's the first render or explicit clear was requested
            if (clear) {
                _cmd.ClearRenderTarget(true, true, _clearColor);
            }

            // Set the accumulated texture for compositing
            _compositingMaterial.SetTexture("_AccumTex", _accumRT);
            _compositingMaterial.SetTexture("_BackgroundTex", _rt);

            // Draw a fullscreen quad with the compositing material
            _cmd.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
            _cmd.DrawMesh(_fullscreenQuad, Matrix4x4.identity, _compositingMaterial);

            // Execute the command buffer
            Graphics.ExecuteCommandBuffer(_cmd);
        }

        /// <summary>
        /// Renders with a custom clear color.
        /// </summary>
        public void Render(Color clearColor) {
            _clearColor = clearColor;
            Render(true);
        }

        /// <summary>
        /// Renders with a custom transform matrix.
        /// </summary>
        public void Render(Matrix4x4 transformMatrix, bool clear = true) {
            _transformMatrix = transformMatrix;
            Render(clear);
        }

        /// <summary>
        /// Clears only the accumulation buffer, keeping the final render texture intact.
        /// </summary>
        public void ClearAccumulation() {
            if (_accumRT == null || !_accumRT.IsCreated()) return;
            
            _cmd.Clear();
            _cmd.SetRenderTarget(_accumRT);
            _cmd.ClearRenderTarget(true, true, Color.clear);
            Graphics.ExecuteCommandBuffer(_cmd);
        }

        /// <summary>
        /// Releases the resources used by this renderer.
        /// </summary>
        public void Dispose() {
            if (_cmd != null) {
                _cmd.Dispose();
                _cmd = null;
            }

            if (_accumulationMaterial != null) {
                GameObject.DestroyImmediate(_accumulationMaterial);
                _accumulationMaterial = null;
            }

            if (_compositingMaterial != null) {
                GameObject.DestroyImmediate(_compositingMaterial);
                _compositingMaterial = null;
            }

            if (_rt != null && _rt.IsCreated()) {
                _rt.Release();
                _rt = null;
            }

            if (_accumRT != null && _accumRT.IsCreated()) {
                _accumRT.Release();
                _accumRT = null;
            }
            
            if (_fullscreenQuad != null) {
                GameObject.DestroyImmediate(_fullscreenQuad);
                _fullscreenQuad = null;
            }
        }
        #endregion

        #region // MARK: - Private
        void UpdateMatrices() {
            if (!_matricesDirty)
                return;
            _projectionMatrix = Matrix4x4.Ortho(-_orthoSize.x, _orthoSize.x, -_orthoSize.y, _orthoSize.y, -1f, 1f);
            _viewMatrix = Matrix4x4.TRS(_cameraPosition, Quaternion.identity, Vector3.one).inverse;
            _matricesDirty = false;
        }
        
        Mesh CreateFullscreenQuad() {
            Mesh mesh = new Mesh();
            
            // Vertices for a fullscreen quad
            Vector3[] vertices = new Vector3[4] {
                new Vector3(-1, -1, 0),
                new Vector3(1, -1, 0),
                new Vector3(-1, 1, 0),
                new Vector3(1, 1, 0)
            };
            
            // UVs
            Vector2[] uv = new Vector2[4] {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            // Triangles
            int[] triangles = new int[6] {
                0, 2, 1,
                2, 3, 1
            };
            
            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            
            return mesh;
        }
        #endregion

        #region // MARK: - Types
        /// <summary>
        /// Blend modes that can be used for rendering.
        /// </summary>
        public enum BlendMode {
            Normal,
            Additive,
            Multiply,
            Screen
        }
        #endregion
    }
}