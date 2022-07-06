// RNSM Implementation compilation boilerplate
// All references to UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED should be defined in the same way,
// as any discrepancies are likely to result in build failures
// ---------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR || ((DEVELOPMENT_BUILD && !UNITY_MP_TOOLS_NET_STATS_MONITOR_DISABLED_IN_DEVELOP) || (!DEVELOPMENT_BUILD && UNITY_MP_TOOLS_NET_STATS_MONITOR_ENABLED_IN_RELEASE))
    #define UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED
#endif
// ---------------------------------------------------------------------------------------------------------------------

#if UNITY_MP_TOOLS_NET_STATS_MONITOR_IMPLEMENTATION_ENABLED

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using Unity.Multiplayer.Tools.Common;
using Unity.Multiplayer.Tools.NetStats;

namespace Unity.Multiplayer.Tools.NetStatsMonitor.Implementation
{
    internal class GraphContent : VisualElement
    {
        GraphParameters m_GraphParams;
        readonly GraphBuffers m_Buffers = new();
        Color[] m_VariableColors;
        IGraphRenderer m_Renderer;

        public GraphContent()
        {
            generateVisualContent += OnGenerateVisualContent;
        }

        public void UpdateConfiguration(DisplayElementConfiguration config)
        {
            m_GraphParams = new GraphParameters
            {
                StatCount = config.Stats.Count,
                SamplesPerStat = config.GraphConfiguration.SampleCount,
            };
            m_VariableColors = config.GraphConfiguration.VariableColors.ToArray();
            switch (config.Type)
            {
                case DisplayElementType.LineGraph:
                    if (m_Renderer is not LineGraphRenderer)
                    {
                        m_Renderer = new LineGraphRenderer();
                    }
                    break;
                case DisplayElementType.StackedAreaGraph:
                    if (m_Renderer is not StackedAreaGraphRenderer)
                    {
                        m_Renderer = new StackedAreaGraphRenderer();
                    }
                    break;
            }
            m_Renderer.UpdateConfiguration(config);
        }

        public MinAndMax UpdateDisplayData(
            MultiStatHistory history,
            List<MetricId> stats,
            float minPlotValue,
            float maxPlotValue)
        {
            if (m_Renderer == null)
            {
                return new();
            }
            var graphContentRect = contentRect;
            var graphWidth = graphContentRect.width;
            if (float.IsNaN(graphWidth))
            {
                return new();
            }

            var newBufferParam = new GraphBufferParameters(
                m_GraphParams,
                graphWidth,
                m_Renderer.MaxPointsPerPixel);

            // We need to do this each time we draw, as even if the configuration hasn't changed,
            // the contentRect may have
            m_Buffers.UpdateIfNeeded(newBufferParam, m_VariableColors);

            var minAndMaxPlotValue = m_Renderer.UpdateVertices(
                history,
                stats,
                minPlotValue,
                maxPlotValue,
                m_GraphParams,
                m_Buffers.Parameters,
                renderBoundsXMin: graphContentRect.xMin,
                renderBoundsXMax: graphContentRect.xMax,

                // Inverting renderBoundsYMin and renderBoundsYMax
                // since we want our graph from bottom to top
                renderBoundsYMin: graphContentRect.yMax,
                renderBoundsYMax: graphContentRect.yMin,

                m_Buffers.Vertices);

            MarkDirtyRepaint();
            return minAndMaxPlotValue;
        }

        void OnGenerateVisualContent(MeshGenerationContext mgc)
        {
            if (mgc == null)
            {
                return;
            }
            m_Buffers.WriteToMeshGenerationContext(mgc);
        }
    }
}
#endif
