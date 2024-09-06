﻿/**
 * Inigo Quilez's 2D Distance Functions, converted to HLSL
 * https://iquilezles.org/articles/distfunctions2d/
 */
#ifndef SDFUNCTIONS
#define SDFUNCTIONS

#define ShapeType_Circle 0
#define ShapeType_RoundedBox 1
#define ShapeType_Box 2
#define ShapeType_OrientedBox 3
#define ShapeType_Segment 4
#define ShapeType_Rhombus 5
#define ShapeType_Trapezoid 6
#define ShapeType_Parallelogram 7
#define ShapeType_EquilateralTriangle 8
#define ShapeType_TriangleIsosceles 9
#define ShapeType_Triangle 10
#define ShapeType_UnevenCapsule 11
#define ShapeType_Pentagon 12
#define ShapeType_Hexagon 13
#define ShapeType_Octogon 14
#define ShapeType_Hexagram 15
#define ShapeType_Star5 16
#define ShapeType_Star 17
#define ShapeType_Pie 18
#define ShapeType_CutDisk 19
#define ShapeType_Arc 20
#define ShapeType_Ring 21
#define ShapeType_Horseshoe 22
#define ShapeType_Vesica 23
#define ShapeType_OrientedVesica 24
#define ShapeType_Moon 25
#define ShapeType_RoundedCross 26
#define ShapeType_Egg 27
#define ShapeType_Heart 28
#define ShapeType_Cross 29
#define ShapeType_RoundedX 30
#define ShapeType_Polygon 31
#define ShapeType_Ellipse 32
#define ShapeType_Parabola 33
#define ShapeType_ParabolaSegment 34
#define ShapeType_Bezier 35
#define ShapeType_BlobbyCross 36
#define ShapeType_Tunnel 37
#define ShapeType_Stairs 38
#define ShapeType_QuadraticCircle 39
#define ShapeType_Hyberbola 40
#define ShapeType_fCoolS 41
#define ShapeType_CircleWave 42

#define SHAPE_TYPE int

float ndot(float2 a, float2 b) { return a.x * b.x - a.y * b.y; }
float dot2(in float2 v) { return dot(v, v); }

float sdCircle(float2 p, float r) {
    return length(p) - r;
}

float sdRoundedBox(in float2 p, in float2 b, in float4 r) {
    r.xy = (p.x > 0.0) ? r.xy : r.zw;
    r.x = (p.y > 0.0) ? r.x : r.y;
    float2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r.x;
}

