using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace Spark2D.Tests {
    [TestFixture]
    public class SimplexNoise_Tests {
        [Test]
        public void Generate_OutputIsWellDistributedWithinRange() {
            const int sampleSize = 10000;
            const float minValue = -1f;
            const float maxValue = 1f;
            const int numBins = 20;
            const float minExpectedFillPercentage = 0.7f;

            var bins = new int[numBins];
            float binSize = (maxValue - minValue) / numBins;

            var currentMin = float.MaxValue;
            var currentMax = float.MinValue;
            for (int i = 0; i < sampleSize; i++) {
                float x = UnityEngine.Random.Range(-100f, 100f);
                float y = UnityEngine.Random.Range(-100f, 100f);

                float noiseValue = SimplexNoise.Generate(x, y);
                
                currentMin = Mathf.Min(currentMin, noiseValue);
                currentMax = Mathf.Max(currentMax, noiseValue);

                Assert.GreaterOrEqual(noiseValue, minValue, $"Noise value {noiseValue} is less than the minimum expected value {minValue}");
                Assert.LessOrEqual(noiseValue, maxValue, $"Noise value {noiseValue} is greater than the maximum expected value {maxValue}");

                int binIndex = Mathf.FloorToInt((noiseValue - minValue) / binSize);
                if (binIndex == numBins) binIndex--; // Handle edge case for maxValue
                bins[binIndex]++;
            }

            int filledBins = bins.Count(b => b > 0);
            float fillPercentage = (float)filledBins / numBins;

            // Debug.Log($"Bin counts: {string.Join(", ", bins)}");
            // Debug.Log($"Filled bins: {filledBins} out of {numBins}");
            // Debug.Log($"Fill percentage: {fillPercentage:P2}");
            // Debug.Log($"Min value: {currentMin}, Max value: {currentMax}");

            Assert.GreaterOrEqual(fillPercentage, minExpectedFillPercentage,
                $"Only {fillPercentage:P2} of the range is filled. Expected at least {minExpectedFillPercentage:P2}");

            float minBinCount = sampleSize * 0.4f / numBins;
            float maxBinCount = sampleSize * 1.5f / numBins;

            for (int i = 1; i < numBins - 1; i++) { // Skip first and last bins
                Assert.GreaterOrEqual(bins[i], minBinCount,
                    $"Bin {i} has too few samples ({bins[i]}). Expected at least {minBinCount}.");
                Assert.LessOrEqual(bins[i], maxBinCount,
                    $"Bin {i} has too many samples ({bins[i]}). Expected at most {maxBinCount}.");
            }
        }
    }
}