
declare namespace CS {
    // const __keep_incompatibility: unique symbol;
    // 
    // interface $Ref<T> {
    //     value: T
    // }
    // namespace System {
    //     interface Array$1<T> extends System.Array {
    //         get_Item(index: number):T;
    //         
    //         set_Item(index: number, value: T):void;
    //     }
    // }
    // interface $Task<T> {}
        enum EBlendMode
        { Normal = 0, Dissolve = 1, Overlay = 2, Multiply = 3, Difference = 4, Darken = 5, ColorBurn = 6, LinearBurn = 7, DarkerColor = 8, Lighten = 9, Screen = 10, ColorDodge = 11, LinearDodge = 12, LighterColor = 13, SoftLight = 14, HardLight = 15, VividLight = 16, LinearLight = 17, PinLight = 18, HardMix = 19, Exclusion = 20, Subtract = 21, Divide = 22, Hue = 23, Saturation = 24, Color = 25, Luminosity = 26 }
        class ComputeShaderTester extends UnityEngine.MonoBehaviour
        {
            protected [__keep_incompatibility]: never;
            public shader : UnityEngine.ComputeShader
            public tex1 : UnityEngine.Texture2D
            public tex2 : UnityEngine.Texture2D
            public result : UnityEngine.RenderTexture
            public tex1Width : number
            public tex1Height : number
            public tex1Color : UnityEngine.Color
            public tex2Position : UnityEngine.Vector2
            public tex2Rotation : number
            public tex2Scale : UnityEngine.Vector2
            public blendMode : number
            public constructor ()
        }
        enum NoiseType
        { Simplex = 0, SNoise = 1, FBM = 2 }
        class NoiseTextureTester extends UnityEngine.MonoBehaviour
        {
            protected [__keep_incompatibility]: never;
            public filterMode : UnityEngine.FilterMode
            public textureWidth : number
            public textureHeight : number
            public scaling : number
            public noiseType : NoiseType
            public seed : number
            public constructor ()
        }
        namespace Spark2D {
        class ObjectMappingPair extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public obj : UnityEngine.Object
            public name : string
            public constructor ($obj: UnityEngine.Object, $m: string)
        }
        class Depot extends UnityEngine.ScriptableObject
        {
            protected [__keep_incompatibility]: never;
            public contents : System.Array$1<Spark2D.ObjectMappingPair>
            public Get ($name: string) : any
            public constructor ()
        }
        interface INoiseGenerator
        {
            Generate ($x: number, $y: number) : number
        }
        class SimplexNoise extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static get Seed(): number;
            public static set Seed(value: number);
            public static Generate ($x: number, $y: number) : number
            public constructor ()
        }
        class SimplexNoise_Heikki extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static perm : System.Array$1<number>
            public static Generate ($x: number, $y: number) : number
        }
        enum RGBFillMode
        { White = 0, Black = 1, Distance = 2, Source = 3 }
        class SDFTextureGenerator extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static Generate ($source: UnityEngine.Texture2D, $destination: UnityEngine.Texture2D, $maxInside: number, $maxOutside: number, $postProcessDistance: number, $rgbMode: Spark2D.RGBFillMode) : void
        }
        class RenderTextureUtil extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static SingleChannelRT32 ($width: number, $height: number) : UnityEngine.RenderTexture
            public constructor ()
        }
        class TextureNativeUtil extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static ExtractMaskFromChannel ($texture: UnityEngine.Texture2D, $output: $Ref<Unity.Collections.NativeArray$1<number>>, $channel: number) : Unity.Jobs.JobHandle
            public static ExtractMaskFromChannel ($input: Unity.Collections.NativeArray$1<UnityEngine.Color32>, $output: $Ref<Unity.Collections.NativeArray$1<number>>, $channel: number) : Unity.Jobs.JobHandle
        }
        enum Precision
        { Byte = 0, Half = 1, Float = 2 }
        enum Channels
        { R = 1, G = 2, B = 4, A = 8, RG = 3, RGB = 7, RGBA = 15 }
        class TextureData extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public get Precision(): Spark2D.Precision;
            public get Channels(): Spark2D.Channels;
            public get Texture2D(): UnityEngine.Texture2D;
            public get Size(): Unity.Mathematics.int2;
            public get ByteSize(): number;
            public constructor ($size: Unity.Mathematics.int2, $precision: Spark2D.Precision, $channels: Spark2D.Channels, $filterMode?: UnityEngine.FilterMode)
        }
        class PairMappingAttribute extends UnityEngine.PropertyAttribute implements System.Runtime.InteropServices._Attribute
        {
            protected [__keep_incompatibility]: never;
            public from : string
            public to : string
            public separator : string
            public label : string
            public constructor ($from: string, $to: string, $separator?: string, $label?: string)
        }
        class noise extends System.Object
        {
            protected [__keep_incompatibility]: never;
            public static fbm ($x: number, $y: number, $octaves?: number, $lacunarity?: number, $gain?: number) : number
            public static simplex ($x: number, $y: number) : number
        }
    }
    namespace Spark2D.TextureNativeUtil {
        class ColorFillJob extends System.ValueType implements Unity.Jobs.IJobParallelFor
        {
            protected [__keep_incompatibility]: never;
            public data : Unity.Collections.NativeArray$1<UnityEngine.Color>
            public color : UnityEngine.Color
            public static Fill ($texture: UnityEngine.Texture2D, $color: UnityEngine.Color, $innerLoopBatchSize?: number, $dependsOn?: Unity.Jobs.JobHandle) : Unity.Jobs.JobHandle
            public Execute ($index: number) : void
        }
        class Color32FillJob extends System.ValueType implements Unity.Jobs.IJobParallelFor
        {
            protected [__keep_incompatibility]: never;
            public data : Unity.Collections.NativeArray$1<UnityEngine.Color32>
            public color : UnityEngine.Color32
            public static Fill ($texture: UnityEngine.Texture2D, $color: UnityEngine.Color32, $innerLoopBatchSize?: number, $dependsOn?: Unity.Jobs.JobHandle) : Unity.Jobs.JobHandle
            public Execute ($index: number) : void
        }
        class ExtractChannelByteFromColor32 extends System.ValueType implements Unity.Jobs.IJobParallelFor
        {
            protected [__keep_incompatibility]: never;
            public input : Unity.Collections.NativeArray$1<UnityEngine.Color32>
            public output : Unity.Collections.NativeArray$1<number>
            public channel : number
            public Execute ($index: number) : void
        }
    }
}

declare const csDepot: CS.Spark2D.Depot;