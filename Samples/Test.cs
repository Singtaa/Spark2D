using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Spark2D {
    public class Test : MonoBehaviour {
        public Texture2D texture;
        public Texture2D res;
        public float maxInside;
        public float maxOutside;
        public float postProcessDistance;
        public RGBFillMode rgbMode;

        void Start() {
            Generate();
        }

        [ContextMenu("Generate")]
        void Generate() {
            res = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);
            SDFTextureGenerator.Generate(texture, res, maxInside, maxOutside, postProcessDistance, rgbMode);
        }
    }
}