float sdBox(in float2 p, in float2 b) {
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

float sdOrientedBox(in float2 p, in float2 a, in float2 b, float th) {
    float l = length(b - a);
    float2 d = (b - a) / l;
    float2 q = (p - (a + b) * 0.5);
    q = mul(q, float2x2(d.x, -d.y, d.y, d.x));
    q = abs(q) - float2(l, th) * 0.5;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
}

float sdSegment(in float2 p, in float2 a, in float2 b) {
    float2 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

float sdRhombus(in float2 p, in float2 b) {
    p = abs(p);
    float h = clamp(ndot(b - 2.0 * p, b) / dot(b, b), -1.0, 1.0);
    float d = length(p - 0.5 * b * float2(1.0 - h, 1.0 + h));
    return d * sign(p.x * b.y + p.y * b.x - b.x * b.y);
}

float sdTrapezoid(in float2 p, in float r1, float r2, float he) {
    float2 k1 = float2(r2, he);
    float2 k2 = float2(r2 - r1, 2.0 * he);
    p.x = abs(p.x);
    float2 ca = float2(p.x - min(p.x, (p.y < 0.0) ? r1 : r2), abs(p.y) - he);
    float2 cb = p - k1 + k2 * clamp(dot(k1 - p, k2) / dot2(k2), 0.0, 1.0);
    float s = (cb.x < 0.0 && ca.y < 0.0) ? -1.0 : 1.0;
    return s * sqrt(min(dot2(ca), dot2(cb)));
}

float sdParallelogram(float2 p, float wi, float he, float sk) {
    float2 e = float2(sk, he);
    p = (p.y < 0.0) ? -p : p;
    float2 w = p - e;
    w.x -= clamp(w.x, -wi, wi);
    float2 d = float2(dot(w, w), -w.y);
    float s = p.x * e.y - p.y * e.x;
    p = (s < 0.0) ? -p : p;
    float2 v = p - float2(wi, 0);
    v -= e * clamp(dot(v, e) / dot(e, e), -1.0, 1.0);
    d = min(d, float2(dot(v, v), wi * he - abs(s)));
    return sqrt(d.x) * sign(-d.y);
}

float sdEquilateralTriangle(in float2 p, in float r) {
    const float k = sqrt(3.0);
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = float2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}

float sdTriangleIsosceles(float2 p, float2 q) {
    p.x = abs(p.x);
    float2 a = p - q * clamp(dot(p, q) / dot(q, q), 0.0, 1.0);
    float2 b = p - q * float2(clamp(p.x / q.x, 0.0, 1.0), 1.0);
    float s = -sign(q.y);
    float2 d = min(float2(dot(a, a), s * (p.x * q.y - p.y * q.x)), float2(dot(b, b), s * (p.y - q.y)));
    return -sqrt(d.x) * sign(d.y);
}

float sdTriangle(float2 p, float2 p0, float2 p1, float2 p2) {
    float2 e0 = p1 - p0;
    float2 e1 = p2 - p1;
    float2 e2 = p0 - p2;
    float2 v0 = p - p0;
    float2 v1 = p - p1;
    float2 v2 = p - p2;

    float2 pq0 = v0 - e0 * clamp(dot(v0, e0) / dot(e0, e0), 0.0, 1.0);
    float2 pq1 = v1 - e1 * clamp(dot(v1, e1) / dot(e1, e1), 0.0, 1.0);
    float2 pq2 = v2 - e2 * clamp(dot(v2, e2) / dot(e2, e2), 0.0, 1.0);

    float s = sign(e0.x * e2.y - e0.y * e2.x);
    float2 d = min(min(float2(dot(pq0, pq0), s * (v0.x * e0.y - v0.y * e0.x)),
                       float2(dot(pq1, pq1), s * (v1.x * e1.y - v1.y * e1.x))),
                   float2(dot(pq2, pq2), s * (v2.x * e2.y - v2.y * e2.x)));

    return -sqrt(d.x) * sign(d.y);
}

float sdUnevenCapsule(float2 p, float r1, float r2, float h) {
    p.x = abs(p.x);
    float b = (r1 - r2) / h;
    float a = sqrt(1.0 - b * b);
    float k = dot(p, float2(-b, a));

    if (k < 0.0) {
        return length(p) - r1;
    }
    if (k > a * h) {
        return length(p - float2(0.0, h)) - r2;
    }
    return dot(p, float2(a, b)) - r1;
}

float sdPentagon(in float2 p, in float r) {
    const float3 k = float3(0.809016994, 0.587785252, 0.726542528);
    p.x = abs(p.x);
    p -= 2.0 * min(dot(float2(-k.x, k.y), p), 0.0) * float2(-k.x, k.y);
    p -= 2.0 * min(dot(float2(k.x, k.y), p), 0.0) * float2(k.x, k.y);
    p -= float2(clamp(p.x, -r * k.z, r * k.z), r);
    return length(p) * sign(p.y);
}

float sdHexagon(in float2 p, in float r) {
    const float3 k = float3(-0.866025404, 0.5, 0.577350269);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
    return length(p) * sign(p.y);
}

float sdOctogon(in float2 p, in float r) {
    const float3 k = float3(-0.9238795325, 0.3826834323, 0.4142135623);
    p = abs(p);
    p -= 2.0 * min(dot(float2(k.x, k.y), p), 0.0) * float2(k.x, k.y);
    p -= 2.0 * min(dot(float2(-k.x, k.y), p), 0.0) * float2(-k.x, k.y);
    p -= float2(clamp(p.x, -k.z * r, k.z * r), r);
    return length(p) * sign(p.y);
}

float sdHexagram(float2 p, float r) {
    const float4 k = float4(-0.5, 0.8660254038, 0.5773502692, 1.7320508076);
    p = abs(p);
    p -= 2.0 * min(dot(k.xy, p), 0.0) * k.xy;
    p -= 2.0 * min(dot(k.yx, p), 0.0) * k.yx;
    p -= float2(clamp(p.x, r * k.z, r * k.w), r);
    return length(p) * sign(p.y);
}

float sdStar5(float2 p, float r, float rf) {
    const float2 k1 = float2(0.809016994375, -0.587785252292);
    const float2 k2 = float2(-k1.x, k1.y);

    p.x = abs(p.x);
    p -= 2.0 * max(dot(k1, p), 0.0) * k1;
    p -= 2.0 * max(dot(k2, p), 0.0) * k2;
    p.x = abs(p.x);
    p.y -= r;

    float2 ba = rf * float2(-k1.y, k1.x) - float2(0, 1);
    float h = clamp(dot(p, ba) / dot(ba, ba), 0.0, r);

    return length(p - ba * h) * sign(p.y * ba.x - p.x * ba.y);
}

float sdStar(in float2 p, in float r, in int n, in float m) {
    float an = 3.141593 / float(n);
    float en = 3.141593 / m;
    float2 acs = float2(cos(an), sin(an));
    float2 ecs = float2(cos(en), sin(en));
    float bn = fmod(atan2(p.x, p.y), 2.0 * an) - an;
    p = length(p) * float2(cos(bn), abs(sin(bn)));
    p -= r * acs;
    p += ecs * clamp(-dot(p, ecs), 0.0, r * acs.y / ecs.y);
    return length(p) * sign(p.x);
}

float sdPie(float2 p, float2 c, float r) {
    p.x = abs(p.x);
    float l = length(p) - r;
    float m = length(p - c * clamp(dot(p, c), 0.0, r)); // c = sin/cos of aperture
    return max(l, m * sign(c.y * p.x - c.x * p.y));
}

float sdCutDisk(in float2 p, in float r, in float h) {
    float w = sqrt(r * r - h * h);
    p.x = abs(p.x);
    float s = max((h - r) * p.x * p.x + w * w * (h + r - 2.0 * p.y), h * p.x - w * p.y);
    return (s < 0.0) ? length(p) - r : (p.x < w) ? h - p.y : length(p - float2(w, h));
}

float sdArc(float2 p, float2 sc, float ra, float rb) {
    p.x = abs(p.x);
    return ((sc.y * p.x > sc.x * p.y) ? length(p - sc * ra) : abs(length(p) - ra)) - rb;
}

float sdRing(float2 p, float2 n, float r, float th) {
    p.x = abs(p.x);
    p = mul(p, float2x2(n.x, n.y, -n.y, n.x));
    return max(abs(length(p) - r) - th * 0.5, length(float2(p.x, max(0.0, abs(r - p.y) - th * 0.5))) * sign(p.x));
}

float sdHorseshoe(float2 p, float2 c, float r, float2 w) {
    p.x = abs(p.x);
    float l = length(p);
    p = mul(p, float2x2(-c.x, c.y, c.y, c.x));
    p = float2((p.y > 0.0 || p.x > 0.0) ? p.x : l * sign(-c.x), (p.x > 0.0) ? p.y : l);
    p = float2(p.x, abs(p.y - r)) - w;
    return length(max(p, 0.0)) + min(0.0, max(p.x, p.y));
}

float sdVesica(float2 p, float r, float d) {
    p = abs(p);
    float b = sqrt(r * r - d * d);
    return ((p.y - b) * d > p.x * b) ? length(p - float2(0.0, b)) : length(p - float2(-d, 0.0)) - r;
}

float sdOrientedVesica(float2 p, float2 a, float2 b, float w) {
    float r = 0.5 * length(b - a);
    float d = 0.5 * (r * r - w * w) / w;
    float2 v = (b - a) / r;
    float2 c = (b + a) * 0.5;
    float2 q = 0.5 * abs(mul(float2x2(v.y, v.x, -v.x, v.y), (p - c)));
    float3 h = (r * q.x < d * (q.y - r)) ? float3(0.0, r, 0.0) : float3(-d, 0.0, d + w);
    return length(q - h.xy) - h.z;
}

float sdMoon(float2 p, float d, float ra, float rb) {
    p.y = abs(p.y);
    float a = (ra * ra - rb * rb + d * d) / (2.0 * d);
    float b = sqrt(max(ra * ra - a * a, 0.0));
    if (d * (p.x * b - p.y * a) > d * d * max(b - p.y, 0.0))
        return length(p - float2(a, b));
    return max(length(p) - ra, -(length(p - float2(d, 0)) - rb));
}

float sdRoundedCross(float2 p, float h) {
    float k = 0.5 * (h + 1.0 / h);
    p = abs(p);
    return (p.x < 1.0 && p.y < p.x * (k - h) + h)
               ? k - sqrt(dot(p - float2(1, k), p - float2(1, k)))
               : sqrt(min(dot(p - float2(0, h), p - float2(0, h)), dot(p - float2(1, 0), p - float2(1, 0))));
}

float sdEgg(float2 p, float ra, float rb) {
    const float k = sqrt(3.0);
    p.x = abs(p.x);
    float r = ra - rb;
    return ((p.y < 0.0)
                ? length(float2(p.x, p.y)) - r
                : (k * (p.x + r) < p.y)
                ? length(float2(p.x, p.y - k * r))
                : length(float2(p.x + r, p.y)) - 2.0 * r) - rb;
}

float sdHeart(float2 p) {
    p.x = abs(p.x);
    if (p.y + p.x > 1.0)
        return sqrt(dot(p - float2(0.25, 0.75), p - float2(0.25, 0.75))) - sqrt(2.0) / 4.0;
    return sqrt(min(dot(p - float2(0.00, 1.00), p - float2(0.00, 1.00)),
                    dot(p - 0.5 * max(p.x + p.y, 0.0), p - 0.5 * max(p.x + p.y, 0.0)))) * sign(p.x - p.y);
}

float sdCross(float2 p, float2 b, float r) {
    p = abs(p);
    p = (p.y > p.x) ? p.yx : p.xy;
    float2 q = p - b;
    float k = max(q.y, q.x);
    float2 w = (k > 0.0) ? q : float2(b.y - p.x, -k);
    return sign(k) * length(max(w, 0.0)) + r;
}

float sdRoundedX(float2 p, float w, float r) {
    p = abs(p);
    return length(p - min(p.x + p.y, w) * 0.5) - r;
}

#define SDF_POLYGON_MAX_VERTICES 20

float sdPolygon(float2 p, float2 v[SDF_POLYGON_MAX_VERTICES], int N) {
    float d = dot(p - v[0], p - v[0]);
    float s = 1.0;
    for (int i = 0, j = N - 1; i < N; j = i, i++) {
        float2 e = v[j] - v[i];
        float2 w = p - v[i];
        float2 b = w - e * clamp(dot(w, e) / dot(e, e), 0.0, 1.0);
        d = min(d, dot(b, b));
        bool3 c = bool3(p.y >= v[i].y, p.y < v[j].y, e.x * w.y > e.y * w.x);
        if (all(c) || all(!c)) s *= -1.0;
    }
    return s * sqrt(d);
}

float sdEllipse(float2 p, float2 ab) {
    p = abs(p);
    if (p.x > p.y) {
        p = p.yx;
        ab = ab.yx;
    }
    float l = ab.y * ab.y - ab.x * ab.x;
    float m = ab.x * p.x / l;
    float m2 = m * m;
    float n = ab.y * p.y / l;
    float n2 = n * n;
    float c = (m2 + n2 - 1.0) / 3.0;
    float c3 = c * c * c;
    float q = c3 + m2 * n2 * 2.0;
    float d = c3 + m2 * n2;
    float g = m + m * n2;
    float co;
    if (d < 0.0) {
        float h = acos(q / c3) / 3.0;
        float s = cos(h);
        float t = sin(h) * sqrt(3.0);
        float rx = sqrt(-c * (s + t + 2.0) + m2);
        float ry = sqrt(-c * (s - t + 2.0) + m2);
        co = (ry + sign(l) * rx + abs(g) / (rx * ry) - m) / 2.0;
    } else {
        float h = 2.0 * m * n * sqrt(d);
        float s = sign(q + h) * pow(abs(q + h), 1.0 / 3.0);
        float u = sign(q - h) * pow(abs(q - h), 1.0 / 3.0);
        float rx = -s - u - c * 4.0 + 2.0 * m2;
        float ry = (s - u) * sqrt(3.0);
        float rm = sqrt(rx * rx + ry * ry);
        co = (ry / sqrt(rm - rx) + 2.0 * g / rm - m) / 2.0;
    }
    float2 r = ab * float2(co, sqrt(1.0 - co * co));
    return length(r - p) * sign(p.y - r.y);
}

float sdParabola(float2 pos, float k) {
    pos.x = abs(pos.x);
    float ik = 1.0 / k;
    float p = ik * (pos.y - 0.5 * ik) / 3.0;
    float q = 0.25 * ik * ik * pos.x;
    float h = q * q - p * p * p;
    float r = sqrt(abs(h));
    float x = (h > 0.0)
                  ? pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q)
                  : 2.0 * cos(atan(r / q) / 3.0) * sqrt(p);
    return length(pos - float2(x, k * x * x)) * sign(pos.x - x);
}

float sdParabolaSegment(float2 pos, float wi, float he) {
    pos.x = abs(pos.x);
    float ik = wi * wi / he;
    float p = ik * (he - pos.y - 0.5 * ik) / 3.0;
    float q = pos.x * ik * ik * 0.25;
    float h = q * q - p * p * p;
    float r = sqrt(abs(h));
    float x = (h > 0.0)
                  ? pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q)
                  : 2.0 * cos(atan(r / q) / 3.0) * sqrt(p);
    x = min(x, wi);
    return length(pos - float2(x, he - x * x / ik)) * sign(ik * (pos.y - he) + pos.x * pos.x);
}

