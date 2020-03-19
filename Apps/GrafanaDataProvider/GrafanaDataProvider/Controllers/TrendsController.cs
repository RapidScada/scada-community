using GrafanaDataProvider.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Scada.Client;
using Scada.Data.Tables;
using Utils;
using System.Text;
using System.IO;

namespace GrafanaDataProvider.Controllers
{
    public class TrendsController : ApiController
    {
        /// <summary>
        /// Logfile for Grafana Data Provider
        /// </summary>
        private const string LogFileName = "GrafanaGrafic.log";
        
        /// <summary>
        /// Settings params
        /// </summary>
        private static readonly AppSettings AppSettings;

        /// <summary>
        /// Object for log
        /// </summary>
        private static readonly Log Log;
        
        /// <summary>
        /// Object for data exchange with SCADA - Server
        /// </summary>
        protected static ServerComm serverComm;

        /// <summary>
        /// Connection settings
        /// </summary>
        protected static CommSettings settings;

        /// <summary>
        /// Get the log directory
        /// </summary>
        private static string LogDir { get; set; }

        
        static TrendsController()
        {
            LogDir = /*DefWebAppDir +*/ "log" + Path.DirectorySeparatorChar;
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;
            
            AppSettings = new AppSettings();
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;

            string errMsg;
            if (!AppSettings.Load(out errMsg))
                Log.WriteAction(errMsg, Log.ActTypes.Exception);
            else
            {
                settings = new CommSettings(AppSettings.ServerHost, AppSettings.ServerPort, AppSettings.ServerUser, 
                    AppSettings.ServerPwd, AppSettings.ServerTimeout);
                serverComm = new ServerComm(settings, Log);
            }
        }

        /// <summary>
        /// A simple example of building graphics.
        /// </summary>
        /// <returns></returns>
        /*private static List<double?[]> PointCalc(GrafanaArg grafArg)
        {
            long t1;
            long t0;

            if (grafArg.range == null)
            {
                t1 = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                t0 = t1 - 900000; // 15 min
            }
            else
            {
                t1 = DateTimeOffset.Parse(grafArg.range.to).ToUnixTimeMilliseconds();
                t0 = DateTimeOffset.Parse(grafArg.range.from).ToUnixTimeMilliseconds();
            }

            List<double?[]> points = new List<double?[]>();

            int step = 1000;
            if (grafArg.intervalMs > 0)
                step = grafArg.intervalMs;

            while (t0 < t1)
            {
                DateTimeOffset offset = DateTimeOffset.FromUnixTimeMilliseconds(t0);
                int day = offset.Day;
                int hour = offset.Hour;

                double time = t0;
                //double y = 100 * Math.Sin(time * 0.0001);
                double y = day * 100 + hour + 0.1 * Math.Sin(time * 0.0001);

                points.Add(new double?[] { y, time });
                t0 += step;
            }

            return points;
        }*/

        /// <summary>
        /// Requests input channel data from Server.
        /// </summary>
        private static Trend GetTrend(DateTime date, int cnlNum, bool chekHours)
        {
            string tableName = chekHours ? 
                SrezAdapter.BuildHourTableName(date) :
                SrezAdapter.BuildMinTableName(date);

            Trend trend = new Trend(cnlNum);
            bool dataReceived = serverComm.ReceiveTrend(tableName, date, trend);

            serverComm.Close();

            if (dataReceived)
                trend.LastFillTime = DateTime.UtcNow;
            else
                Log.WriteError("Unable to receive trend.");

            return trend;
        }

        /// <summary>
        /// Returns empty list.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<TrendData> GetEmptyTrend()
        {
            TrendData[] trends = new TrendData[]
            {
                new TrendData { target = "-1", datapoints = null }
            };
            return trends;
        }
        
        /// <summary>
        /// Definition of a minute or hour trend.
        /// </summary>        
        /// <returns></returns>
        private static bool CheckDateRange(GrafanaArg grafArg)
        {
            bool checkDataRange = false;
            long diff = DateTimeOffset.Parse(grafArg.range.to).ToUnixTimeMilliseconds() -
                DateTimeOffset.Parse(grafArg.range.from).ToUnixTimeMilliseconds();

            // more than 24 h
            if (diff / 60000 > 1440)
                checkDataRange = true;
            else
                checkDataRange = false;

            return checkDataRange;
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (DateTime day = from; day.Date <= to; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        [HttpGet, HttpPost]
        public IEnumerable<TrendData> GetDataForGrafana([FromBody]GrafanaArg grafanaArg)
        {
            if (grafanaArg == null)
            {
                return GetEmptyTrend();
            }
            else
            {
                if (grafanaArg.targets == null)
                {
                    Log.WriteError("It is not possible to receive data");
                    return GetEmptyTrend();
                }
                else
                {
                    if (!int.TryParse(grafanaArg.targets[0].target.Trim(), out int cnlNum))
                    {
                        Log.WriteError("It is not possible to read the dates for the channel " + cnlNum);
                        return GetEmptyTrend();
                    }
                    else
                    {
                        if (DateTime.TryParse(grafanaArg.range.from, out DateTime from) &&
                            DateTime.TryParse(grafanaArg.range.from, out DateTime to))
                        {
                            List<double?[]> points = new List<double?[]>();

                            foreach (DateTime date in EachDay(from, to)) 
                            {
                                bool checkRange = CheckDateRange(grafanaArg);
                                Trend trend = GetTrend(date, cnlNum, checkRange);
                                // k = 60 for hourly trend
                                int k = 1;
                                if (checkRange)
                                    k = 60;

                                for (int i = 0; i < trend.Points.Count; i++)
                                {
                                    if (i > 0)
                                    {
                                        if ((DateTimeOffset.Parse(trend.Points[i].DateTime.ToString()).ToUnixTimeMilliseconds() -
                                            DateTimeOffset.Parse(trend.Points[i - 1].DateTime.ToString()).
                                            ToUnixTimeMilliseconds()) > k * 60000)
                                            points.Add(new double?[] { null,
                                        DateTimeOffset.Parse(trend.Points[i-1].DateTime.ToString()).
                                        ToUnixTimeMilliseconds() + k* 60000 });
                                        else
                                            points.Add(new double?[] { trend.Points[i].Val,
                                                DateTimeOffset.Parse(trend.Points[i].DateTime.ToString()).ToUnixTimeMilliseconds() });
                                    }
                                    else
                                    {
                                        points.Add(new double?[] { trend.Points[i].Val,
                                            DateTimeOffset.Parse(trend.Points[i].DateTime.ToString()).ToUnixTimeMilliseconds() });
                                    }
                                }
                            }

                            TrendData[] trends = new TrendData[]
                            {
                                new TrendData { target = cnlNum.ToString(), datapoints = points /*PointCalc(grafanaArg)*/ }
                            };

                            Log.WriteAction("Channel data received " + cnlNum);
                            return trends;
                        }
                        else
                        {
                            Log.WriteError("It is not possible to read the dates for the channel " + cnlNum);
                            return GetEmptyTrend();
                        }
                    }                   
                }                
            }
        }
    }
}
