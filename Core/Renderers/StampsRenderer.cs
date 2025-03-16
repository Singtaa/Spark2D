using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace Spark2D {
    /// <summary>
    /// API is designed for zero-allocation during Render() calls.
    /// </summary>
    public class StampsRenderer {
        Mesh _mesh;
        Texture2D _stampTexture;
        RenderTexture _rt;
        int _width;
        int _height;

        Material _material;
        CommandBuffer _cmd;
        Color _clearColor = Color.clear;
        bool _autoClear = true;

        Vector2 _orthoSize = Vector2.one;
        Vector3 _cameraPosition = Vector3.zero;
        Matrix4x4 _transformMatrix = Matrix4x4.identity;

        Matrix4x4 _projectionMatrix = Matrix4x4.identity;
        Matrix4x4 _viewMatrix = Matrix4x4.identity;
        bool _matricesDirty = true;

        /// <summary>
        /// Creates a new StampsRenderer with the specified mesh and stamp texture.
        /// </summary>
        public StampsRenderer(int rtWidth = 128, int rtHeight = 128, Mesh mesh = null, Texture2D stampTexture = null) {
            _mesh = mesh;
            _stampTexture = stampTexture;

            SetupRenderTexture(rtWidth, rtHeight);

            // Initialize shader and material
            var shader = Shader.Find("Spark2D/StampShader");
            if (shader == null) {
                Debug.LogError("Failed to find Spark2D/StampShader shader. Make sure it's included in the project.");
                return;
            }

            _material = new Material(shader);
            _material.SetTexture("_MainTex", _stampTexture);

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
                if (_material != null && _stampTexture != null) {
                    _material.SetTexture("_MainTex", _stampTexture);
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

        public int Width {
            get { return _width; }
            set {
                if (value <= 0)
                    throw new Exception("Width must be greater than 0");
                if (_width != value) {
                    _width = value;
                    SetupRenderTexture(_width, _height);
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
                    SetupRenderTexture(_width, _height);
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
        /// Sets up the RenderTexture with the specified dimensions and format. New RenderTexture is created
        /// only if needed, that is, if the current RenderTexture is null or has different dimensions or format.
        /// </summary>
        public void SetupRenderTexture(int width, int height, RenderTextureFormat format = RenderTextureFormat.ARGB32) {
            if (_rt == null || _rt.width != width || _rt.height != height || _rt.format != format) {
                if (_rt != null) {
                    _rt.Release();
                }

                _rt = new RenderTexture(width, height, 0, format);
                _rt.antiAliasing = 1;
                _rt.filterMode = FilterMode.Point;
                _rt.Create();
            }
            _width = width;
            _height = height;
        }

        /// <summary>
        /// Sets a texture property on the material.
        /// </summary>
        public void SetTexture(string name, Texture texture) {
            if (_material != null) {
                _material.SetTexture(name, texture);
            }
        }

        /// <summary>
        /// Sets a float property on the material.
        /// </summary>
        public void SetFloat(string name, float value) {
            if (_material != null) {
                _material.SetFloat(name, value);
            }
        }

        /// <summary>
        /// Sets a color property on the material.
        /// </summary>
        public void SetColor(string name, Color value) {
            if (_material != null) {
                _material.SetColor(name, value);
            }
        }

        public void SetBlendMode(BlendMode blendMode) {
            if (_material == null)
                return;

            // First, set blend mode parameters
            switch (blendMode) {
                case BlendMode.Normal:
                    _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case BlendMode.Additive:
                    _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    break;
                case BlendMode.Multiply:
                    _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.DstColor);
                    _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    break;
                case BlendMode.Screen:
                    _material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    _material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                    break;
            }

            // Ensure shader knows to apply these blend settings
            _material.DisableKeyword("_ALPHATEST_ON");
            _material.EnableKeyword("_ALPHABLEND_ON");
            _material.DisableKeyword("_ALPHAPREMULTIPLY_ON");

            // Make sure the render queue is appropriate for transparency
            _material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        public void SetIntensity(float intensity) {
            if (_material != null) {
                _material.SetFloat("_Intensity", intensity);
            }
        }

        /// <summary>
        /// Renders the mesh with the stamp texture onto the render texture.
        /// </summary>
        public void Render() {
            Render(_autoClear);
        }

        /// <summary>
        /// Renders the mesh with the stamp texture onto the render texture, with a clear option.
        /// </summary>
        public void Render(bool clear) {
            if (_mesh == null || _stampTexture == null || _rt == null || !_rt.IsCreated() || _material == null) {
                Debug.LogError("Cannot render: missing required resources.");
                return;
            }

            // Update matrices if needed
            UpdateMatrices();

            // Clear the command buffer
            _cmd.Clear();

            // Set render target
            _cmd.SetRenderTarget(_rt);

            // Clear the render texture if requested
            if (clear) {
                _cmd.ClearRenderTarget(true, true, _clearColor);
            }

            _cmd.SetViewProjectionMatrices(_viewMatrix, _projectionMatrix);

            // Draw the mesh with the material
            _cmd.DrawMesh(_mesh, _transformMatrix, _material);

            // Execute the command buffer
            Graphics.ExecuteCommandBuffer(_cmd);
        }

        /// <summary>
        /// Renders the mesh with the stamp texture onto the render texture, with a custom clear color.
        /// </summary>
        public void Render(Color clearColor) {
            _clearColor = clearColor;
            Render(true);
        }

        /// <summary>
        /// Renders the mesh with the stamp texture onto the render texture, with a custom transform matrix.
        /// </summary>
        public void Render(Matrix4x4 transformMatrix, bool clear = true) {
            _transformMatrix = transformMatrix;
            Render(clear);
        }

        /// <summary>
        /// Releases the resources used by this renderer.
        /// </summary>
        public void Dispose() {
            if (_cmd != null) {
                _cmd.Dispose();
                _cmd = null;
            }

            if (_material != null) {
                GameObject.DestroyImmediate(_material);
                _material = null;
            }

            if (_rt != null && _rt.IsCreated()) {
                _rt.Release();
                _rt = null;
            }
        }
        #endregion

        // MARK: - Private
        void UpdateMatrices() {
            if (!_matricesDirty)
                return;
            _projectionMatrix = Matrix4x4.Ortho(-_orthoSize.x, _orthoSize.x, -_orthoSize.y, _orthoSize.y, -1f, 1f);
            _viewMatrix = Matrix4x4.TRS(_cameraPosition, Quaternion.identity, Vector3.one).inverse;
            _matricesDirty = false;
        }

        // MARK: - Types
        /// <summary>
        /// Blend modes that can be used for rendering.
        /// </summary>
        public enum BlendMode {
            Normal,
            Additive,
            Multiply,
            Screen
        }
    }
}