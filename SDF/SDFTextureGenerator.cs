/**
 * Original code by CatlikeCoding and Stefan Gustavson.
 * Converted to Burst+Jobs by @Singtaa
 */

using System;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace Spark2D {
    public enum RGBFillMode {
        White,
        Black,
        Distance,
        Source
    }

    public static class SDFTextureGenerator {
        static NativeArray<Color32> _srcColors;
        static NativeArray<Color32> _dstColors;

        static NativeArray<float> _distances;
        static NativeArray<float> _alphas;
        static NativeArray<float2> _gradients;
        static NativeArray<int> _dXs;
        static NativeArray<int> _dYs;

        static int _width, _height;

        public static void Generate(Texture2D source, Texture2D destination, float maxInside, float maxOutside, float postProcessDistance, RGBFillMode rgbMode) {
            if (source.height != destination.height || source.width != destination.width) {
                throw new Exception("Source and destination textures must be the same size.");
            }
            _width = source.width;
            _height = source.height;
            int totalPixels = _width * _height;

            _srcColors = source.GetRawTextureData<Color32>();
            _dstColors = destination.GetRawTextureData<Color32>();

            _distances = new NativeArray<float>(totalPixels, Allocator.TempJob);
            _alphas = new NativeArray<float>(totalPixels, Allocator.TempJob);
            _gradients = new NativeArray<float2>(totalPixels, Allocator.TempJob);
            _dXs = new NativeArray<int>(totalPixels, Allocator.TempJob);
            _dYs = new NativeArray<int>(totalPixels, Allocator.TempJob);

            int x, y;
            float scale;
            Color32 defaultColor = rgbMode == RGBFillMode.White ? Color.white : Color.black;

            JobHandle dep = default(JobHandle);

            if (maxInside > 0f) {
                dep = SetComplementAlpha(dep);
                dep = ComputeEdgeGradients(dep);
                dep = GenerateDistanceTransform(dep);
                if (postProcessDistance > 0f) {
                    dep = PostProcess(postProcessDistance, dep);
                }
                scale = 1f / maxInside;
                dep = SetDestination(defaultColor, scale, dep);
            }

            if (maxOutside > 0f) {
                dep = SetSameAlpha(dep);
                dep = ComputeEdgeGradients(dep);
                dep = GenerateDistanceTransform(dep);
                if (postProcessDistance > 0f) {
                    dep = PostProcess(postProcessDistance, dep);
                }
                scale = 1f / maxOutside;
                if (maxInside > 0f) {
                    dep = SetDestinationWithBiasAndScale(defaultColor, scale, dep);
                } else {
                    dep = SetInverseDestination(defaultColor, scale, dep);
                }
            }

            if (rgbMode == RGBFillMode.Distance) {
                dep = SetDestinationFillModeDistance(dep);
            } else if (rgbMode == RGBFillMode.Source) {
                dep = SetDestinationFillModeSource(dep);
            }

            dep.Complete();

            destination.Apply();

            _distances.Dispose();
            _alphas.Dispose();
            _gradients.Dispose();
            _dXs.Dispose();
            _dYs.Dispose();
        }

        #region Set Destination Jobs
        [BurstCompile]
        struct SetDestinationJob : IJobParallelFor {
            [ReadOnly] public NativeArray<float> distances;
            public NativeArray<Color32> destination;
            public Color32 defaultColor;
            public float scale;

            public void Execute(int index) {
                defaultColor.a = (byte)(Mathf.Clamp01(distances[index] * scale) * 255);
                destination[index] = defaultColor;
            }
        }

        static JobHandle SetDestination(Color32 defaultColor, float scale, JobHandle handle = default(JobHandle)) {
            var job = new SetDestinationJob {
                distances = _distances,
                destination = _dstColors,
                defaultColor = defaultColor,
                scale = scale
            };
            return job.Schedule(_dstColors.Length, 64, handle);
        }
        
        [BurstCompile]
        struct SetInverseDestinationJob : IJobParallelFor {
            [ReadOnly] public NativeArray<float> distances;
            public NativeArray<Color32> destination;
            public Color32 defaultColor;
            public float scale;

            public void Execute(int index) {
                defaultColor.a = (byte)(Mathf.Clamp01(1f - distances[index] * scale) * 255);
                destination[index] = defaultColor;
            }
        }

        static JobHandle SetInverseDestination(Color32 defaultColor, float scale, JobHandle handle = default(JobHandle)) {
            var job = new SetInverseDestinationJob {
                distances = _distances,
                destination = _dstColors,
                defaultColor = defaultColor,
                scale = scale
            };
            return job.Schedule(_dstColors.Length, 64, handle);
        }

        [BurstCompile]
        struct SetDestinationWithBiasAndScaleJob : IJobParallelFor {
            [ReadOnly] public NativeArray<float> distances;
            public NativeArray<Color32> destination;
            public Color32 defaultColor;
            public float scale;

            public void Execute(int index) {
                var a = Mathf.Clamp01(distances[index] * scale);
                a = 0.5f + (destination[index].a / 255f - a) * 0.5f;
                defaultColor.a = (byte)(a * 255);
                destination[index] = defaultColor;
            }
        }

        static JobHandle SetDestinationWithBiasAndScale(Color32 defaultColor, float scale, JobHandle handle = default(JobHandle)) {
            var job = new SetDestinationWithBiasAndScaleJob {
                distances = _distances,
                destination = _dstColors,
                defaultColor = defaultColor,
                scale = scale
            };
            return job.Schedule(_dstColors.Length, 64, handle);
        }

        [BurstCompile]
        struct SetDestinationFillModeDistanceJob : IJobParallelFor {
            public NativeArray<Color32> destination;

            public void Execute(int index) {
                var c = destination[index];
                c.r = c.a;
                c.g = c.a;
                c.b = c.a;
                destination[index] = c;
            }
        }

        static JobHandle SetDestinationFillModeDistance(JobHandle handle = default(JobHandle)) {
            var job = new SetDestinationFillModeDistanceJob {
                destination = _dstColors,
            };
            return job.Schedule(_dstColors.Length, 64, handle);
        }

        [BurstCompile]
        struct SetDestinationFillModeSourceJob : IJobParallelFor {
            [ReadOnly] public NativeArray<Color32> source;
            public NativeArray<Color32> destination;

            public void Execute(int index) {
                var c = source[index];
                c.a = destination[index].a;
                destination[index] = c;
            }
        }

        static JobHandle SetDestinationFillModeSource(JobHandle handle = default(JobHandle)) {
            var job = new SetDestinationFillModeSourceJob {
                source = _srcColors,
                destination = _dstColors,
            };
            return job.Schedule(_dstColors.Length, 64, handle);
        }
        #endregion


        [BurstCompile]
        struct SetSameAlphaJob : IJobParallelFor {
            [ReadOnly] public NativeArray<Color32> input;
            public NativeArray<float> output;

            public void Execute(int index) {
                output[index] = input[index].a / 255f;
            }
        }

        static JobHandle SetSameAlpha(JobHandle handle = default(JobHandle)) {
            var job = new SetSameAlphaJob {
                input = _srcColors,
                output = _alphas
            };
            return job.Schedule(_alphas.Length, 64, handle);
        }

        [BurstCompile]
        struct SetComplementAlphaJob : IJobParallelFor {
            [ReadOnly] public NativeArray<Color32> input;
            public NativeArray<float> output;

            public void Execute(int index) {
                output[index] = 1f - input[index].a / 255f;
            }
        }

        static JobHandle SetComplementAlpha(JobHandle handle = default(JobHandle)) {
            var job = new SetComplementAlphaJob {
                input = _srcColors,
                output = _alphas
            };
            return job.Schedule(_alphas.Length, 64, handle);
        }

        [BurstCompile]
        struct ComputeEdgeGradientsJob : IJobParallelFor {
            [ReadOnly] public NativeArray<float> alphas;
            public NativeArray<float2> gradients;
            public int width;
            public int height;
            public float sqrt2;

            public void Execute(int index) {
                int x = index % width;
                int y = index / height;
                if (x > 0 && x < width - 1 && y > 0 && y < height - 1) {
                    int i = index;
                    float g = -_alpha(x - 1, y - 1) - _alpha(x - 1, y + 1) + _alpha(x + 1, y - 1) + _alpha(x + 1, y + 1);
                    var gradient = gradients[i];
                    gradient.x = g + (_alpha(x + 1, y) - _alpha(x - 1, y)) * sqrt2;
                    gradient.y = g + (_alpha(x, y + 1) - _alpha(x, y - 1)) * sqrt2;
                    gradients[i] = math.normalize(gradient);
                }
            }

            float _alpha(int x, int y) {
                var i = y * width + x;
                return alphas[i];
            }
        }

        static JobHandle ComputeEdgeGradients(JobHandle dep = default(JobHandle)) {
            float sqrt2 = Mathf.Sqrt(2f);
            var job = new ComputeEdgeGradientsJob {
                gradients = _gradients,
                alphas = _alphas,
                width = _width,
                height = _height,
                sqrt2 = sqrt2
            };
            return job.Schedule(_gradients.Length, 64, dep);
        }

        [BurstCompile]
        struct GenerateDistanceTransformJob : IJob {
            [ReadOnly] public NativeArray<float> alphas;
            [ReadOnly] public NativeArray<float2> gradients;
            public NativeArray<float> distances;
            public NativeArray<int> dXs;
            public NativeArray<int> dYs;
            public int width;
            public int height;

            public void Execute() { // perform anti-aliased Euclidean distance transform
                int x, y, pi;

                // initialize distances
                for (y = 0; y < height; y++) {
                    for (x = 0; x < width; x++) {
                        pi = _index(x, y);
                        dXs[pi] = 0;
                        dYs[pi] = 0;
                        if (alphas[pi] <= 0f) {
                            // outside
                            distances[pi] = 1000000f;
                        } else if (alphas[pi] < 1f) {
                            // on the edge
                            distances[pi] = ApproximateEdgeDelta(gradients[pi].x, gradients[pi].y, alphas[pi]);
                        } else {
                            // inside
                            distances[pi] = 0f;
                        }
                    }
                }

                // perform 8SSED (eight-points signed sequential Euclidean distance transform)
                // scan up
                for (y = 1; y < height; y++) {
                    // |P.
                    // |XX
                    pi = _index(0, y);
                    if (distances[pi] > 0f) {
                        UpdateDistance(pi, 0, y, 0, -1);
                        UpdateDistance(pi, 0, y, 1, -1);
                    }
                    // -->
                    // XP.
                    // XXX
                    for (x = 1; x < width - 1; x++) {
                        pi = _index(x, y);
                        if (distances[pi] > 0f) {
                            UpdateDistance(pi, x, y, -1, 0);
                            UpdateDistance(pi, x, y, -1, -1);
                            UpdateDistance(pi, x, y, 0, -1);
                            UpdateDistance(pi, x, y, 1, -1);
                        }
                    }
                    // XP|
                    // XX|
                    pi = _index(width - 1, y);
                    if (distances[pi] > 0f) {
                        UpdateDistance(pi, width - 1, y, -1, 0);
                        UpdateDistance(pi, width - 1, y, -1, -1);
                        UpdateDistance(pi, width - 1, y, 0, -1);
                    }
                    // <--
                    // .PX
                    for (x = width - 2; x >= 0; x--) {
                        pi = _index(x, y);
                        if (distances[pi] > 0f) {
                            UpdateDistance(pi, x, y, 1, 0);
                        }
                    }
                }
                // scan down
                for (y = height - 2; y >= 0; y--) {
                    // XX|
                    // .P|
                    pi = _index(width - 1, y);
                    if (distances[pi] > 0f) {
                        UpdateDistance(pi, width - 1, y, 0, 1);
                        UpdateDistance(pi, width - 1, y, -1, 1);
                    }
                    // <--
                    // XXX
                    // .PX
                    for (x = width - 2; x > 0; x--) {
                        pi = _index(x, y);
                        if (distances[pi] > 0f) {
                            UpdateDistance(pi, x, y, 1, 0);
                            UpdateDistance(pi, x, y, 1, 1);
                            UpdateDistance(pi, x, y, 0, 1);
                            UpdateDistance(pi, x, y, -1, 1);
                        }
                    }
                    // |XX
                    // |PX
                    pi = _index(0, y);
                    if (distances[pi] > 0f) {
                        UpdateDistance(pi, 0, y, 1, 0);
                        UpdateDistance(pi, 0, y, 1, 1);
                        UpdateDistance(pi, 0, y, 0, 1);
                    }
                    // -->
                    // XP.
                    for (x = 1; x < width; x++) {
                        pi = _index(x, y);
                        if (distances[pi] > 0f) {
                            UpdateDistance(pi, x, y, -1, 0);
                        }
                    }
                }
            }

            void UpdateDistance(int targetPixelIndex, int x, int y, int oX, int oY) {
                var pi = targetPixelIndex;
                // Pixel neighbor = pixels[x + oX, y + oY];
                // Pixel closest = pixels[x + oX - neighbor.dX, y + oY - neighbor.dY];
                var ni = _index(x + oX, y + oY); // Neighbor index
                var ci = _index(x + oX - dXs[ni], y + oY - dYs[ni]); // Closest index

                if (alphas[ci] == 0f || ci == pi) {
                    // neighbor has no closest yet
                    // or neighbor's closest is p itself
                    return;
                }

                int dX = dXs[ni] - oX;
                int dY = dYs[ni] - oY;
                float distance = Mathf.Sqrt(dX * dX + dY * dY) + ApproximateEdgeDelta(dX, dY, alphas[ci]);
                if (distance < distances[pi]) {
                    distances[pi] = distance;
                    dXs[pi] = dX;
                    dYs[pi] = dY;
                }
            }

            int _index(int x, int y) {
                return y * width + x;
            }
        }

        static JobHandle GenerateDistanceTransform(JobHandle dep = default(JobHandle)) {
            var job = new GenerateDistanceTransformJob {
                alphas = _alphas,
                distances = _distances,
                gradients = _gradients,
                dXs = _dXs,
                dYs = _dYs,
                width = _width,
                height = _height
            };
            return job.Schedule(dep);
        }

        [BurstCompile]
        struct PostProcessJob : IJobParallelFor {
            [ReadOnly] public NativeArray<float> alphas;
            [ReadOnly] public NativeArray<float2> gradients;
            [ReadOnly] public NativeArray<int> dXs;
            [ReadOnly] public NativeArray<int> dYs;
            public NativeArray<float> distances;
            public float maxDistance;
            public int width;
            public int height;

            public void Execute(int index) {
                int pi = index;
                int x = index % width;
                int y = index / height;

                if ((dXs[pi] == 0 && dYs[pi] == 0) || distances[pi] >= maxDistance) {
                    // ignore edge, inside, and beyond max distance
                    return;
                }
                float dX = dXs[pi], dY = dYs[pi];
                int ci = _index(x - dXs[pi], y - dYs[pi]);
                var g = gradients[ci];

                if (g.x == 0f && g.y == 0f) {
                    // ignore unknown gradients (inside)
                    return;
                }

                // compute hit point offset on gradient inside pixel
                float df = ApproximateEdgeDelta(g.x, g.y, alphas[ci]);
                float t = dY * g.x - dX * g.y;
                float u = -df * g.x + t * g.y;
                float v = -df * g.y - t * g.x;

                // use hit point to compute distance
                if (Mathf.Abs(u) <= 0.5f && Mathf.Abs(v) <= 0.5f) {
                    distances[pi] = Mathf.Sqrt((dX + u) * (dX + u) + (dY + v) * (dY + v));
                }
            }

            int _index(int x, int y) {
                return y * width + x;
            }
        }

        static JobHandle PostProcess(float maxDistance, JobHandle dep = default(JobHandle)) { // adjust distances near edges based on the local edge gradient
            var job = new PostProcessJob {
                alphas = _alphas,
                gradients = _gradients,
                dXs = _dXs,
                dYs = _dYs,
                distances = _distances,
                maxDistance = maxDistance,
                width = _width,
                height = _height
            };
            return job.Schedule(_distances.Length, 64, dep);
        }

        static float ApproximateEdgeDelta(float gx, float gy, float a) {
            // (gx, gy) can be either the local pixel gradient or the direction to the pixel

            if (gx == 0f || gy == 0f) {
                // linear function is correct if both gx and gy are zero
                // and still fair if only one of them is zero
                return 0.5f - a;
            }

            // normalize (gx, gy)
            float length = math.sqrt(gx * gx + gy * gy);
            gx = gx / length;
            gy = gy / length;

            // reduce symmetrical equation to first octant only
            // gx >= 0, gy >= 0, gx >= gy
            gx = math.abs(gx);
            gy = math.abs(gy);
            if (gx < gy) {
                float temp = gx;
                gx = gy;
                gy = temp;
            }

            // compute delta
            float a1 = 0.5f * gy / gx;
            if (a < a1) {
                // 0 <= a < a1
                return 0.5f * (gx + gy) - math.sqrt(2f * gx * gy * a);
            }
            if (a < (1f - a1)) {
                // a1 <= a <= 1 - a1
                return (0.5f - a) * gx;
            }
            // 1-a1 < a <= 1
            return -0.5f * (gx + gy) + math.sqrt(2f * gx * gy * (1f - a));
        }
    }
}