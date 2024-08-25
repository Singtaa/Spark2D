using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Spark2D {
    public static class TextureNativeUtil {
        [BurstCompile]
        public struct ColorFillJob : IJobParallelFor {
            public NativeArray<Color> data;
            public Color color;

            public static JobHandle Fill(Texture2D texture, Color color, int innerLoopBatchSize = 64, JobHandle dependsOn = default) {
                var data = texture.GetRawTextureData<Color>();
                var job = new ColorFillJob {
                    data = data,
                    color = color,
                };
                return job.Schedule(data.Length, innerLoopBatchSize, dependsOn);
            }

            public void Execute(int index) {
                data[index] = color;
            }
        }

        [BurstCompile]
        public struct Color32FillJob : IJobParallelFor {
            public NativeArray<Color32> data;
            public Color32 color;

            public static JobHandle Fill(Texture2D texture, Color32 color, int innerLoopBatchSize = 64, JobHandle dependsOn = default) {
                var data = texture.GetRawTextureData<Color32>();
                var job = new Color32FillJob {
                    data = data,
                    color = color,
                };
                return job.Schedule(data.Length, innerLoopBatchSize, dependsOn);
            }

            public void Execute(int index) {
                data[index] = color;
            }
        }

        [BurstCompile]
        public struct ExtractChannelByteFromColor32 : IJobParallelFor {
            [ReadOnly] public NativeArray<Color32> input;
            public NativeArray<byte> output;
            public int channel;

            public void Execute(int index) {
                output[index] = input[index][channel];
            }
        }

        public static JobHandle ExtractMaskFromChannel(Texture2D texture, ref NativeArray<byte> output, int channel) {
            var input = texture.GetRawTextureData<Color32>();
            return ExtractMaskFromChannel(input, ref output, channel);
        }

        public static JobHandle ExtractMaskFromChannel(NativeArray<Color32> input, ref NativeArray<byte> output, int channel) {
            var job = new ExtractChannelByteFromColor32 {
                input = input,
                output = output,
                channel = channel,
            };
            return job.Schedule(output.Length, 64);
        }
    }
}