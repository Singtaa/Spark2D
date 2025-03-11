using UnityEngine;
using System;

namespace Spark2D {
    /// <summary>
    /// API is designed for zero-allocation during Generate() calls.
    /// </summary>
    public class QuadsMesher {
        Mesh _mesh;
        Vector2[] _positions;
        float[] _rotations;
        float[] _scales;
        Color[] _colors;
        int _capacity;

        Vector3[] _meshVertices;
        Vector2[] _meshUVs;
        Color[] _meshColors;

        /// <summary>
        /// Creates a new QuadsMesher with the specified capacity and initial values.
        /// </summary>
        /// <param name="capacity">Maximum number of quads that can be generated.</param>
        /// <param name="positions">Initial positions for each quad.</param>
        /// <param name="rotations">Initial rotations for each quad (in degrees). Can be null.</param>
        /// <param name="scales">Initial scales for each quad. Can be null.</param>
        /// <param name="colors">Initial colors for each quad. Can be null.</param>
        public QuadsMesher(int capacity, Vector2[] positions, float[] rotations = null, float[] scales = null, Color[] colors = null) {
            if (capacity <= 0)
                throw new Exception("Capacity must be greater than 0");

            _capacity = capacity;
            _positions = positions;
            _rotations = rotations;
            _scales = scales;
            _colors = colors;

            // Initialize the mesh with sufficient capacity
            InitializeMesh();
        }

        #region // MARK: - Properties
        /// <summary>
        /// Gets the internal mesh that contains the generated quads.
        /// </summary>
        public Mesh Mesh => _mesh;

        /// <summary>
        /// Gets or sets the positions for each quad.
        /// </summary>
        public Vector2[] Positions {
            get { return _positions; }
            set { _positions = value; }
        }

        /// <summary>
        /// Gets or sets the rotations for each quad (in degrees).
        /// </summary>
        public float[] Rotations {
            get { return _rotations; }
            set { _rotations = value; }
        }

        /// <summary>
        /// Gets or sets the scales for each quad.
        /// </summary>
        public float[] Scales {
            get { return _scales; }
            set { _scales = value; }
        }

        /// <summary>
        /// Gets or sets the colors for each quad.
        /// </summary>
        public Color[] Colors {
            get { return _colors; }
            set { _colors = value; }
        }
        #endregion

        #region // MARK: - Public 
        public void Make() {
            if (_mesh == null || _positions == null || _positions.Length == 0)
                return;

            // Calculate how many quads we can create
            int quadCount = Mathf.Min(_positions.Length, _capacity);

            // Check if we have colors
            bool hasColors = _colors != null && _colors.Length > 0;
            if (hasColors && _meshColors == null) {
                _meshColors = new Color[_meshVertices.Length];
            }

            // Update vertices for each quad
            for (int i = 0; i < quadCount; i++) {
                Vector2 pos = _positions[i];
                int vertexOffset = i * 4;

                // Get rotation in radians if available
                float rotation = 0f;
                if (_rotations != null && i < _rotations.Length) {
                    rotation = _rotations[i] * Mathf.Deg2Rad;
                }

                // Get scale if available
                float scale = 1f;
                if (_scales != null && i < _scales.Length) {
                    scale = _scales[i];
                }

                // Calculate quad corners with rotation and scale
                float halfSize = 0.5f * scale;
                float sin = Mathf.Sin(rotation);
                float cos = Mathf.Cos(rotation);

                // Apply rotation and scale to the corners
                Vector2 bl = new Vector2(-halfSize, -halfSize);
                Vector2 br = new Vector2(halfSize, -halfSize);
                Vector2 tr = new Vector2(halfSize, halfSize);
                Vector2 tl = new Vector2(-halfSize, halfSize);

                // Rotate corners
                bl = new Vector2(bl.x * cos - bl.y * sin, bl.x * sin + bl.y * cos);
                br = new Vector2(br.x * cos - br.y * sin, br.x * sin + br.y * cos);
                tr = new Vector2(tr.x * cos - tr.y * sin, tr.x * sin + tr.y * cos);
                tl = new Vector2(tl.x * cos - tl.y * sin, tl.x * sin + tl.y * cos);

                // Apply position offset
                _meshVertices[vertexOffset + 0] = new Vector3(pos.x + bl.x, pos.y + bl.y, 0);
                _meshVertices[vertexOffset + 1] = new Vector3(pos.x + br.x, pos.y + br.y, 0);
                _meshVertices[vertexOffset + 2] = new Vector3(pos.x + tr.x, pos.y + tr.y, 0);
                _meshVertices[vertexOffset + 3] = new Vector3(pos.x + tl.x, pos.y + tl.y, 0);

                // Set up UVs
                _meshUVs[vertexOffset + 0] = new Vector2(0, 0);
                _meshUVs[vertexOffset + 1] = new Vector2(1, 0);
                _meshUVs[vertexOffset + 2] = new Vector2(1, 1);
                _meshUVs[vertexOffset + 3] = new Vector2(0, 1);

                // Set colors if available
                if (hasColors && i < _colors.Length) {
                    Color color = _colors[i];
                    _meshColors[vertexOffset + 0] = color;
                    _meshColors[vertexOffset + 1] = color;
                    _meshColors[vertexOffset + 2] = color;
                    _meshColors[vertexOffset + 3] = color;
                }
            }

            // Set remaining vertices to zero if needed
            for (int i = quadCount * 4; i < _meshVertices.Length; i++) {
                _meshVertices[i] = Vector3.zero;
                _meshUVs[i] = Vector2.zero;
                if (hasColors) {
                    _meshColors[i] = Color.clear;
                }
            }

            // Update the mesh using non-allocating methods
            _mesh.SetVertices(_meshVertices, 0, _meshVertices.Length);
            _mesh.SetUVs(0, _meshUVs, 0, _meshUVs.Length);
            if (hasColors) {
                _mesh.SetColors(_meshColors, 0, _meshColors.Length);
            }

            // Recalculate bounds
            _mesh.RecalculateBounds();
        }
        #endregion

        #region // MARK: - Private
        void InitializeMesh() {
            _mesh = new Mesh();
            _mesh.name = "QuadsMesher_Mesh";

            int vertexCount = _capacity * 4;
            _meshVertices = new Vector3[vertexCount];
            _meshUVs = new Vector2[vertexCount];
            int[] triangles = new int[_capacity * 6];

            // Initialize triangles (same as before)
            for (int i = 0; i < _capacity; i++) {
                int vertexOffset = i * 4;
                int triangleOffset = i * 6;

                triangles[triangleOffset + 0] = vertexOffset + 0;
                triangles[triangleOffset + 1] = vertexOffset + 1;
                triangles[triangleOffset + 2] = vertexOffset + 2;

                triangles[triangleOffset + 3] = vertexOffset + 0;
                triangles[triangleOffset + 4] = vertexOffset + 2;
                triangles[triangleOffset + 5] = vertexOffset + 3;
            }

            // Set mesh data using non-allocating methods
            _mesh.SetVertices(_meshVertices);
            _mesh.SetUVs(0, _meshUVs);
            _mesh.SetTriangles(triangles, 0);

            // Set up vertex colors if needed
            if (_colors != null) {
                _meshColors = new Color[vertexCount];
                _mesh.SetColors(_meshColors);
            }
        }
        #endregion
    }
}