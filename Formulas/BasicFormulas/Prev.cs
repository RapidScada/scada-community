using System;
using System.Collections.Generic;

namespace BasicFormulas
{
    /// <summary>
    /// The formulas for working with previous channel data.
    /// </summary>
    /// <remarks>
    /// Use StorePrev(val) to store previous data of an input channel.
    /// PrevVal(n), PrevStat(n), Deriv(n) and DerivStat(n) retrieve the previously saved data.
    /// </remarks>
    public class Prev : FormulaTester.FormulaTester
    {
        /// <summary>
        /// Channel data points accessing by channel numbers.
        /// </summary>
        public Dictionary<int, CnlDataPoint> CnlDataPoints = new Dictionary<int, CnlDataPoint>();
        /// <summary>
        /// Time stamps of the channel data.
        /// </summary>
        public Dictionary<int, DateTime> CnlTimeStamps = new Dictionary<int, DateTime>();

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
            DateTime dateTime;
            DateTime timeStamp = CnlTimeStamps.TryGetValue(CnlNum, out dateTime) ? 
                dateTime : DateTime.MinValue;

            CnlDataPoints[CnlNum] = new CnlDataPoint
            {
                TimeStamp = timeStamp,
                Val = Val(CnlNum),
                Stat = Stat(CnlNum)
            };

            CnlTimeStamps[CnlNum] = DateTime.Now;
            return val;
        }

        /// <summary>
        /// Gets the specified channel value.
        /// </summary>
        public double PrevVal(int n)
        {
            CnlDataPoint point; // don't use inline variable declaration
            return CnlDataPoints.TryGetValue(n, out point) ?
                point.Val : 0.0;
        }

        /// <summary>
        /// Gets the specified channel status.
        /// </summary>
        public int PrevStat(int n)
        {
            CnlDataPoint point;
            return CnlDataPoints.TryGetValue(n, out point) ?
                point.Stat : 0;
        }

        /// <summary>
        /// Gets the derivative of the specified channel.
        /// </summary>
        public double Deriv(int n)
        {
            CnlDataPoint point;
            if (CnlDataPoints.TryGetValue(n, out point) && point.TimeStamp > DateTime.MinValue)
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
            CnlDataPoint point;
            return Stat(n) > 0 && CnlDataPoints.TryGetValue(n, out point) && 
                point.TimeStamp > DateTime.MinValue && point.Stat > 0 ?
                Stat(n) : 0;
        }

        /// <summary>
        /// Gets the time difference between the current and previous data points.
        /// </summary>
        public double TimeDiff(int n)
        {
            CnlDataPoint point;
            if (CnlDataPoints.TryGetValue(n, out point) && point.TimeStamp > DateTime.MinValue)
            {
                return (DateTime.Now - point.TimeStamp).TotalSeconds;
            }
            else
            {
                return 0.0;
            }
        }
    }
}
