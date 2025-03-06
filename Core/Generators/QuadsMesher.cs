using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System;

namespace Spark2D {
    public class QuadsMesher {
        Mesh _mesh;
        float2[] _positions;
        float[] _rotations;
        float[] _scales;
        float4[] _colors;
        int _capacity;

        /// <summary>
        /// Gets the internal mesh that contains the generated quads.
        /// </summary>
        public Mesh Mesh => _mesh;

        /// <summary>
        /// Gets or sets the positions for each quad.
        /// </summary>
        public float2[] Positions {
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
        public float4[] Colors {
            get { return _colors; }
            set { _colors = value; }
        }

        /// <summary>
        /// Creates a new QuadsMesher with the specified capacity and initial values.
        /// </summary>
        /// <param name="capacity">Maximum number of quads that can be generated.</param>
        /// <param name="positions">Initial positions for each quad.</param>
        /// <param name="rotations">Initial rotations for each quad (in degrees). Can be null.</param>
        /// <param name="scales">Initial scales for each quad. Can be null.</param>
        /// <param name="colors">Initial colors for each quad. Can be null.</param>
        public QuadsMesher(int capacity, float2[] positions, float[] rotations = null, float[] scales = null, float4[] colors = null) {
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

        void InitializeMesh() {
            _mesh = new Mesh();
            _mesh.name = "QuadsMesher_Mesh";

            int vertexCount = _capacity * 4; // 4 vertices per quad
            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            int[] triangles = new int[_capacity * 6]; // 6 indices (2 triangles) per quad

            // Initialize triangles - this only needs to be done once
            for (int i = 0; i < _capacity; i++) {
                int vertexOffset = i * 4;
                int triangleOffset = i * 6;

                // First triangle (bottom-left, bottom-right, top-right)
                triangles[triangleOffset + 0] = vertexOffset + 0;
                triangles[triangleOffset + 1] = vertexOffset + 1;
                triangles[triangleOffset + 2] = vertexOffset + 2;

                // Second triangle (bottom-left, top-right, top-left)
                triangles[triangleOffset + 3] = vertexOffset + 0;
                triangles[triangleOffset + 4] = vertexOffset + 2;
                triangles[triangleOffset + 5] = vertexOffset + 3;
            }

            // Initialize arrays (these will be updated in Generate)
            _mesh.vertices = vertices;
            _mesh.uv = uvs;
            _mesh.triangles = triangles;

            // Set up vertex colors if needed
            if (_colors != null) {
                Color[] vertexColors = new Color[vertexCount];
                _mesh.colors = vertexColors;
            }
        }

        /// <summary>
        /// Generates quads in the mesh based on the current positions, rotations, scales, and colors.
        /// This method avoids allocations by reusing the existing mesh arrays.
        /// </summary>
        public void Generate() {
            if (_mesh == null || _positions == null || _positions.Length == 0)
                return;

            // Get existing mesh data
            Vector3[] vertices = _mesh.vertices;
            Vector2[] uvs = _mesh.uv;

            // Calculate how many quads we can create
            int quadCapacity = vertices.Length / 4;
            int quadCount = Mathf.Min(_positions.Length, quadCapacity);

            // Prepare colors array if needed
            Color[] vertexColors = null;
            bool hasColors = _colors != null && _colors.Length > 0;
            if (hasColors) {
                if (_mesh.colors != null && _mesh.colors.Length == vertices.Length) {
                    vertexColors = _mesh.colors;
                } else {
                    vertexColors = new Color[vertices.Length];
                }
            }

            // Use NativeArray and Reinterpret to access float2 data as Vector2
            unsafe {
                fixed (float2* posPtr = _positions) {
                    // Create a native array that references the original data without copying
                    var positionsNative = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<float2>(
                        posPtr, _positions.Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref positionsNative,
                        AtomicSafetyHandle.Create());
#endif

                    // Update vertices for each quad
                    for (int i = 0; i < quadCount; i++) {
                        float2 pos = positionsNative[i];
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
                        float2 bl = new float2(-halfSize, -halfSize);
                        float2 br = new float2(halfSize, -halfSize);
                        float2 tr = new float2(halfSize, halfSize);
                        float2 tl = new float2(-halfSize, halfSize);

                        // Rotate corners
                        bl = new float2(bl.x * cos - bl.y * sin, bl.x * sin + bl.y * cos);
                        br = new float2(br.x * cos - br.y * sin, br.x * sin + br.y * cos);
                        tr = new float2(tr.x * cos - tr.y * sin, tr.x * sin + tr.y * cos);
                        tl = new float2(tl.x * cos - tl.y * sin, tl.x * sin + tl.y * cos);

                        // Apply position offset
                        vertices[vertexOffset + 0] = new Vector3(pos.x + bl.x, pos.y + bl.y, 0); // Bottom-left
                        vertices[vertexOffset + 1] = new Vector3(pos.x + br.x, pos.y + br.y, 0); // Bottom-right
                        vertices[vertexOffset + 2] = new Vector3(pos.x + tr.x, pos.y + tr.y, 0); // Top-right
                        vertices[vertexOffset + 3] = new Vector3(pos.x + tl.x, pos.y + tl.y, 0); // Top-left

                        // Set up UVs (0,0 to 1,1 for each quad)
                        uvs[vertexOffset + 0] = new Vector2(0, 0);
                        uvs[vertexOffset + 1] = new Vector2(1, 0);
                        uvs[vertexOffset + 2] = new Vector2(1, 1);
                        uvs[vertexOffset + 3] = new Vector2(0, 1);

                        // Set colors if available
                        if (hasColors && i < _colors.Length) {
                            float4 color = _colors[i];
                            Color unityColor = new Color(color.x, color.y, color.z, color.w);
                            vertexColors[vertexOffset + 0] = unityColor;
                            vertexColors[vertexOffset + 1] = unityColor;
                            vertexColors[vertexOffset + 2] = unityColor;
                            vertexColors[vertexOffset + 3] = unityColor;
                        }
                    }

                    // Set remaining vertices to zero if needed
                    for (int i = quadCount * 4; i < vertices.Length; i++) {
                        vertices[i] = Vector3.zero;
                        uvs[i] = Vector2.zero;
                        if (hasColors) {
                            vertexColors[i] = Color.clear;
                        }
                    }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    AtomicSafetyHandle.Release(NativeArrayUnsafeUtility.GetAtomicSafetyHandle(positionsNative));
#endif
                }
            }

            // Update the mesh (does not allocate as we're passing the same arrays back)
            _mesh.vertices = vertices;
            _mesh.uv = uvs;
            if (hasColors) {
                _mesh.colors = vertexColors;
            }

            // Recalculate bounds
            _mesh.RecalculateBounds();
        }
    }
}