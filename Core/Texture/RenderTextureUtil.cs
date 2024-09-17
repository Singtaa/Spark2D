using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;

namespace Spark2D {
    /// <summary>
    /// Made this helper class in C# primarily to work around this problem:
    /// https://github.com/Tencent/puerts/issues/1821
    /// Initializing a RenderTexture in Puerts/JS crashes Unity Editor.
    /// </summary>
    public class RenderTextureUtil {
        /// <summary>
        /// Initialize a single channel 32-bit floating point RenderTexture
        /// (RenderTextureFormat.RFloat)
        /// </summary>
        public static RenderTexture SingleChannelRT32(int width, int height) {
            return new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
        }

        public static RenderTexture CreateRFloatRT(int width, int height) {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.RFloat);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        public static RenderTexture CreateRT(int width, int height) {
            return CreateRT(width, height, Application.isMobilePlatform ? RenderTextureFormat.ARGBHalf : RenderTextureFormat.ARGBFloat);
        }

        public static RenderTexture CreateRT(int width, int height, RenderTextureFormat format) {
            var rt = new RenderTexture(width, height, 0, format);
            rt.depthStencilFormat = GraphicsFormat.None;
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        public static Texture2D CreateSinglePixelTexture(Color color) {
            var tex = new Texture2D(1, 1, Application.isMobilePlatform ? TextureFormat.RGBAHalf : TextureFormat.RGBAFloat, false);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }

        public static RenderTexture Clone(RenderTexture rt) {
            var clone = new RenderTexture(rt.width, rt.height, rt.depth, rt.format) {
                antiAliasing = rt.antiAliasing,
                filterMode = rt.filterMode,
                wrapMode = rt.wrapMode,
                useMipMap = rt.useMipMap,
                autoGenerateMips = rt.autoGenerateMips,
                anisoLevel = rt.anisoLevel,
                volumeDepth = rt.volumeDepth,
                dimension = rt.dimension,
                vrUsage = rt.vrUsage,
                memorylessMode = rt.memorylessMode,
                enableRandomWrite = rt.enableRandomWrite,
                useDynamicScale = rt.useDynamicScale,
                bindTextureMS = rt.bindTextureMS,
            };
            clone.enableRandomWrite = true;
            clone.Create();
            Graphics.Blit(rt, clone);
            return clone;
        }

        public static RenderTexture InitNew(RenderTexture rt) {
            return InitNew(rt, rt.format);
        }

        public static RenderTexture InitNew(RenderTexture rt, RenderTextureFormat format) {
            var clone = new RenderTexture(rt.width, rt.height, rt.depth, format) {
                antiAliasing = rt.antiAliasing,
                filterMode = rt.filterMode,
                wrapMode = rt.wrapMode,
                useMipMap = rt.useMipMap,
                autoGenerateMips = rt.autoGenerateMips,
                anisoLevel = rt.anisoLevel,
                volumeDepth = rt.volumeDepth,
                dimension = rt.dimension,
                vrUsage = rt.vrUsage,
                memorylessMode = rt.memorylessMode,
                enableRandomWrite = rt.enableRandomWrite,
                useDynamicScale = rt.useDynamicScale,
                bindTextureMS = rt.bindTextureMS,
            };
            clone.enableRandomWrite = true;
            clone.Create();
            return clone;
        }
    }
}