using System;
using UnityEngine;

namespace Spark2D {
    /// <summary>
    /// Specifies the method used to determine the number of points to generate.
    /// </summary>
    public enum PointGenerationMode {
        /// <summary>
        /// Generate a specific number of points along the curve.
        /// </summary>
        Count,

        /// <summary>
        /// Generate points with a specific spacing along the curve.
        /// </summary>
        Spacing
    }

    /// <summary>
    /// Creates Bezier curves from control points with configurable point distribution.
    /// API is designed for zero-allocation during Generate() calls.
    /// </summary>
    public class CurveMaker {
        // Core data
        Vector2[] _controlPoints;
        Vector2[] _outputPoints;

        // Point generation settings
        int _count;
        float _spacing;
        float _jitter;
        PointGenerationMode _pointGenerationMode;
        bool _evenSpacing;

        // Temporary buffers for De Casteljau's algorithm
        float[] _tempX;
        float[] _tempY;

        // Arc length parameterization cache
        int _sampleCount;
        Vector2[] _cachedSamplePoints;
        float[] _cachedArcLengths;
        float _cachedTotalLength;
        bool _cacheValid;
        const int _minSampleMultiplier = 4; // Minimum ratio of samples to output points
        const int _minSampleCount = 100; // Absolute minimum sample count

        /// <summary>
        /// Creates a new curve maker with the specified control points and generation parameters.
        /// </summary>
        /// <param name="controlPoints">The control points defining the Bezier curve.</param>
        /// <param name="count">The number of points to generate when using Count mode.</param>
        /// <param name="spacing">The distance between points when using Spacing mode.</param>
        /// <param name="jitter">The maximum random displacement radius for each generated point.</param>
        /// <param name="mode">The mode determining how points are distributed.</param>
        /// <param name="evenSpacing">Whether to distribute points evenly along the curve's arc length.</param>
        public CurveMaker(
            Vector2[] controlPoints = null,
            int count = 20,
            float spacing = 0.1f,
            float jitter = 0f,
            PointGenerationMode mode = PointGenerationMode.Count,
            bool evenSpacing = true) {
            // Set initial values
            _count = 0; // Will be set by Count property
            _spacing = spacing;
            _jitter = jitter;
            _pointGenerationMode = mode;
            _evenSpacing = evenSpacing;
            _sampleCount = 0; // Will be set by Count property

            // Initialize properties (which will handle array allocation)
            Count = count;
            ControlPoints = controlPoints == null ? new[] { new Vector2(-1, 0), new Vector2(1, 0) } : controlPoints;
        }

        #region // MARK: - Properties
        /// <summary>
        /// Gets or sets the control points defining the Bezier curve.
        /// </summary>
        public Vector2[] ControlPoints {
            get => _controlPoints;
            set {
                if (value == null || value.Length < 2)
                    throw new ArgumentException("Control points must have at least 2 points");

                // Resize arrays only when needed
                if (_controlPoints == null || _controlPoints.Length != value.Length) {
                    _controlPoints = new Vector2[value.Length];
                    _tempX = new float[value.Length];
                    _tempY = new float[value.Length];
                }

                // Copy control points
                for (int i = 0; i < value.Length; i++) {
                    _controlPoints[i] = value[i];
                }

                _cacheValid = false;
            }
        }

        /// <summary>
        /// Gets the generated curve points. Available after calling Generate().
        /// </summary>
        public Vector2[] OutputPoints => _outputPoints;

