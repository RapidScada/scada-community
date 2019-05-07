using System;
using System.Collections.Generic;

namespace BasicFormulas
{
    /// <summary>
    /// The formulas for working with previous channel data.
    /// </summary>
    public class Prev : FormulaTester.FormulaTester
    {
        /// <summary>
        /// Channel data points accessing by channel numbers.
        /// </summary>
        public Dictionary<int, CnlDataPoint> CnlDataPoints = new Dictionary<int, CnlDataPoint>();

        /// <summary>
        /// Represents channel data at a particular time.
        /// </summary>
        public struct CnlDataPoint
        {
            public DateTime TimeStamp { get; set; }
            public double Val { get; set; }
            public int Stat { get; set; }
        }

        /// <summary>
        /// Stores the specified channel data.
        /// </summary>
        public double StorePrev(double val)
        {
            CnlDataPoints[CnlNum] = new CnlDataPoint
            {
                TimeStamp = DateTime.Now,
                Val = Val(CnlNum),
                Stat = Stat(CnlNum)
            };

            return val;
        }

        /// <summary>
        /// Gets the specified channel value.
        /// </summary>
        public double PrevVal(int n)
        {
            return CnlDataPoints.TryGetValue(n, out CnlDataPoint point) ?
                point.Val : 0.0;
        }

        /// <summary>
        /// Gets the specified channel status.
        /// </summary>
        public int PrevStat(int n)
        {
            return CnlDataPoints.TryGetValue(n, out CnlDataPoint point) ?
                point.Stat : 0;
        }

        /// <summary>
        /// Gets the derivative of the specified channel.
        /// </summary>
        public double Deriv(int n)
        {
            if (CnlDataPoints.TryGetValue(n, out CnlDataPoint point))
            {
                DateTime nowDT = DateTime.Now;
                return nowDT > point.TimeStamp ?
                    (Val(n) - point.Val) / (nowDT - point.TimeStamp).TotalSeconds : 0;
            }
            else
            {
                return 0.0;
            }
        }

        /// <summary>
        /// Gets the status of the channel derivative.
        /// </summary>
        public double DerivStat(int n)
        {
            return CnlStat > 0 && CnlDataPoints.TryGetValue(n, out CnlDataPoint point) && point.Stat > 0 ?
                CnlStat : 0;
        }
    }
}
