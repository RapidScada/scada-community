using GrafanaDataProvider.Models;
using System;
using System.Collections.Generic;
using System.Web.Http;
using Scada.Client;
using Scada.Data.Tables;
using Utils;
using System.Text;
using System.IO;
using Scada.Data.Models;
using System.Text.RegularExpressions;

namespace GrafanaDataProvider.Controllers
{
    /// <summary>
    /// Represents a controller for plotting a graph.
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
        protected static readonly ServerComm serverComm;

        /// <summary>
        /// Cache of the data received from SCADA-Server for clients usage.
        /// </summary>
        protected static readonly DataCache dataCache;

        /// <summary>
        /// The object for thread-safe access to client cache data.
        /// </summary>
        protected static readonly DataAccess dataAccess;


        /// <summary>
        /// Initializes the class.
        /// </summary>
        static TrendsController()
        {
            string path = AppDomain.CurrentDomain.BaseDirectory;
            string LogDir = path + "log" + Path.DirectorySeparatorChar;
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;

            AppSettings appSettings = new AppSettings();
            Log = new Log(Log.Formats.Simple) { Encoding = Encoding.UTF8 };
            Log.FileName = LogDir + LogFileName;

            if (!appSettings.Load(out string errMsg))
                Log.WriteAction(errMsg, Log.ActTypes.Exception);
            else
            {
                CommSettings settings = new CommSettings(appSettings.ServerHost, appSettings.ServerPort, appSettings.ServerUser,
                    appSettings.ServerPwd, appSettings.ServerTimeout);
                serverComm = new ServerComm(settings, Log);
                dataCache = new DataCache(serverComm, Log);
                dataAccess = new DataAccess(dataCache, Log);
            }
        }
        /*
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
                t1 = grafArg.range.to;
                t0 = grafArg.range.from;
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
        */

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

            //serverComm.Close();

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
        /// Converts the specified date and time to the unix milliseconds.
        /// </summary>
        private static long GetUnixTimeMs(DateTime dateTime)
        {           
            return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
        }

        /// <summary>
        /// Converts the specified unix milliseconds time to date and time. (added 04/01/2021 by Folharini)
        /// </summary>
        private static DateTime GetDateFromUnixMs(long timeMilliseconds)
        {
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            date = date.AddMilliseconds(timeMilliseconds).ToLocalTime();
            return date;

        }

        /// <summary>
        /// Converts the specified string to DateTime and later to Unix (added 08/03/2021 by Folharini)
        /// </summary>
        private static long GetUnixFromString(string timeString)
        {
            DateTime date = Convert.ToDateTime(timeString);
            return new DateTimeOffset(date).ToUnixTimeMilliseconds();
        }
        