float sdBezier(float2 pos, float2 A, float2 B, float2 C) {
    float2 a = B - A;
    float2 b = A - 2.0 * B + C;
    float2 c = a * 2.0;
    float2 d = A - pos;
    float kk = 1.0 / dot(b, b);
    float kx = kk * dot(a, b);
    float ky = kk * (2.0 * dot(a, a) + dot(d, b)) / 3.0;
    float kz = kk * dot(d, a);
    float res = 0.0;
    float p = ky - kx * kx;
    float p3 = p * p * p;
    float q = kx * (2.0 * kx * kx - 3.0 * ky) + kz;
    float h = q * q + 4.0 * p3;
    if (h >= 0.0) {
        h = sqrt(h);
        float2 x = (float2(h, -h) - q) * 0.5;
        float2 uv = sign(x) * pow(abs(x), float2(1.0 / 3.0, 1.0 / 3.0));
        float t = clamp(uv.x + uv.y - kx, 0.0, 1.0);
        res = dot(d + (c + b * t) * t, d + (c + b * t) * t);
    } else {
        float z = sqrt(-p);
        float v = acos(q / (p * z * 2.0)) / 3.0;
        float m = cos(v);
        float n = sin(v) * 1.732050808;
        float3 t = clamp(float3(m + m, -n - m, n - m) * z - kx, 0.0, 1.0);
        res = min(dot(d + (c + b * t.x) * t.x, d + (c + b * t.x) * t.x),
                  dot(d + (c + b * t.y) * t.y, d + (c + b * t.y) * t.y));
    }
    return sqrt(res);
}

