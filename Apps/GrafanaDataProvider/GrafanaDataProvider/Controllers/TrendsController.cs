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
    /// <summary>
    /// Represents data for plotting a graph.
    /// </summary>
    public class TrendsController : ApiController
    {
        /// <summary>
        /// The application log file name.
        /// </summary>
        private const string LogFileName = "GrafanaGrafic.log";

        /// <summary>
        /// The application log.
        /// </summary>
        private static readonly Log Log;

        /// <summary>
        /// Communicates with the Server application.
        /// </summary>
        protected static ServerComm serverComm;

        
        static TrendsController()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string LogDir = path + "log" + Path.DirectorySeparatorChar;
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;

            AppSettings appSettings = new AppSettings();
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;

            string errMsg;
            if (!appSettings.Load(out errMsg))
                Log.WriteAction(errMsg, Log.ActTypes.Exception);
            else
            {
                CommSettings settings = new CommSettings(appSettings.ServerHost, appSettings.ServerPort, appSettings.ServerUser,
                    appSettings.ServerPwd, appSettings.ServerTimeout);
                serverComm = new ServerComm(settings, Log);
            }
        }

#if DEBUG
        /// <summary>
        /// A simple example of building graphics.
        /// </summary>
        private static List<double?[]> PointCalc(GrafanaArg grafArg)
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
        }
#endif

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
        /// Returns an empty list.
        /// </summary>
        private IEnumerable<TrendData> GetEmptyTrend()
        {
            TrendData[] trends = new TrendData[]
            {
                new TrendData { target = "-1", datapoints = null }
            };
            return trends;
        }

        /// <summary>
        /// Determine the type of archive, hourly or minute.
        /// </summary>        
        private static void SelectArcType(GrafanaArg grafArg, out bool isHour, out int coef)
        {
            coef = 1;
            isHour = false;

            long diff = DateTimeOffset.Parse(grafArg.range.to).ToUnixTimeMilliseconds() -
                DateTimeOffset.Parse(grafArg.range.from).ToUnixTimeMilliseconds();

            // more than 24 h
            if (diff / 60000 > 1440)
            {
                coef = 60;
                isHour = true;
            }
            else
            {
                coef = 1;
                isHour = false;
            }
        }

        /// <summary>
        /// Iterates through the dates.
        /// </summary>        
        public IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
        {
            for (DateTime day = from; day.Date <= to; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        /// <summary>
        /// Gets points of data on specific channels number.
        /// </summary>
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
                else if (!DateTime.TryParse(grafanaArg.range.from, out DateTime from) || 
                    !DateTime.TryParse(grafanaArg.range.to, out DateTime to))
                {
                    Log.WriteError("It is not possible to read the dates");
                    return GetEmptyTrend();
                }
                else
                {
                    List<double?[]> points = new List<double?[]>();                    

                    SelectArcType(grafanaArg, out bool isHour, out int timeCoef);
                    TrendData[] trends = new TrendData[grafanaArg.targets.Length];

                    for (int i = 0; i < grafanaArg.targets.Length; i++)
                    {
                        points = new List<double?[]>();
                        if (!int.TryParse(grafanaArg.targets[i].target.Trim(), out int cnlNum))
                        {
                            Log.WriteError("It is not possible to read the dates for the channel " + cnlNum);
                            trends[i] = new TrendData { target = "-1", datapoints = null };                               
                        }
                        else
                        {
                            foreach (DateTime date in EachDay(from, to))
                            {
                                Trend trend = GetTrend(date, cnlNum, isHour);
                                for (int i1 = 0; i1 < trend.Points.Count; i1++)
                                {
                                    if (i1 > 0)
                                    {
                                        if ((DateTimeOffset.Parse(trend.Points[i1].DateTime.ToString()).ToUnixTimeMilliseconds() -
                                             DateTimeOffset.Parse(trend.Points[i1 - 1].DateTime.ToString()).ToUnixTimeMilliseconds()) > timeCoef * 60000)
                                        {
                                            points.Add(new double?[] { null,
                                                        DateTimeOffset.Parse(trend.Points[i1-1].DateTime.ToString()).
                                                        ToUnixTimeMilliseconds() + timeCoef * 60000 });
                                        }
                                        else
                                        {
                                            points.Add(new double?[] { trend.Points[i1].Val,
                                                        DateTimeOffset.Parse(trend.Points[i1].DateTime.ToString()).ToUnixTimeMilliseconds() });
                                        }
                                    }
                                    else
                                    {
                                        points.Add(new double?[] { trend.Points[i1].Val,
                                                    DateTimeOffset.Parse(trend.Points[i1].DateTime.ToString()).ToUnixTimeMilliseconds() });
                                    }
                                }
                            }
                            trends[i] = new TrendData { target = cnlNum.ToString(), datapoints = points };
                            Log.WriteAction("Channel data received " + cnlNum);
                        }
                    }

                    return trends;
                }
            }
        }
    }
}
