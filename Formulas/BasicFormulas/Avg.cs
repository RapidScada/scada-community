using Scada.Data.Configuration;
using System.Collections.Generic;

namespace BasicFormulas
{
    /// <summary>
    /// The formulas for averaging.
    /// </summary>
    /// <remarks>
    /// Usage example:
    /// 1. Add the content of the class to the Formulas table.
    /// 2. Set the Formula field of an input channel to the following: MovAvg(5); MovAvgStat()
    /// This means that the input channel value is the average of the last 5 values.
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
            public double Sum { get; private set; }
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
        /// Adds a new or gets an existing MovAvgItem.
        /// </summary>
        public MovAvgItem GetOrAddMovAvgItem(int cnlNum, int pointCount)
        {
            if (MovAvgItems.TryGetValue(cnlNum, out MovAvgItem movAvgItem))
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
        /// Calculates moving average of the current input channel.
        /// </summary>
        public double MovAvg(int pointCount)
        {
            MovAvgItem movAvgItem = GetOrAddMovAvgItem(CnlNum, pointCount);
            movAvgItem.Append(CnlVal);
            return movAvgItem.Avg;
        }

        /// <summary>
        /// Gets status for an averaged channel.
        /// </summary>
        public int MovAvgStat()
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) && movAvgItem.Count > 0 ? 
                CnlStat : BaseValues.CnlStatuses.Undefined;
        }

        /// <summary>
        /// Gets the sum of all points included in the average.
        /// </summary>
        public double MovAvgSum(int cnlNum)
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) ?
                movAvgItem.Sum : 0;
        }

        /// <summary>
        /// Gets the number of all points included in the average.
        /// </summary>
        public int MovAvgCount(int cnlNum)
        {
            return MovAvgItems.TryGetValue(CnlNum, out MovAvgItem movAvgItem) ?
                movAvgItem.Count : 0;
        }
    }
}
