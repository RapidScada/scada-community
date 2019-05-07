using System;
using System.Collections.Generic;

namespace BasicFormulas
{
    /// <summary>
    /// The formulas for averaging.
    /// </summary>
    /// <remarks>
    /// Usage example:
    /// 1. Add the content of the class to the Formulas table.
    /// 2.1. Set the Formula field of an input channel to the following: MovAvg(5); AvgStat()
    /// This means that the input channel value is the average of the last 5 values.
    /// 2.2. To average values over a time span: TimeAvg(10, 5); AvgStat()
    /// This means that the input channel value is the average over the last 10 seconds and includes max. 5 last values.
    /// 
    /// These formulas are sponsored by Horacio Venturino from Uruguay.
    /// </remarks>
    public class Avg : FormulaTester.FormulaTester
    {
        /// <summary>
        /// Data of moving averages accessing by input channels.
        /// </summary>
        public Dictionary<int, MovAvgItem> MovAvgItems = new Dictionary<int, MovAvgItem>();

        /// <summary>
        /// Item that provides moving averaging of one input channel.
        /// </summary>
        public class MovAvgItem
        {
            public MovAvgItem(int pointCount)
            {
                PointCount = pointCount;
                Values = new Queue<double>(pointCount);
                Sum = 0.0;
            }

            public int PointCount { get; private set; }
            public Queue<double> Values { get; private set; }
            public double Sum { get; protected set; }
            public int Count
            {
                get
                {
                    return Values.Count;
                }
            }
            public double Avg
            {
                get
                {
                    int count = Count;
                    return count == 0 ? double.NaN : Sum / count;
                }
            }

            public void Append(double val)
            {
                if (Count >= PointCount)
                    Sum -= Values.Dequeue();
                Values.Enqueue(val);
                Sum += val;
            }
        }

        /// <summary>
        /// Item that provides averaging of one input channel over a time span.
        /// </summary>
        public class TimeAvgItem : MovAvgItem
        {
            public TimeAvgItem(int timeSpanSec, int maxPointCount)
                : base(maxPointCount)
            {
                TimeSpan = TimeSpan.FromSeconds(timeSpanSec);
                TimeStamps = new Queue<DateTime>(maxPointCount);
            }

            public TimeSpan TimeSpan { get; private set; }
            public Queue<DateTime> TimeStamps { get; private set; }

            public new void Append(double val)
            {
                DateTime utcNowDT = DateTime.UtcNow;

                while (Count > 0)
                {
                    DateTime dateTime = TimeStamps.Peek();
                    if (utcNowDT - dateTime <= TimeSpan)
                    {
                        break;
                    }
                    else
                    {
                        TimeStamps.Dequeue();
                        Sum -= Values.Dequeue();
                    }
                }

                TimeStamps.Enqueue(utcNowDT);
                Values.Enqueue(val);
                Sum += val;
            }
        }

        /// <summary>
        /// Adds a new or gets an existing MovAvgItem.
        /// </summary>
        public MovAvgItem GetOrAddMovAvgItem(int cnlNum, int pointCount)
        {
            MovAvgItem movAvgItem;
            if (MovAvgItems.TryGetValue(cnlNum, out movAvgItem))
            {
                return movAvgItem;
            }
            else
            {
                movAvgItem = new MovAvgItem(pointCount);
                MovAvgItems.Add(cnlNum, movAvgItem);
                return movAvgItem;
            }
        }

        /// <summary>
        /// Adds a new or gets an existing TimeAvgItem.
        /// </summary>
        public TimeAvgItem GetOrAddTimeAvgItem(int cnlNum, int timeSpanSec, int maxPointCount)
        {
            MovAvgItem movAvgItem;
            if (MovAvgItems.TryGetValue(cnlNum, out movAvgItem) && movAvgItem is TimeAvgItem)
            {
                return (TimeAvgItem)movAvgItem;
            }
            else
            {
                TimeAvgItem timeAvgItem = new TimeAvgItem(timeSpanSec, maxPointCount);
                MovAvgItems[cnlNum] = timeAvgItem;
                return timeAvgItem;
            }
        }

        /// <summary>
        /// Calculates moving average of the current input channel.
        /// </summary>
        public double MovAvg(int pointCount)
        {
            return MovAvg(pointCount, CnlVal);
        }

        /// <summary>
        /// Calculates moving average of the current input channel.
        /// </summary>
        public double MovAvg(int pointCount, double cnlVal)
        {
            MovAvgItem movAvgItem = GetOrAddMovAvgItem(CnlNum, pointCount);
            movAvgItem.Append(cnlVal);
            return movAvgItem.Avg;
        }

        /// <summary>
        /// Calculates average of the current input channel over the time span.
        /// </summary>
        public double TimeAvg(int timeSpanSec, int maxPointCount)
        {
            return TimeAvg(timeSpanSec, maxPointCount, CnlVal);
        }

        /// <summary>
        /// Calculates average of the current input channel over the time span.
        /// </summary>
        public double TimeAvg(int timeSpanSec, int maxPointCount, double cnlVal)
        {
            TimeAvgItem timeAvgItem = GetOrAddTimeAvgItem(CnlNum, timeSpanSec, maxPointCount);
            timeAvgItem.Append(cnlVal);
            return timeAvgItem.Avg;
        }

        /// <summary>
        /// Gets the status for an averaged channel.
        /// </summary>
        public int AvgStat()
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) && movAvgItem.Count > 0 ? 
                CnlStat : 0 /*Undefined*/;
        }

        /// <summary>
        /// Gets the sum of all points included in the average.
        /// </summary>
        public double AvgSum(int cnlNum)
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) ?
                movAvgItem.Sum : 0;
        }

        /// <summary>
        /// Gets the number of all points included in the average.
        /// </summary>
        public int AvgCount(int cnlNum)
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) ?
                movAvgItem.Count : 0;
        }
    }
}