float sdBlobbyCross(float2 pos, float he) {
    pos = abs(pos);
    pos = float2(abs(pos.x - pos.y), 1.0 - pos.x - pos.y) / sqrt(2.0);
    float p = (he - pos.y - 0.25 / he) / (6.0 * he);
    float q = pos.x / (he * he * 16.0);
    float h = q * q - p * p * p;
    float x;
    if (h > 0.0) {
        float r = sqrt(h);
        x = pow(q + r, 1.0 / 3.0) - pow(abs(q - r), 1.0 / 3.0) * sign(r - q);
    } else {
        float r = sqrt(p);
        x = 2.0 * r * cos(acos(q / (p * r)) / 3.0);
    }
    x = min(x, sqrt(2.0) / 2.0);
    float2 z = float2(x, he * (1.0 - 2.0 * x * x)) - pos;
    return length(z) * sign(z.y);
}

float sdTunnel(float2 p, float2 wh) {
    p.x = abs(p.x);
    p.y = -p.y;
    float2 q = p - wh;
    float d1 = dot(float2(max(q.x, 0.0), q.y), float2(max(q.x, 0.0), q.y));
    q.x = (p.y > 0.0) ? q.x : length(p) - wh.x;
    float d2 = dot(float2(q.x, max(q.y, 0.0)), float2(q.x, max(q.y, 0.0)));
    float d = sqrt(min(d1, d2));
    return (max(q.x, q.y) < 0.0) ? -d : d;
}