        /// <summary>
        /// Converts the specified date and time to the local time.
        /// </summary>
        private static DateTime UtcToLocalTime(DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc).ToLocalTime();
        }

        /// <summary>
        /// Converts local date and time to the univeral time.
        /// </summary>
        private static DateTime LocalToUtcTime(DateTime dateTime)
        {
            return DateTime.SpecifyKind(dateTime, DateTimeKind.Local).ToUniversalTime();
        }

        /// <summary>
        /// Determine the type of archive, hourly or minute.
        /// </summary>        
        private static void SelectArcType(GrafanaArg grafArg, out bool isHour, out int coef)
        {
            long diff = grafArg.endTime - grafArg.startTime;

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
        private IEnumerable<DateTime> EachDay(DateTime from, DateTime to)
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
                else
                {
                    SelectArcType(grafanaArg, out bool isHour, out int timeCoef);
                    TrendData[] trends = new TrendData[grafanaArg.targets.Length];
                    /// long fromMs = GetUnixTimeMs(grafanaArg.range.from);
                    /// long toMs = GetUnixTimeMs(grafanaArg.range.to);
                    /// 

                    /// Range conversion to Unix
                    long fromMs = 0;
                    long toMs = 0;

                    string fromStrNum = grafanaArg.range.from;
                    string fromStr = Regex.Replace(fromStrNum, "[0-9]", "");
                    string _strfromNum = Regex.Replace(fromStrNum, "[^0-9.]", "");
                    if (!String.IsNullOrEmpty(fromStr))
                    {
                        if (fromStr == "--T::.Z") // If data received is in DateTime form, as original version for SimpleJson eg: 2021-03-09T15:42:21.500Z
                        {
                            fromMs = GetUnixFromString(fromStrNum); 
                        }
                        if (fromStr == "h")
                        {
                            if (Int32.TryParse(_strfromNum, out int fromNum))
                            {
                                long fromNumMs = fromNum * 3600000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                fromMs = nowMilliseconds - fromNumMs;
                            } else
                            {
                                Log.WriteError("It is not possible to parse the number of hours from " + fromStrNum);
                            }
                        }
                        if (fromStr == "m")
                        {
                            if (Int32.TryParse(_strfromNum, out int fromNum))
                            {
                                long fromNumMs = fromNum * 60000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                fromMs = nowMilliseconds - fromNumMs;
                            }
                            else
                            {
                                Log.WriteError("It is not possible to parse the number of minutes from " + fromStrNum);
                            }
                        }
                        if (fromStr == "s")
                        {
                            if (Int32.TryParse(_strfromNum, out int fromNum))
                            {
                                long fromNumMs = fromNum * 1000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                fromMs = nowMilliseconds - fromNumMs;
                            }
                            else
                            {
                                Log.WriteError("It is not possible to parse the number of seconds from " + fromStrNum);
                            }
                        }
                    }
                    else
                    {
                        if(Int64.TryParse(fromStrNum, out long _intfromMs))
                        {
                            fromMs = _intfromMs;
                            Log.WriteAction("Defined 'from' time: " + fromMs);
                        }
                        else
                        {
                            Log.WriteError("'From' string is null or empty: " + fromStrNum);
                        }
                    }

                    string toStrNum = grafanaArg.range.to;
                    string toStr = Regex.Replace(toStrNum, "[0-9]", "");
                    string _strtoNum = Regex.Replace(toStrNum, "[^0-9.]", "");
                    if (!String.IsNullOrEmpty(toStr))
                    {
                        if (toStr == "--T::.Z") // If data received is in DateTime form, as original version for SimpleJson eg: 2021-03-09T15:42:21.500Z
                        {
                            toMs = GetUnixFromString(toStrNum);
                        }
                        if (toStr == "now-h")
                        {
                            if (Int32.TryParse(_strtoNum, out int toNum))
                            {
                                long toNumMs = toNum * 3600000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                toMs = nowMilliseconds - toNumMs;
                            }
                            else
                            {
                                Log.WriteError("It is not possible to parse the number of hours from " + toStrNum);
                            }
                        }
                        if (toStr == "now-m")
                        {
                            if (Int32.TryParse(_strtoNum, out int toNum))
                            {
                                long toNumMs = toNum * 60000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                toMs = nowMilliseconds - toNumMs;
                            }
                            else
                            {
                                Log.WriteError("It is not possible to parse the number of minutes from " + toStrNum);
                            }
                        }
                        if (toStr == "now-s")
                        {
                            if (Int32.TryParse(_strtoNum, out int toNum))
                            {
                                long toNumMs = toNum * 1000;
                                DateTimeOffset now = DateTimeOffset.UtcNow;
                                long nowMilliseconds = now.ToUnixTimeMilliseconds();
                                toMs = nowMilliseconds - toNumMs;
                            }
                            else
                            {
                                Log.WriteError("It is not possible to parse the number of seconds from " + toStrNum);
                            }
                        }
                        if (toStr == "now")
                        {
                            //long toNumMs = toNum * 1000;
                            DateTimeOffset now = DateTimeOffset.UtcNow;
                            long nowMilliseconds = now.ToUnixTimeMilliseconds();
                            toMs = nowMilliseconds;
                        }
                    }
                    else
                    {
                        if (Int64.TryParse(toStrNum, out long _inttoMs))
                        {
                            toMs = _inttoMs;
                            Log.WriteAction("Defined 'from' time: " + toMs);
                        }
                        else
                        {
                            Log.WriteError("'From' string is null or empty: " + toStrNum);
                        }
                    }

                    grafanaArg.startTime = fromMs;
                    grafanaArg.endTime = toMs;


                    for (int i = 0; i < grafanaArg.targets.Length; i++)
                    {
                        List<double?[]> points = new List<double?[]>();
                        if (!int.TryParse(grafanaArg.targets[i].target.Trim(), out int cnlNum))
                        {
                            Log.WriteError("It is not possible to read the dates for the channel " + cnlNum);
                            trends[i] = new TrendData { target = "-1", datapoints = null };
                        }
                        else
                        {
                            foreach (DateTime date in EachDay (GetDateFromUnixMs(fromMs), GetDateFromUnixMs(toMs)))
                            {
                                Trend trend = GetTrend(UtcToLocalTime(date), cnlNum, isHour);

                                for (int i1 = 0; i1 < trend.Points.Count; i1++)
                                {
                                    long pointMs = GetUnixTimeMs(LocalToUtcTime(trend.Points[i1].DateTime));

                                    if (pointMs >= fromMs && pointMs <= toMs)
                                    {
                                        if (i1 > 0)
                                        {
                                            long prevMs = GetUnixTimeMs(LocalToUtcTime(trend.Points[i1 - 1].DateTime));

                                            if (pointMs - prevMs > timeCoef * 60000)
                                            {
                                                points.Add(new double?[] { null, prevMs + timeCoef * 60000 });
                                            }
                                            else
                                            {
                                                if (trend.Points[i1].Stat > 0)
                                                    points.Add(new double?[] { trend.Points[i1].Val, pointMs });
                                                else
                                                    points.Add(new double?[] { null, pointMs });
                                            }
                                        }
                                        else
                                        {
                                            if (trend.Points[i1].Stat > 0)
                                                points.Add(new double?[] { trend.Points[i1].Val, pointMs });
                                            else
                                                points.Add(new double?[] { null, pointMs });
                                        }
                                     }
                                }
                            }

                            InCnlProps inCnlProps = dataAccess.GetCnlProps(cnlNum);
                            string cnlName = inCnlProps == null ? "" : inCnlProps.CnlName;

                            trends[i] = new TrendData { target = "[" + cnlNum + "] " + cnlName, datapoints = points };
                            Log.WriteAction("Channel data received " + cnlNum);
                        }
                    }

                    return trends;
                }
            }
        }
    }
}
