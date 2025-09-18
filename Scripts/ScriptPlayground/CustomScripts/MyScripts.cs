using ScriptPlayground;

namespace CustomScripts
{
    /// <summary>
    /// Represents an example of custom scripts.
    /// </summary>
    public class MyScripts : ScriptTester
    {
        /// <summary>
        /// Linear transforms the input channel value.
        /// </summary>
        public double LinearTransform(double a, double b)
        {
            return a * Cnl + b;
        }

        /// <summary>
        /// Calculates the sum of the values of the specified input channels.
        /// </summary>
        public double CnlValSum(params int[] cnlNums)
        {
            double sum = 0.0;

            if (cnlNums != null)
            {
                foreach (int cnlNum in cnlNums)
                {
                    sum += Val(cnlNum);
                }
            }

            return sum;
        }
    }
}
