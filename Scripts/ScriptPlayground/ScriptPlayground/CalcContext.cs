using Scada.Data.Models;
using Scada.Server.Engine;

namespace ScriptPlayground
{
    /// <summary>
    /// Simulates access to channel data.
    /// <para>Имитирует доступ к данным каналов.</para>
    /// </summary>
    public class CalcContext : ICalcContext
    {
        private readonly Dictionary<int, CnlData> cnlDataDict = [];

        public DateTime Timestamp => DateTime.UtcNow;

        public bool IsCurrent => true;

        public CnlData GetCnlData(int cnlNum)
        {
            return cnlDataDict.GetValueOrDefault(cnlNum, CnlData.Empty);
        }

        public CnlData GetPrevCnlData(int cnlNum)
        {
            return CnlData.Empty;
        }

        public DateTime GetCnlTime(int cnlNum)
        {
            return DateTime.UtcNow;
        }

        public DateTime GetPrevCnlTime(int cnlNum)
        {
            return DateTime.MinValue;
        }

        public void SetCnlData(int cnlNum, CnlData cnlData)
        {
            cnlDataDict[cnlNum] = cnlData;
        }
    }
}
