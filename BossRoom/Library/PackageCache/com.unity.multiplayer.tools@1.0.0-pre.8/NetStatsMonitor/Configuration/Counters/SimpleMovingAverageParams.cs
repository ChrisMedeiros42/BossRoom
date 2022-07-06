using System;
using UnityEngine;

namespace Unity.Multiplayer.Tools.NetStatsMonitor
{
    /// <summary>
    /// Parameters for the simple moving average smoothing method in <see cref="CounterConfiguration"/>.
    /// </summary>
    [Serializable]
    public sealed class SimpleMovingAverageParams
    {
        /// <summary>
        /// The number of samples that are maintained for the purpose of smoothing.
        /// </summary>
        /// <remarks>
        /// If the value is out of the range [0, 4096], it will be clamped to the
        /// nearest value.
        /// </remarks>
        [field: SerializeField]
        [field: Min(1)]
        [field: Tooltip("The number of samples that are maintained for the purpose of smoothing." +
                        "If the value is out of the range [0, 4096], it will be clamped to the nearest value.")]
        [field: Range(ConfigurationLimits.k_CounterSampleMin, ConfigurationLimits.k_CounterSampleMax)]
        int m_SampleCount = 64;
        
        public int SampleCount
        {
            get => m_SampleCount; 
            set => m_SampleCount = Mathf.Clamp(
                value,
                ConfigurationLimits.k_CounterSampleMin,
                ConfigurationLimits.k_CounterSampleMax); 
        }

        internal int ComputeHashCode()
        {
            return SampleCount;
        }
    }
}