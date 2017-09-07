using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;

namespace HistoricalDataDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime startDate = Convert.ToDateTime("9/6/2017");
            DateTime endDate = Convert.ToDateTime("9/7/2017");

            try
            {
                Trader trader = new Trader();
                trader.Connect();

                foreach (DateTime day in EachDay(startDate, endDate))
                {
                    string _enddate = day.ToString("yyyyMMdd");
                    GetHistoricalDataForADay(trader, _enddate, "EUR");
                }
                JoinDailyData("EUR");
                Console.ReadKey();
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date < thru.Date; day = day.AddDays(1))
                yield return day;
        }

        static void GetHistoricalDataForADay(Trader trader, string _enddate, string sym)
        {


            DateTime _startTime = DateTime.SpecifyKind(DateTime.ParseExact(_enddate, "yyyyMMdd", null), DateTimeKind.Local);
            DateTime _endTime = _startTime + new TimeSpan(1, 0, 0, 0);          // one day later
            double sec = _endTime.Subtract(_startTime).TotalSeconds;      // should be 24 hours or 86,400 secs

            long space = 28800;

            long totalRequests = (long)sec / space;
            TimeSpan eightHours = new TimeSpan((int)space / 3600, 0, 0);

            DateTime s = _startTime;
            DateTime t = _startTime + eightHours;

            Console.WriteLine("Requesting historical bars for {0} on {1}...", sym, _startTime.ToShortDateString());
            for (int i = 0; i < totalRequests; i++)
            {
                Console.WriteLine("Request #: " + (i + 1).ToString() + "/" + totalRequests);

                Contract contract = new Contract();
                string duration = space.ToString() + " S";
                string barSize = "1 min";
                contract.Symbol = sym;
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

                trader.GetHistoricalData((int)MessageType.FxHistoricalAsk, contract, t.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "ASK");
                Thread.Sleep(10000);

                trader.GetHistoricalData((int)MessageType.FxHistoricalBid, contract, t.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "BID");
                Thread.Sleep(10000);

                s += eightHours;
                t += eightHours;
            }

            var filteredList = trader._fxHistoricalDataDict.Where(i => i.Key.Substring(0, 8) == _enddate).Select(i => i.Value.ToString());
            if (filteredList.Count() > 0) {
                File.WriteAllLines("E:/GitHub/TWS/Data/" + sym + "/2017/Daily/" + _enddate + "_M1.csv", filteredList);
            }
            trader._fxHistoricalDataDict.Clear();
        }

        static void JoinDailyData(string sym)
        {
            if(File.Exists("E:/GitHub/TWS/Data/" + sym + "/2017/2017_M1.csv"))
            {
                File.Delete("E:/GitHub/TWS/Data/" + sym + "/2017/2017_M1.csv");
            }
            string[] files = Directory.GetFiles("E:/GitHub/TWS/Data/" + sym + "/2017/Daily");
            foreach(string file in files)
            {
                File.AppendAllLines("E:/GitHub/TWS/Data/" + sym + "/2017/2017_M1.csv", File.ReadAllLines(file));
            }
        }

        static void testData()
        {

            try
            {
                Trader trader = new Trader();
                trader.Connect();

                Contract contract = new Contract();
                string endTime = "20170830  14:15:00";//"20170901  06:16:00 GMT";// DateTime.Now.ToString("yyyyMMdd HH:mm:ss");

                string duration = "900 S";
                string barSize = "1 min";
                contract.Symbol = "EUR";
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

				trader.ibClient.ClientSocket.reqHistoricalData((int)MessageType.FxHistoricalBid, contract, endTime, duration, barSize, "BID", 0, 1, new List<TagValue>());

                Console.ReadKey();
                foreach(var s in trader._fxHistoricalDataDict)
                {
					Console.WriteLine(s.Value.ToString());
                }
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
