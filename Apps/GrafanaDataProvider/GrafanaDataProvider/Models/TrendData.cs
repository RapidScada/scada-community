using System.Collections.Generic;

namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Represents parameters transmitted to Grafana for build graph.
    /// </summary>
    public class TrendData
    {
        public string target { get; set; }
        public List<double?[]> datapoints { get; set; }
    }
}