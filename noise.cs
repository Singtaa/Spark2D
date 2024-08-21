namespace Spark2D {
    public static class noise {
        public static float fbm(float x, float y, int octaves = 5, float lacunarity = 2f, float gain = 0.5f) {
            float sum = 0f;
            float frequency = 1f;
            float amplitude = 1f;
            float maxAmplitude = 0f;

            for (int i = 0; i < octaves; i++) {
                sum += amplitude * SimplexNoise.Generate(x * frequency, y * frequency);
                maxAmplitude += amplitude;
                frequency *= lacunarity;
                amplitude *= gain;
            }

            return sum / maxAmplitude;
        }

        public static float simplex(float x, float y) {
            return SimplexNoise.Generate(x, y);
        }
    }
}