using System.Collections.Generic;

namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Parameters transmitted to grafana for build graphic
    /// </summary>
    public class TrendData
    {
        public string target { get; set; }
        public List<double?[]> datapoints { get; set; }
    }
}