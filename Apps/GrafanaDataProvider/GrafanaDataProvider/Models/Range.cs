using System;

namespace GrafanaDataProvider.Models
{
    /// <summary>
    /// Represents time period for graph.
    /// </summary>
    public class Range
    {
        public /*string*/DateTime from { get; set; }
        public /*string*/DateTime to { get; set; }
    }
}