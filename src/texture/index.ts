import { Texture2D, TextureFormat } from "UnityEngine"

export { tiling } from "./tiling"

export function tex_r_byte(width?: number, height?: number): Texture2D {
    width = width ?? 512;
    height = height ?? width;
    return new Texture2D(width, height, TextureFormat.R8, false);
}

export function tex_r_half(width?: number, height?: number): Texture2D {
    width = width ?? 512;
    height = height ?? width;
    return new Texture2D(width, height, TextureFormat.RHalf, false);
}

export function tex_r_float(width?: number, height?: number): Texture2D {
    width = width ?? 512;
    height = height ?? width;
    return new Texture2D(width, height, TextureFormat.RFloat, false);
}