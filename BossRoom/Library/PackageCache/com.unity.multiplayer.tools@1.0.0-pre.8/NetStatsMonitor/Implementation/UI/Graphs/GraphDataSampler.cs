// RNSM Implementation compilation boilerplate
// All references to UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED should be defined in the same way,
// as any discrepancies are likely to result in build failures
// ---------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR || ((DEVELOPMENT_BUILD && !UNITY_MP_TOOLS_NET_STATS_MONITOR_DISABLED_IN_DEVELOP) || (!DEVELOPMENT_BUILD && UNITY_MP_TOOLS_NET_STATS_MONITOR_ENABLED_IN_RELEASE))
    #define UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED
#endif
// ---------------------------------------------------------------------------------------------------------------------

#if UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED
using System;
using Unity.Multiplayer.Tools.Common;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    /// Computes the value of points on the graph, which may straddle
    /// an arbitrary number of samples in the history of a stat
    static class GraphDataSampler
    {
        /// <summary>
        /// Computes the value of a point that straddles an arbitrary number of
        /// samples. Advances to the next point by updating the sampleIndex
        /// by reference to the beginning of the next point.
        /// <br/><br/>
        /// This function is called from the inner loops of the
        /// LineGraphRenderer and the StackedAreaGraphRenderer.
        /// <br/><br/>
        /// As this function runs in the inner loop during graphing, some redundant
        /// parameters are included to avoid recomputing or rereading during each
        /// iteration of the loop. The overhead of passing these parameters might be
        /// greater than recomputing, but this function is written with the intent of
        /// being inlined. If (based on profiles) this function isn't being inlined its
        /// contents should be inlined manually.
        /// </summary>
        /// <param name="graphSamplesPerPoint">
        /// The number of samples per point in the graph
        /// </param>
        /// <param name="graphPointsPerSample">
        /// The number of points per sample in the graph.
        /// This is the inverse of graphSamplesPerPoint, and
        /// is included to avoid recomputing in the inner loop.
        /// </param>
        /// <param name="sampleCount">
        /// The actual number of samples available for this stat, which may be
        /// more or less than the number of samples shown on the graph
        /// </param>
        /// <param name="statData">
        /// The actual sample data of the stat
        /// </param>
        /// <param name="sampleIndex">
        /// </param>
        /// <param name="lastReadSample">
        /// Avoid re-reading by remembering the most recently read value
        /// </param>
        /// <param name="fractionOfPreviousSample">
        /// Avoid recomputing by saving this from the previous call
        /// </param>
        /// <remarks>
        /// This is a shared block of code that runs in the inner loop of
        /// the LineGraphRenderer and the StackedAreaGraphRenderer.
        /// Some of the parameters included are redundant, but are included
        /// to avoid recomputing them from the other parameters each time this
        /// is called. Hopefully this function gets inlined 🙏
        /// </remarks>
        /// <returns>
        /// The value of a point straddling an arbitrary number of samples.
        /// </returns>
        public static float SamplePointAndAdvance(
            float graphSamplesPerPoint,
            int sampleCount,
            RingBuffer<float> statData,
            ref float sampleIndex,
            ref float lastReadSample,
            ref float fractionOfPreviousSample)
        {
            var indexBegin = Math.Clamp(sampleIndex, 0f, sampleCount);
            var indexEnd = Math.Clamp(sampleIndex + graphSamplesPerPoint, 0f, sampleCount);
            if (indexEnd <= indexBegin)
            {
                return 0f;
            }

            var sum = 0f;

            // 1. Add the fraction of the first partial sample of this point
            sum = fractionOfPreviousSample * lastReadSample;

            // 2. Add all samples that fall completely within this point
            var wholeIndexBegin = (int)Math.Ceiling(indexBegin);
            var wholeIndexEnd = (int)Math.Floor(indexEnd);
            for (int i = wholeIndexBegin; i < wholeIndexEnd; ++i)
            {
                sum += statData[i];
            }

            // 3. Add the fraction of the last partial sample of this point
            if (indexEnd > wholeIndexEnd)
            {
                var fractionOfLastSample = indexEnd - wholeIndexEnd;
                lastReadSample = statData[wholeIndexEnd];
                sum += fractionOfLastSample * lastReadSample;
                fractionOfPreviousSample = 1f - fractionOfLastSample;
            }
            else
            {
                fractionOfPreviousSample = 0f;
            }

            // Normalize the point value to the average of the samples
            var pointValue = sum / (indexEnd - indexBegin);
            sampleIndex = indexEnd;
            return pointValue;
        }
    }
}
#endif
