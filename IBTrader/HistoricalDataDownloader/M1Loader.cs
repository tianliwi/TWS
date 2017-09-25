using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;

namespace HistoricalDataDownloader
{
    public class M1Loader
    {
        private string yearToWork;
        private string _startDate;
        private string _endDate;
        private string _symbol;

        public M1Loader(string startDate, string endDate, string symbol)
        {
            _startDate = startDate;
            if (string.IsNullOrEmpty(endDate))
            {
                _endDate = DateTime.Today.Date.ToString("MM/dd/yyyy");
            } else
            {
                _endDate = endDate;
            }
            _symbol = symbol;
            yearToWork = Convert.ToDateTime(_startDate).Year.ToString();
        }
        public void LoadM1Data()
        {
            DateTime startDate = Convert.ToDateTime(_startDate);
            DateTime endDate = Convert.ToDateTime(_endDate);  // endDate is excluded, i.e. [startDate, endDate)
            try
            {
                Trader trader = new Trader();
                trader.Connect();

                foreach (DateTime day in EachDay(startDate, endDate))
                {
                    string _enddate = day.AddHours(14).ToString("yyyyMMdd HH:mm:ss");
                    GetHistoricalDataForADay(trader, _enddate, _symbol);
                }
                JoinDailyData(_symbol);
                Console.WriteLine("Historical data downloading completed!");
                Console.ReadKey();
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date < thru.Date; day = day.AddDays(1))
                yield return day;
        }

        private void GetHistoricalDataForADay(Trader trader, string _enddate, string sym)
        {


            DateTime _startTime = DateTime.SpecifyKind(DateTime.ParseExact(_enddate, "yyyyMMdd HH:mm:ss", null), DateTimeKind.Local);
            DateTime _endTime = _startTime.AddDays(1);          // one day later
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

                trader.GetHistoricalData((int)MessageType.FxHistoricalAsk, contract, t.ToString("yyyyMMdd HH:mm:ss"), duration, barSize, "ASK");
                Thread.Sleep(10000);

                trader.GetHistoricalData((int)MessageType.FxHistoricalBid, contract, t.ToString("yyyyMMdd HH:mm:ss"), duration, barSize, "BID");
                Thread.Sleep(10000);

                s += eightHours;
                t += eightHours;
            }

            var filteredList = trader._fxHistoricalDataDict.Where(i => Trader.Rfc2DateLocal(i.Key) >= _startTime)
                .Where(i=> i.Value.OpenAsk * i.Value.OpenBid * i.Value.HighAsk * i.Value.HighBid * i.Value.LowAsk * i.Value.LowBid * i.Value.CloseAsk * i.Value.CloseBid >0)
                .Select(i => i.Value.ToString());
            if (filteredList.Count() > 0)
            {
                File.WriteAllLines(IBTrader.Constants.BaseDir + sym + @"\" + yearToWork + @"\" + _enddate.Substring(0,8) + "_M1.csv", filteredList);
            }
            trader._fxHistoricalDataDict.Clear();
        }

        public void JoinDailyData(string sym)
        {
            if (File.Exists(IBTrader.Constants.BaseDir + sym + @"\" + yearToWork + "_M1.csv"))
            {
                File.Delete(IBTrader.Constants.BaseDir + sym + @"\" + yearToWork + "_M1.csv");
            }
            string[] files = Directory.GetFiles(IBTrader.Constants.BaseDir + sym + "/" + yearToWork);
            foreach (string file in files)
            {
                File.AppendAllLines(IBTrader.Constants.BaseDir + sym + "/" + yearToWork + "_M1.csv", File.ReadAllLines(file));
            }
        }
    }
}
