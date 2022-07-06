using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Graph configuration used by <see cref="DisplayElementConfiguration"/>.
    /// This configuration contain all information about a Graph.
    /// </summary>
    [Serializable]
    public sealed class GraphConfiguration
    {
        /// <summary>
        /// The number of samples that are maintained for the purpose of graphing.
        /// </summary>
        /// <remarks>
        /// If the value is out of the range [0, 4096], it will be clamped to the
        /// nearest value.
        /// </remarks>
        [field: SerializeField]
        [field: Tooltip("The number of samples that are maintained for the purpose of graphing. " +
                        "If the value is out of the range [0, 4096], it will be clamped to the nearest value.")]
        [field: Range(ConfigurationLimits.k_GraphSampleMin, ConfigurationLimits.k_GraphSampleMax)]
        int m_SampleCount = 256;

        public int SampleCount
        {
            get => m_SampleCount;
            set => m_SampleCount = Mathf.Clamp(
                value,
                ConfigurationLimits.k_GraphSampleMin,
                ConfigurationLimits.k_GraphSampleMax);
        }

        /// <summary>
        /// List of colors to override the default colors of the graph.
        /// </summary>
        [field: SerializeField]
        public List<Color> VariableColors { get; set; } = new();

        /// <summary>
        /// The units used for displaying the bounds of the graph's x-axis.
        /// By default the graph bounds are displayed in units of sample count.
        /// If set to time, the the x-axis graph bounds will display
        /// the time over which these samples were collected.
        /// </summary>
        [field: SerializeField]
        public GraphXAxisType XAxisType { get; set; } = GraphXAxisType.Samples;

        /// <summary>
        /// Line-graph specific options.
        /// </summary>
        [field: SerializeField]
        public LineGraphConfiguration LineGraphConfiguration { get; set; } = new();

        internal int ComputeHashCode()
        {
            var hash = HashCode.Combine(SampleCount, (int)XAxisType, LineGraphConfiguration.ComputeHashCode());
            if (VariableColors != null)
            {
                foreach (var color in VariableColors)
                {
                    hash = HashCode.Combine(hash, color);
                }
            }
            return hash;
        }
    }
}