using System;
using System.Collections.Generic;

using NUnit.Framework;

using UnityEngine;
using UnityEngine.UIElements;
using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.MetricTypes;
using Unity.Multiplayer.Tools.NetStats;
using Unity.Multiplayer.Tools.NetStatsMonitor.Implementation;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Tests.Implementation.Graphs
{
    [TestFixture]
    [Ignore(
        "The graphs themselves are working, " +
        "I just need to get to the bottom of what to do about these tests")]
    class LineGraphRendererTests
    {
        // The actual square root function isn't constant,
        // so we need a literal constant to use this in TestCase attributes
        const float k_Sqrt2 = 1.4142135623730950488016887242096980785696718753769480731766797379f;
        const float k_HalfSqrt2 = 0.5f * k_Sqrt2;

        static readonly MetricId k_RpcSent = MetricId.Create(DirectedMetricType.RpcSent);

        static void AssertAreApproximatelyEqual(float[] expected, float[] actual, float epsilon)
        {
            Assert.AreEqual(expected.Length, actual.Length, "The two arrays must have the same length to be equal.");

            var differenceCount = 0;
            var expectedStr = "[";
            var actualStr   = "[";
            for (var i = 0; i < expected.Length; ++i)
            {
                var x = expected[i];
                var y = actual[i];
                if (MathF.Abs(x - y) > epsilon)
                {
                    ++differenceCount;
                }
                var separator = (i + 1) < expected.Length ? ", " : "]\n";
                expectedStr += x.ToString("F") + separator;
                actualStr += y.ToString("F") + separator;
            }
            Assert.AreEqual(0, differenceCount,
                $"FP Arrays differed in {differenceCount} places.\n" +
                $"  Expected:\n" +
                $"      {expectedStr}\n" +
                $"  Actual:\n" +
                $"      {actualStr}\n");
        }

        static Rect MakeGraphRect(float width, float height)
        {
            // Negate the height as we're plotting bottom-to-top whereas the UI is top-to-bottom
            return new Rect(0, 0, width, height);
        }

        static MultiStatHistory MakeSimpleHistory(MetricId metricId, float[] values)
        {
            return MultiStatHistory.CreateMockMultiStatHistoryForTest(
                metricId,
                new StatHistory(new RingBuffer<float>(values)));
        }

        /// A float[] parameter is used for the expectedVertexPositions because an array
        /// of Vector2s can't be constructed as a compile time constant, and so can't be
        /// used as a parameter for the TestCase attribute.
        /// Each pair of consecutive floats represents a 2D point in the output.

        [TestCase(10, 10, 1, new float[] {}, new float[] {}, TestName = "Zero Samples")]

        [TestCase(10, 10, 1, new float[] {5}, new float[] {0, 0, 0, 0}, TestName = "One Sample")]

        [TestCase(10, 10, 1, new float[] {5, 5}, new float[]
        {
            0, 5.5f,
            0, 4.5f,
            10, 5.5f,
            10, 4.5f
        }, TestName = "Straight Line, One Segment")]

        [TestCase(10, 10, 1, new float[] {5, 5, 5}, new float[]
        {
            00, 5.5f,
            00, 4.5f,
            05, 5.5f,
            05, 4.5f,
            10, 5.5f,
            10, 4.5f
        }, TestName = "Straight Line, Two Segments")]

        [TestCase(10, 10, 1, new float[] {5, 5, 5, 5, 5}, new float[]
        {
            00.0f, 5.5f,
            00.0f, 4.5f,
            02.5f, 5.5f,
            02.5f, 4.5f,
            05.0f, 5.5f,
            05.0f, 4.5f,
            07.5f, 5.5f,
            07.5f, 4.5f,
            10.0f, 5.5f,
            10.0f, 4.5f
        }, TestName = "Straight Line, Four Segments")]

        [TestCase(10, 10, 1, new float[] {5, 0, 5}, new float[]
        {
            00.0f, 5f + k_HalfSqrt2,
            00.0f, 5f - k_HalfSqrt2,
            05.0f, 0f + k_HalfSqrt2,
            05.0f, 0f,  // The value here would be -k_HalfSqrt2, but it gets clamped to zero
            10.0f, 5f + k_HalfSqrt2,
            10.0f, 5f - k_HalfSqrt2,
        }, TestName = "V-Shape, Two Segments")]

        public void OneStatGraphTests(
            float graphWidth,
            float graphHeight,
            float lineThickness,

            float[] samples,
            float[] expectedVertexPositions)
        {
            var stats = new List<MetricId>{ k_RpcSent };
            var renderer = new LineGraphRenderer { LineThickness = lineThickness };
            var history = MakeSimpleHistory(k_RpcSent, samples);

            var graphParams = new GraphParameters
            {
                SamplesPerStat = samples.Length,
                StatCount = stats.Count,
            };

            var bufferParams = new GraphBufferParameters
            {
                GraphWidthPoints = samples.Length,
                StatCount = stats.Count,
            };

            var vertexCount = stats.Count * samples.Length * GraphBuffers.k_VerticesPerPoint;

            var vertices = new Vertex[vertexCount];
            renderer.UpdateVertices(
                history,
                stats,
                yAxisMin: 0,
                yAxisMax: graphHeight,
                graphParams,
                bufferParams,
                renderBoundsXMin: 0f,
                renderBoundsXMax: graphWidth,
                renderBoundsYMin: 0f,
                renderBoundsYMax: graphHeight,
                vertices);

            const int k_FloatsPerVertexPosition = 2;
            var vertexPositions = new float[vertexCount * k_FloatsPerVertexPosition];

            for (var i = 0; i < vertexCount; ++i)
            {
                var baseIndex = i * k_FloatsPerVertexPosition;
                vertexPositions[baseIndex + 0] = vertices[i].position.x;
                vertexPositions[baseIndex + 1] = vertices[i].position.y;
            }
            AssertAreApproximatelyEqual(expectedVertexPositions, vertexPositions, 1e-4f);
        }
    }
}