float sdStairs(float2 p, float2 wh, float n) {
    float2 ba = wh * n;
    float d = min(dot(p - float2(clamp(p.x, 0.0, ba.x), 0.0), p - float2(clamp(p.x, 0.0, ba.x), 0.0)),
                  dot(p - float2(ba.x, clamp(p.y, 0.0, ba.y)), p - float2(ba.x, clamp(p.y, 0.0, ba.y))));
    float s = sign(max(-p.y, p.x - ba.x));
    float dia = length(wh);
    p = mul(float2x2(wh.x, -wh.y, wh.y, wh.x), p) / dia;
    float id = clamp(round(p.x / dia), 0.0, n - 1.0);
    p.x = p.x - id * dia;
    p = mul(float2x2(wh.x, wh.y, -wh.y, wh.x), p) / dia;
    float hh = wh.y / 2.0;
    p.y -= hh;
    if (p.y > hh * sign(p.x)) s = 1.0;
    p = (id < 0.5 || p.x > 0.0) ? p : -p;
    d = min(d, dot(p - float2(0.0, clamp(p.y, -hh, hh)), p - float2(0.0, clamp(p.y, -hh, hh))));
    d = min(d, dot(p - float2(clamp(p.x, 0.0, wh.x), hh), p - float2(clamp(p.x, 0.0, wh.x), hh)));
    return sqrt(d) * s;
}

