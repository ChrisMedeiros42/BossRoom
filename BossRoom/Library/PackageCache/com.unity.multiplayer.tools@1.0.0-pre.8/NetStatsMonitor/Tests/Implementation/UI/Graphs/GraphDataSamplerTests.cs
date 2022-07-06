using System;
using NUnit.Framework;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStatsMonitor.Implementation;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Tests.Implementation.Graphs
{
    class GraphDataSamplerTests
    {
        [TestCase(
            1f,
            new float[]{4, 5, 7, 9},
            new float[]{4, 5, 7, 9},
            TestName = "Stride = 1")]
        [TestCase(
            0.5f,
            new float[]{4, 5, 7, 9},
            new float[]{4, 4, 5, 5, 7, 7, 9, 9},
            TestName = "Stride = 0.5")]
        [TestCase(
            2f,
            new float[]{4, 5, 7, 9},
            new float[]{4.5f, 8},
            TestName = "Stride = 2")]
        [TestCase(
            0.75f,
            new float[]{4, 5, 7, 9},
            new float[]{
                4,
                (0.25f * 4 + .50f * 5) / 0.75f,
                (0.50f * 5 + .25f * 7) / 0.75f,
                7,
                9,
                9,
            },
            TestName = "Stride = 0.75f")]
        [TestCase(
            1.5f,
            new float[]{4, 5, 7, 9},
            new float[]{
                (1.0f * 4 + 0.5f * 5) / 1.5f,
                (0.5f * 5 + 1.0f * 7) / 1.5f,
                9,
            },
            TestName = "Stride = 1.5f")]
        public void SamplePointTests(float stride, float[] input, float[] output)
        {
            var inputCount = input.Length;
            var pointCount = (int)(MathF.Ceiling(inputCount / stride));

            var samplesPerPoint = stride;

            var values = new RingBuffer<float>(input);

            var sampleIndex = 0f;
            var lastReadSample = 0f;
            var fractionOfPreviousSample = 0f;
            for (var i = 0; i < pointCount; ++i)
            {
                var sampleValue = GraphDataSampler.SamplePointAndAdvance(
                    graphSamplesPerPoint: samplesPerPoint,
                    sampleCount: inputCount,
                    statData: values,
                    sampleIndex: ref sampleIndex,
                    lastReadSample: ref lastReadSample,
                    fractionOfPreviousSample: ref fractionOfPreviousSample);
                var expected = output[i];
                Assert.AreEqual(expected, sampleValue, 0.0001f);
            }
        }

    }
}