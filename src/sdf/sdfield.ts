import { ComputeShader, Graphics, Mathf, RenderTexture, RenderTextureFormat, Shader, Texture2D, TextureFormat } from "UnityEngine";

enum ShapeType {
    Circle,
    RoundedBox,
    Box,
    OrientedBox,
    Segment,
    Rhombus,
    Trapezoid,
    Parallelogram,
    EquilateralTriangle,
    TriangleIsosceles,
    Triangle,
    UnevenCapsule,
    Pentagon,
    Hexagon,
    Octogon,
    Hexagram,
    Star5,
    Star,
    Pie,
    CutDisk,
    Arc,
    Ring,
    Horseshoe,
    Vesica,
    OrientedVesica,
    Moon,
    RoundedCross,
    Egg,
    Heart,
    Cross,
    RoundedX,
    Polygon,
    Ellipse,
    Parabola,
    ParabolaSegment,
    Bezier,
    BlobbyCross,
    Tunnel,
    Stairs,
    QuadraticCircle,
    Hyberbola,
    fCoolS,
    CircleWave
}

const RESULT: number = Shader.PropertyToID("result");
const SHAPE_TYPE: number = Shader.PropertyToID("shapeType");
const POS: number = Shader.PropertyToID("pos");
const ROT: number = Shader.PropertyToID("rot");
const SCALE: number = Shader.PropertyToID("scale");
const ONION: number = Shader.PropertyToID("onion");
const ROUNDED: number = Shader.PropertyToID("rounded");
const F1: number = Shader.PropertyToID("f1");
const F2: number = Shader.PropertyToID("f2");
const F3: number = Shader.PropertyToID("f3");
const F4: number = Shader.PropertyToID("f4");
const F5: number = Shader.PropertyToID("f5");
const F6: number = Shader.PropertyToID("f6");

const VERTS: number = Shader.PropertyToID("verts");

// Object.setPrototypeOf(ComputeShader, ComputeShader.prototype);

/**
 * Returns a class that allows you to draw SDF shapes to a single-channel 
 * texture field (RenderTextureFormat.RFloat)
 */
export function sdfield(width?: number, height?: number) {
    width = width ?? 512;
    height = height ?? width;
    return new SDField(width, height);
}

/**
 * A class that allows you to draw SDF shapes to a single-channel texture field
 */
export class SDField {
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

    #x: number = 0
    #y: number = 0
    #rot: number = 0
    #scaleX: number = 1
    #scaleY: number = 1
    #onion: boolean = false
    #rounded: number = 0

    constructor(width: number, height: number) {
        this.#threadGroupsX = Mathf.CeilToInt(width / 8);
        this.#threadGroupsY = Mathf.CeilToInt(height / 8);
        this.#texture = CS.Spark2D.RenderTextureUtil.SingleChannelRT32(width, height);
        this.#texture.enableRandomWrite = true;
        this.#texture.Create();
        this.#shader = csDepot.Get("sdfield")
        this.#kernel = this.#shader.FindKernel("CSMain");
    }

    pos(x: number, y: number) {
        this.#x = x;
        this.#y = y;
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

    onion(v: boolean = true) {
        this.#onion = v;
        return this;
    }

    rounded(v: number) {
        this.#rounded = v;
        return this;
    }

    #dispatch() {
        this.#shader.SetTexture(this.#kernel, RESULT, this.#texture);
        this.#shader.SetFloats(POS, this.#x, this.#y);
        this.#shader.SetFloat(ROT, this.#rot);
        this.#shader.SetFloats(SCALE, this.#scaleX, this.#scaleY);
        this.#shader.SetBool(ONION, this.#onion);
        this.#shader.SetFloat(ROUNDED, this.#rounded);

        Graphics.SetRenderTarget(this.#texture);
        this.#shader.Dispatch(this.#kernel, this.#threadGroupsX, this.#threadGroupsY, 1);
    }

    circle(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Circle);
        this.#shader.SetFloat(F1, radius);

        this.#dispatch();
        return this;
    }