float sdQuadraticCircle(float2 p) {
    p = abs(p);
    if (p.y > p.x) p = p.yx;
    float a = p.x - p.y;
    float b = p.x + p.y;
    float c = (2.0 * b - 1.0) / 3.0;
    float h = a * a + c * c * c;
    float t;
    if (h >= 0.0) {
        h = sqrt(h);
        t = sign(h - a) * pow(abs(h - a), 1.0 / 3.0) - pow(h + a, 1.0 / 3.0);
    } else {
        float z = sqrt(-c);
        float v = acos(a / (c * z)) / 3.0;
        t = -z * (cos(v) + sin(v) * 1.732050808);
    }
    t *= 0.5;
    float2 w = float2(-t, t) + 0.75 - t * t - p;
    return length(w) * sign(a * a * 0.5 + b - 1.5);
}

float sdHyberbola(float2 p, float k, float he) {
    p = abs(p);
    p = float2(p.x - p.y, p.x + p.y) / sqrt(2.0);
    float x2 = p.x * p.x / 16.0;
    float y2 = p.y * p.y / 16.0;
    float r = k * (4.0 * k - p.x * p.y) / 12.0;
    float q = (x2 - y2) * k * k;
    float h = q * q + r * r * r;
    float u;
    if (h < 0.0) {
        float m = sqrt(-r);
        u = m * cos(acos(q / (r * m)) / 3.0);
    } else {
        float m = pow(sqrt(h) - q, 1.0 / 3.0);
        u = (m - r / m) / 2.0;
    }
    float w = sqrt(u + x2);
    float b = k * p.y - x2 * p.x * 2.0;
    float t = p.x / 4.0 - w + sqrt(2.0 * x2 - u + b / w / 4.0);
    t = max(t, sqrt(he * he * 0.5 + k) - he / sqrt(2.0));
    return length(p - float2(t, k / t)) * sign(p.x * p.y < k ? 1.0 : -1.0);
}

float sdfCoolS(float2 p) {
    float six = (p.y < 0.0) ? -p.x : p.x;
    p.x = abs(p.x);
    p.y = abs(p.y) - 0.2;
    float rex = p.x - min(round(p.x / 0.4), 0.4);
    float aby = abs(p.y - 0.2) - 0.6;
    float d = dot(p - float2(clamp(0.5 * (six - p.y), 0.0, 0.2), p.y),
                  p - float2(clamp(0.5 * (six - p.y), 0.0, 0.2), p.y));
    d = min(d, dot(p - float2(rex, clamp(p.y, 0.0, 0.4)), p - float2(rex, clamp(p.y, 0.0, 0.4))));
    d = min(d, dot(p - float2(p.x, -aby), p - float2(p.x, -aby)));
    float s = 2.0 * p.x + aby + abs(aby + 0.4) - 0.4;
    return sqrt(d) * sign(s);
}

float sdCircleWave(float2 p, float tb, float ra) {
    tb = 3.1415927 * 5.0 / 6.0 * max(tb, 0.0001);
    float2 co = ra * float2(sin(tb), cos(tb));
    p.x = abs(fmod(p.x, co.x * 4.0) - co.x * 2.0);
    float2 p1 = p;
    float2 p2 = float2(abs(p.x - 2.0 * co.x), -p.y + 2.0 * co.y);
    float d1 = ((co.y * p1.x > co.x * p1.y) ? length(p1 - co) : abs(length(p1) - ra));
    float d2 = ((co.y * p2.x > co.x * p2.y) ? length(p2 - co) : abs(length(p2) - ra));
    return min(d1, d2);
}

#endif