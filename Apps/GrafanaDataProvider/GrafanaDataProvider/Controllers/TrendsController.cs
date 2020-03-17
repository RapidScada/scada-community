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
        private static GrafanaArg grafArg = new GrafanaArg();
        private static Log log;
        internal const string LogFileName = "GrafanaGrafic.log";
        /// <summary>
        /// Default web application directory
        /// </summary>
        internal const string DefWebAppDir = @"C:\SCADA\ScadaWeb\";

        /// <summary>
        /// Object for data exchange with SCADA - Server
        /// </summary>
        protected static ServerComm serverComm;

        /// <summary>
        /// connection settings
        /// </summary>
        protected static CommSettings settings;

        /// <summary>
        /// Get the log directory
        /// </summary>
        private static string LogDir { get; set; }

        /// <summary>
        /// Get settings
        /// </summary>
        public AppSettings AppSettings { get; private set; }

        private TrendsController()
        {
            LogDir = DefWebAppDir + "log" + Path.DirectorySeparatorChar;
            log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            log.FileName = LogDir + LogFileName;
            
            AppSettings = new AppSettings();
            log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            log.FileName = LogDir + LogFileName;

            string errMsg;
            if (!AppSettings.Load(out errMsg))
                log.WriteAction(errMsg, Log.ActTypes.Exception);
            else
            {
                //settings = new CommSettings("92.63.105.201", 10000, "ScadaWeb", "KNcX7aeyrFX3Nc8z", 10000);          
                settings = new CommSettings(AppSettings.Server, AppSettings.Port, AppSettings.User, AppSettings.Password,
                    AppSettings.TimeOut);
                serverComm = new ServerComm(settings, log);
                log.WriteAction("Successfully connected server " + AppSettings.Server, Log.ActTypes.Information);
            }
        }

        /// <summary>
        /// Graphing example.
        /// </summary>
        /// <returns></returns>
        private static List<double[]> PointCalc()
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

            List<double[]> points = new List<double[]>();

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

                points.Add(new double[] { y, time });
                t0 += step;
            }

            return points;
        }

        /// <summary>
        /// Requests input channel data from Server.
        /// </summary>
        private static Trend GetTrend(DateTime date, int cnlNum, bool chekHours)
        {
            Trend trend = new Trend(cnlNum);
            string tableName = "";
            //min data
            if (chekHours)
                tableName = SrezAdapter.BuildHourTableName(date);
            else
                tableName = SrezAdapter.BuildMinTableName(date);

            bool dataReceived = serverComm.ReceiveTrend(tableName, date, trend);

            serverComm.Close();
            if (dataReceived)
            {
                trend.LastFillTime = DateTime.UtcNow;
            }
            else
            {
                log.WriteError("Unable to receive trend.");
            }

            return trend;
        }

        private static bool checkDateRange()
        {
            bool check = false;
            long diff = DateTimeOffset.Parse(grafArg.range.to).ToUnixTimeMilliseconds() -
                DateTimeOffset.Parse(grafArg.range.from).ToUnixTimeMilliseconds();

            // more than 24 h
            if (diff / 60000 > 1440)
                check = true;
            else
                check = false;

            return check;
        }

        public IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (var day = from; day.Date <= to; day = day.AddDays(1))
                yield return day;
        }

        [HttpGet, HttpPost]
        public IEnumerable<TrendData> GetDataForGraf([FromBody]GrafanaArg grafanaArg)
        {
            if (grafanaArg != null)
            {
                Trend trend;
                grafArg = grafanaArg;

                int cnlNum;

                try
                {
                    cnlNum = Convert.ToInt32(grafArg.targets[0].target.Trim());
                }
                catch
                {
                    cnlNum = -1;
                    log.WriteError("It is not possible to receive data on the channel " + cnlNum.ToString());
                }

                if (cnlNum != -1)
                {
                    List<double?[]> points = new List<double?[]>();

                    foreach (DateTime date in EachDay(Convert.ToDateTime(grafArg.range.from), Convert.ToDateTime(grafArg.range.to)))
                    {
                        bool checkRange = checkDateRange();
                        trend = GetTrend(date, cnlNum, checkRange);
                        int k = 1;
                        if (checkRange)
                            k = 60;

                        for (int i = 0; i < trend.Points.Count; i++)
                        {
                            if (i > 0)
                            {
                                if ((DateTimeOffset.Parse(trend.Points[i].DateTime.ToString()).ToUnixTimeMilliseconds() -
                                    DateTimeOffset.Parse(trend.Points[i - 1].DateTime.ToString()).ToUnixTimeMilliseconds()) > k * 60000)
                                    points.Add(new double?[] { null,
                                        DateTimeOffset.Parse(trend.Points[i-1].DateTime.ToString()).ToUnixTimeMilliseconds() + k* 60000 });
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
                        new TrendData {target = cnlNum.ToString(), datapoints = points }
                    };

                    log.WriteAction("Channel data received " + cnlNum.ToString());
                    return trends;
                }
                else
                    return null;
            }
            else
            {
                return null;
            }
        }
    }
}