        /// <summary>
        /// Gets or sets the number of points to generate when using Count mode.
        /// </summary>
        public int Count {
            get => _count;
            set {
                if (value < 2)
                    throw new ArgumentException("Count must be at least 2");

                if (_count != value) {
                    _count = value;

                    // Only allocate new output array if in Count mode
                    if (_pointGenerationMode == PointGenerationMode.Count) {
                        _outputPoints = new Vector2[value];
                    }

                    // Calculate new sample count based on output point count
                    int newSampleCount = Math.Max(_minSampleCount, value * _minSampleMultiplier);

                    // Only reallocate sample arrays if needed
                    if (newSampleCount != _sampleCount) {
                        _sampleCount = newSampleCount;
                        _cachedSamplePoints = new Vector2[_sampleCount];
                        _cachedArcLengths = new float[_sampleCount];
                    }

                    _cacheValid = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the distance between generated points when using Spacing mode.
        /// </summary>
        public float Spacing {
            get => _spacing;
            set {
                if (value <= 0)
                    throw new ArgumentException("Spacing must be greater than zero");

                if (!Mathf.Approximately(_spacing, value)) {
                    _spacing = value;
                    _cacheValid = false;

                    // No need to reallocate arrays here since point count is determined at generation time
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum random displacement radius for each generated point.
        /// A value of 0 means no jitter is applied.
        /// </summary>
        public float Jitter {
            get => _jitter;
            set {
                if (value < 0)
                    throw new ArgumentException("Jitter must be non-negative");

                _jitter = value;
            }
        }

        /// <summary>
        /// Gets or sets whether to use count or spacing to determine point distribution.
        /// </summary>
        public PointGenerationMode PointGenerationMode {
            get => _pointGenerationMode;
            set {
                if (_pointGenerationMode != value) {
                    _pointGenerationMode = value;

                    // When changing modes, we may need to resize the output array
                    if (value == PointGenerationMode.Count && (_outputPoints == null || _outputPoints.Length != _count)) {
                        _outputPoints = new Vector2[_count];
                    }
                    // if (value == PointGenerationMode.Spacing) {
                    //     UpdateCache();
                    //     var pointCount = Math.Max(2, (int)Math.Ceiling(_cachedTotalLength / _spacing) + 1);
                    //     _outputPoints = new Vector2[pointCount];
                    // }

                    _cacheValid = false;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to distribute points evenly along the curve's arc length (true) 
        /// or with uniform parameter spacing (false).
        /// </summary>
        public bool EvenSpacing {
            get => _evenSpacing;
            set => _evenSpacing = value;
        }
        #endregion

        #region // MARK: - Public
        /// <summary>
        /// Calculates a single point on the Bezier curve at parameter t (0 to 1).
        /// </summary>
        public Vector2 ComputePoint(float t) {
            t = Mathf.Clamp01(t);

            // Copy control points to temp arrays
            for (int k = 0; k < _controlPoints.Length; k++) {
                _tempX[k] = _controlPoints[k].x;
                _tempY[k] = _controlPoints[k].y;
            }

            // Apply De Casteljau's algorithm
            for (int j = _controlPoints.Length - 1; j > 0; j--) {
                for (int k = 0; k < j; k++) {
                    _tempX[k] = Mathf.Lerp(_tempX[k], _tempX[k + 1], t);
                    _tempY[k] = Mathf.Lerp(_tempY[k], _tempY[k + 1], t);
                }
            }

            return new Vector2(_tempX[0], _tempY[0]);
        }

        /// <summary>
        /// Generates curve points based on the current settings.
        /// </summary>
        public void Generate() {
            if (_controlPoints == null || _controlPoints.Length < 2)
                return;

            // Ensure the cache is up to date for arc length calculations
            if (_evenSpacing && !UpdateCache())
                return;

            // Determine point count based on generation mode
            int pointCount;

            if (_pointGenerationMode == PointGenerationMode.Count) {
                pointCount = _count;
            } else { // Spacing mode
                if (!_evenSpacing) {
                    // For uniform parameter, approximate curve length
                    if (!UpdateCache())
                        return;
                }

                // Calculate point count based on curve length and spacing
                pointCount = Math.Max(2, (int)Math.Ceiling(_cachedTotalLength / _spacing) + 1);

                // Resize output array if needed
                if (_outputPoints == null || _outputPoints.Length != pointCount) {
                    _outputPoints = new Vector2[pointCount];
                }
            }

            // Generate points based on spacing method
            if (_evenSpacing) {
                GenerateEvenlySpaced(pointCount);
            } else {
                GenerateUniformParameter(pointCount);
            }

            ApplyJitter();
        }

        /// <summary>
        /// Gets the total length of the curve.
        /// </summary>
        public float GetTotalLength() {
            if (!UpdateCache())
                return 0;

            return _cachedTotalLength;
        }
        #endregion

        #region // MARK: - Private
        /// <summary>
        /// Internal method to generate points with uniform parameter spacing.
        /// </summary>
        void GenerateUniformParameter(int pointCount) {
            float step = 1f / (pointCount - 1);

            for (int i = 0; i < pointCount; i++) {
                _outputPoints[i] = ComputePoint(i * step);
            }
        }

        /// <summary>
        /// Internal method to generate points with even arc length spacing.
        /// </summary>
        void GenerateEvenlySpaced(int pointCount) {
            // First and last points
            _outputPoints[0] = _controlPoints[0];
            _outputPoints[pointCount - 1] = _controlPoints[_controlPoints.Length - 1];

            if (_pointGenerationMode == PointGenerationMode.Count) {
                // Count-based: distribute points evenly along total length
                float arcLengthStep = _cachedTotalLength / (pointCount - 1);

                for (int i = 1; i < pointCount - 1; i++) {
                    _outputPoints[i] = GetPointAtArcLength(i * arcLengthStep);
                }
            } else {
                // Spacing-based: place points at regular intervals
                float currentDistance = 0;

                for (int i = 1; i < pointCount - 1; i++) {
                    currentDistance += _spacing;
                    _outputPoints[i] = GetPointAtArcLength(currentDistance);
                }
            }
        }

        /// <summary>
        /// Gets a point on the curve at a specific arc length from the start.
        /// </summary>
        Vector2 GetPointAtArcLength(float targetArcLength) {
            // Clamp to the curve length
            targetArcLength = Mathf.Clamp(targetArcLength, 0, _cachedTotalLength);

            // Binary search to find the segment containing target arc length
            int low = 0;
            int high = _sampleCount - 1;

            while (low < high - 1) {
                int mid = (low + high) / 2;
                if (_cachedArcLengths[mid] < targetArcLength)
                    low = mid;
                else
                    high = mid;
            }

            // Handle boundary cases
            if (low == 0 && targetArcLength <= _cachedArcLengths[0])
                return _cachedSamplePoints[0];

            if (high == _sampleCount - 1 && targetArcLength >= _cachedArcLengths[_sampleCount - 1])
                return _cachedSamplePoints[_sampleCount - 1];

            // Interpolate between the two closest sample points
            float lowArcLength = _cachedArcLengths[low];
            float highArcLength = _cachedArcLengths[high];
            float segmentLength = highArcLength - lowArcLength;

            if (segmentLength < float.Epsilon)
                return _cachedSamplePoints[low];

            // Calculate interpolation factor and parameter value
            float factor = (targetArcLength - lowArcLength) / segmentLength;
            float t = Mathf.Lerp(low / (float)(_sampleCount - 1), high / (float)(_sampleCount - 1), factor);

            return ComputePoint(t);
        }

        /// <summary>
        /// Builds the arc length cache for curve parameterization.
        /// </summary>
        bool UpdateCache() {
            if (_cacheValid) return true;
            if (_controlPoints == null || _controlPoints.Length < 2) return false;

            float totalLength = 0;
            Vector2 prevPoint = ComputePoint(0);
            _cachedSamplePoints[0] = prevPoint;
            _cachedArcLengths[0] = 0;

            float step = 1f / (_sampleCount - 1);

            for (int i = 1; i < _sampleCount; i++) {
                float t = i * step;
                Vector2 currentPoint = ComputePoint(t);
                _cachedSamplePoints[i] = currentPoint;

                float segmentLength = Vector2.Distance(prevPoint, currentPoint);
                totalLength += segmentLength;
                _cachedArcLengths[i] = totalLength;

                prevPoint = currentPoint;
            }

            _cachedTotalLength = totalLength;
            _cacheValid = true;
            return true;
        }

        /// <summary>
        /// Applies random displacement to generated points within the jitter radius.
        /// </summary>
        void ApplyJitter() {
            if (_jitter <= 0 || _outputPoints == null)
                return;

            for (int i = 0; i < _outputPoints.Length; i++) {
                // Skip first and last points to preserve curve endpoints
                if (i == 0 || i == _outputPoints.Length - 1)
                    continue;

                // Generate random displacement within the jitter radius
                float angle = UnityEngine.Random.Range(0f, 2f * Mathf.PI);
                float distance = UnityEngine.Random.Range(0f, _jitter);

                Vector2 offset = new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );

                _outputPoints[i] += offset;
            }
        }
        #endregion
    }
}