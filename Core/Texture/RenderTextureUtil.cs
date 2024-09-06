using UnityEngine;

namespace Spark2D {
    public class RenderTextureUtil {
        /// <summary>
        /// Initialize a single channel 32-bit floating point RenderTexture
        /// (RenderTextureFormat.RFloat)
        /// </summary>
        public static RenderTexture SingleChannelRT32(int width, int height) {
            return new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        }
    }
}