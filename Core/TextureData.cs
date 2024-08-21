using Unity.Collections;
using Unity.Mathematics;
using System;
using UnityEngine;

namespace Spark2D {
    public enum Precision {
        Byte,
        Half,
        Float
    }

    [Flags]
    public enum Channels {
        R = 1,
        G = 2,
        B = 4,
        A = 8,
        RG = R | G,
        RGB = R | G | B,
        RGBA = R | G | B | A
    }

    public class TextureData {
        public Precision Precision { get; private set; }
        public Channels Channels { get; private set; }
        public Texture2D Texture2D { get; private set; }
        public int2 Size { get; private set; }

        public int ByteSize => Size.x * Size.y * GetChannelCount() * GetPrecisionSize();

        Texture2D _cachedTexture;

        public TextureData(int2 size, Precision precision, Channels channels, FilterMode filterMode = FilterMode.Bilinear) {
            Size = size;
            Precision = precision;
            Channels = channels;
            Texture2D = new Texture2D(Size.x, Size.y, GetUnityTextureFormat(), false);
            Texture2D.filterMode = filterMode;
        }

        public NativeArray<T> GetRawTextureData<T>() where T : struct {
            return Texture2D.GetRawTextureData<T>();
        }

        TextureFormat GetUnityTextureFormat() {
            return (Precision, Channels) switch {
                (Precision.Byte, Channels.R) => TextureFormat.R8,
                (Precision.Byte, Channels.RG) => TextureFormat.RG16,
                (Precision.Byte, Channels.RGB) => TextureFormat.RGB24,
                (Precision.Byte, Channels.RGBA) => TextureFormat.RGBA32,
                (Precision.Half, Channels.R) => TextureFormat.RHalf,
                (Precision.Half, Channels.RG) => TextureFormat.RGHalf,
                // (Precision.Half, Channels.RGB) => TextureFormat.RGB48, // GPUs generally don’t support RGB render targets
                (Precision.Half, Channels.RGBA) => TextureFormat.RGBAHalf,
                (Precision.Float, Channels.R) => TextureFormat.RFloat,
                (Precision.Float, Channels.RG) => TextureFormat.RGFloat,
                // (Precision.Float, Channels.RGB) => TextureFormat.RGBFloat, // GPUs generally don’t support RGB render targets
                (Precision.Float, Channels.RGBA) => TextureFormat.RGBAFloat,
                _ => throw new System.ArgumentException("Unsupported precision and channel combination")
            };
        }

        int GetChannelCount() {
            return math.countbits((uint)Channels);
        }

        int GetPrecisionSize() {
            return Precision switch {
                Precision.Byte => 1,
                Precision.Half => 2,
                Precision.Float => 4,
                _ => throw new ArgumentException("Invalid precision")
            };
        }
    }
}