    roundedBox(width: number, height: number, c1: number, c2: number, c3: number, c4: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.RoundedBox);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, c1);
        this.#shader.SetFloat(F4, c2);
        this.#shader.SetFloat(F5, c3);
        this.#shader.SetFloat(F6, c4);

        this.#dispatch();
        return this;
    }

    box(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Box);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    orientedBox(width: number, height: number, th: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.OrientedBox);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, th);
        this.#dispatch();
        return this;
    }

    segment(x1: number, y1: number, x2: number, y2: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Segment);
        this.#shader.SetFloat(F1, x1);
        this.#shader.SetFloat(F2, y1);
        this.#shader.SetFloat(F3, x2);
        this.#shader.SetFloat(F4, y2);
        this.#dispatch();
        return this;
    }

    rhombus(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Rhombus);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    trapezoid(r1: number, r2: number, he: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Trapezoid);
        this.#shader.SetFloat(F1, r1);
        this.#shader.SetFloat(F2, r2);
        this.#shader.SetFloat(F3, he);
        this.#dispatch();
        return this;
    }

    parallelogram(width: number, height: number, skew: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Parallelogram);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, skew);
        this.#dispatch();
        return this;
    }

    equilateralTriangle(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.EquilateralTriangle);
        this.#shader.SetFloat(F1, radius);
        this.#dispatch();
        return this;
    }

    triangleIsosceles(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.TriangleIsosceles);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    triangle(x1: number, y1: number, x2: number, y2: number, x3: number, y3: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Triangle);
        this.#shader.SetFloat(F1, x1);
        this.#shader.SetFloat(F2, y1);
        this.#shader.SetFloat(F3, x2);
        this.#shader.SetFloat(F4, y2);
        this.#shader.SetFloat(F5, x3);
        this.#shader.SetFloat(F6, y3);
        this.#dispatch();
        return this;
    }

    unevenCapsule(r1: number, r2: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.UnevenCapsule);
        this.#shader.SetFloat(F1, r1);
        this.#shader.SetFloat(F2, r2);
        this.#shader.SetFloat(F3, height);
        this.#dispatch();
        return this;
    }

    pentagon(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Pentagon);
        this.#shader.SetFloat(F1, radius);
        this.#dispatch();
        return this;
    }

    hexagon(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Hexagon);
        this.#shader.SetFloat(F1, radius);
        this.#dispatch();
        return this;
    }

    octogon(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Octogon);
        this.#shader.SetFloat(F1, radius);
        this.#dispatch();
        return this;
    }

    hexagram(radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Hexagram);
        this.#shader.SetFloat(F1, radius);
        this.#dispatch();
        return this;
    }

    star5(outerRadius: number, innerRadius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Star5);
        this.#shader.SetFloat(F1, outerRadius);
        this.#shader.SetFloat(F2, innerRadius);
        this.#dispatch();
        return this;
    }

    star(radius: number, n: number, m: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Star);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, n);
        this.#shader.SetFloat(F3, m);
        this.#dispatch();
        return this;
    }

    pie(radius: number, angle: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Pie);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, angle);
        this.#dispatch();
        return this;
    }

    cutDisk(radius: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.CutDisk);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    arc(radius: number, startAngle: number, endAngle: number, thickness: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Arc);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, startAngle);
        this.#shader.SetFloat(F3, endAngle);
        this.#shader.SetFloat(F4, thickness);
        this.#dispatch();
        return this;
    }

    ring(radius: number, thickness: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Ring);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, thickness);
        this.#dispatch();
        return this;
    }

    horseshoe(radius: number, angle: number, thickness: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Horseshoe);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, angle);
        this.#shader.SetFloat(F3, thickness);
        this.#dispatch();
        return this;
    }

    vesica(radius: number, distance: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Vesica);
        this.#shader.SetFloat(F1, radius);
        this.#shader.SetFloat(F2, distance);
        this.#dispatch();
        return this;
    }

    orientedVesica(width: number, height: number, radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.OrientedVesica);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, radius);
        this.#dispatch();
        return this;
    }

    moon(distance: number, ra: number, rb: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Moon);
        this.#shader.SetFloat(F1, distance);
        this.#shader.SetFloat(F2, ra);
        this.#shader.SetFloat(F3, rb);
        this.#dispatch();
        return this;
    }

    roundedCross(size: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.RoundedCross);
        this.#shader.SetFloat(F1, size);
        this.#dispatch();
        return this;
    }

    egg(ra: number, rb: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Egg);
        this.#shader.SetFloat(F1, ra);
        this.#shader.SetFloat(F2, rb);
        this.#dispatch();
        return this;
    }

    heart() {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Heart);
        this.#dispatch();
        return this;
    }

    cross(width: number, height: number, thickness: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Cross);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, thickness);
        this.#dispatch();
        return this;
    }

    roundedX(width: number, radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.RoundedX);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, radius);
        this.#dispatch();
        return this;
    }

    polygon(vertices: Array<[number, number]>) {
        const verts = vertices.flat();
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Polygon);
        this.#shader.SetFloats.call(null, [VERTS, ...verts]); // Need to test this
        this.#shader.SetInt(F1, vertices.length);
        this.#dispatch();
        return this;
    }

    ellipse(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Ellipse);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    parabola(k: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Parabola);
        this.#shader.SetFloat(F1, k);
        this.#dispatch();
        return this;
    }

    parabolaSegment(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.ParabolaSegment);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    bezier(x1: number, y1: number, x2: number, y2: number, x3: number, y3: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Bezier);
        this.#shader.SetFloat(F1, x1);
        this.#shader.SetFloat(F2, y1);
        this.#shader.SetFloat(F3, x2);
        this.#shader.SetFloat(F4, y2);
        this.#shader.SetFloat(F5, x3);
        this.#shader.SetFloat(F6, y3);
        this.#dispatch();
        return this;
    }

    blobbyCross(height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.BlobbyCross);
        this.#shader.SetFloat(F1, height);
        this.#dispatch();
        return this;
    }

    tunnel(width: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Tunnel);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    stairs(width: number, height: number, steps: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Stairs);
        this.#shader.SetFloat(F1, width);
        this.#shader.SetFloat(F2, height);
        this.#shader.SetFloat(F3, steps);
        this.#dispatch();
        return this;
    }

    quadraticCircle() {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.QuadraticCircle);
        this.#dispatch();
        return this;
    }

    hyperbola(k: number, height: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.Hyberbola);
        this.#shader.SetFloat(F1, k);
        this.#shader.SetFloat(F2, height);
        this.#dispatch();
        return this;
    }

    coolS() {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.fCoolS);
        this.#dispatch();
        return this;
    }

    circleWave(thickness: number, radius: number) {
        this.#shader.SetInt(SHAPE_TYPE, ShapeType.CircleWave);
        this.#shader.SetFloat(F1, thickness);
        this.#shader.SetFloat(F2, radius);
        this.#dispatch();
        return this;
    }
}