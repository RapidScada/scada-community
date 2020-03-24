namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Represents parameters passed by Grafana.
    /// </summary>
    public class GrafanaArg
    {
        public string app { get; set; }
        public int intervalMs { get; set; }
        public Range range { get; set; }        
        public Targets[] targets { get; set; }
        public long startTime { get; set; }
        public long endTime { get; set; }
    }
}