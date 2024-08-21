using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Spark2D {
    public static class TextureNativeUtil {
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