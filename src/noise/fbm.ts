import { ComputeShader, Graphics, Mathf, RenderTexture, Shader } from "UnityEngine";

const RESULT: number = Shader.PropertyToID("result");
const SCALE: number = Shader.PropertyToID("scale");
const OFFSET: number = Shader.PropertyToID("offset");
const OCTAVES: number = Shader.PropertyToID("octaves");
const LACUNARITY: number = Shader.PropertyToID("lacunarity");
const GAIN: number = Shader.PropertyToID("gain");

/**
 * Returns an FBM (Fractional Brownian Motion) noise generator
 */
export function fbm(width?: number, height?: number) {
    width = width ?? 512;
    height = height ?? width;
    return new FBM(width, height);
}

export class FBM {
    /**
     * The underlying RenderTexture
     */
    get rt() {
        return this.#texture;
    }

    #threadGroupsX: number;
    #threadGroupsY: number;
    #texture: RenderTexture;
    #shader: ComputeShader;
    #kernel: number;

    #scale: number = 2;
    #offsetX: number = 0;
    #offsetY: number = 0;
    #octaves: number = 5;
    #lacunarity: number = 2;
    #gain: number = 0.5;

    constructor(width: number, height: number) {
        this.#threadGroupsX = Mathf.CeilToInt(width / 8);
        this.#threadGroupsY = Mathf.CeilToInt(height / 8);
        this.#texture = CS.Spark2D.RenderTextureUtil.SingleChannelRT32(width, height);
        this.#texture.enableRandomWrite = true;
        this.#texture.Create();
        this.#shader = csDepot.Get("fbm")
        this.#kernel = this.#shader.FindKernel("CSMain");
    }

    offset(x: number, y: number) {
        this.#offsetX = x;
        this.#offsetY = y;
        return this;
    }

    scale(value: number) {
        this.#scale = value;
        return this;
    }

    octaves(value: number) {
        this.#octaves = Mathf.Clamp(Math.floor(value), 1, 10);
        return this;
    }

    lacunarity(value: number) {
        this.#lacunarity = Mathf.Max(value, 1);
        return this;
    }

    gain(value: number) {
        this.#gain = Mathf.Clamp01(value);
        return this;
    }

    dispatch() {
        this.#shader.SetTexture(this.#kernel, RESULT, this.#texture);
        this.#shader.SetFloat(SCALE, this.#scale);
        this.#shader.SetFloats(OFFSET, this.#offsetX, this.#offsetY);
        this.#shader.SetInt(OCTAVES, this.#octaves);
        this.#shader.SetFloat(LACUNARITY, this.#lacunarity);
        this.#shader.SetFloat(GAIN, this.#gain);

        Graphics.SetRenderTarget(this.#texture);
        this.#shader.Dispatch(this.#kernel, this.#threadGroupsX, this.#threadGroupsY, 1);
        return this;
    }
}