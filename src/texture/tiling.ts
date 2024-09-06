import { ComputeShader, Graphics, Mathf, RenderTexture, Shader } from "UnityEngine";

const RESULT: number = Shader.PropertyToID("result");
const TEX1: number = Shader.PropertyToID("tex1");
const OFFSET: number = Shader.PropertyToID("offset");
const ROTATION: number = Shader.PropertyToID("rotation");
const SCALE: number = Shader.PropertyToID("scale");

interface RTProvider {
    rt: RenderTexture;
}

/**
 * Returns a wrapper that allows you to do transformations on the passed in RenderTexture
 */
export function tiling(tex: RenderTexture | RTProvider) {
    return new Tiling("rt" in tex ? tex.rt : tex);
}

export class Tiling {
    /**
     * The underlying RenderTexture
     */
    get rt() {
        return this.#result;
    }

    #threadGroupsX: number;
    #threadGroupsY: number;
    #shader: ComputeShader;
    #kernel: number;
    #tex1: RenderTexture;
    #result: RenderTexture;

    #u: number = 0
    #v: number = 0
    #rot: number = 0
    #scaleX: number = 1
    #scaleY: number = 1

    constructor(rt: RenderTexture) {
        const { width, height } = rt;
        this.#tex1 = rt;
        this.#threadGroupsX = Mathf.CeilToInt(width / 8);
        this.#threadGroupsY = Mathf.CeilToInt(height / 8);
        this.#result = CS.Spark2D.RenderTextureUtil.SingleChannelRT32(width, height);
        this.#result.enableRandomWrite = true;
        this.#result.Create();
        this.#shader = csDepot.Get("tiling");
        this.#kernel = this.#shader.FindKernel("CSMain");
    }

    /**
     * In UV space
     */
    offset(u: number, v: number) {
        this.#u = u;
        this.#v = v;
        return this;
    }

    rot(rot: number) { // in degrees
        this.#rot = rot * Mathf.Deg2Rad;
        return this;
    }

    rotr(rot: number) { // in radians
        this.#rot = rot;
        return this;
    }

    scale(x: number, y?: number) {
        this.#scaleX = x;
        this.#scaleY = y ?? x;
        return this;
    }

    dispatch() {
        this.#shader.SetTexture(this.#kernel, TEX1, this.#tex1);
        this.#shader.SetTexture(this.#kernel, RESULT, this.#result);
        this.#shader.SetFloats(OFFSET, this.#u, this.#v);
        this.#shader.SetFloat(ROTATION, this.#rot);
        this.#shader.SetFloats(SCALE, this.#scaleX, this.#scaleY);

        Graphics.SetRenderTarget(this.#result);
        this.#shader.Dispatch(this.#kernel, this.#threadGroupsX, this.#threadGroupsY, 1);
        return this;
    }
}