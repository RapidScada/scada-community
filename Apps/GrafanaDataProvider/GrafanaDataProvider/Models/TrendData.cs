using System.Collections.Generic;

namespace GrafanaDataProvider.Models
{
    public class TrendData
    {
        public string target { get; set; }
        public List<double?[]> datapoints { get; set; }
    }
}