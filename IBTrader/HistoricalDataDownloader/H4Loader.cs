using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;

namespace HistoricalDataDownloader
{
    public class H4Loader
    {
        private string yearToWork;
        private string _startDate;
        private string _endDate;

        public H4Loader(string startDate, string endDate)
        {
            _startDate = startDate;
            if (string.IsNullOrEmpty(endDate))
            {
                _endDate = DateTime.Today.Date.ToString("MM/dd/yyyy");
            } else
            {
                _endDate = endDate;
            }
        }
        public void LoadM1Data()
        {
            DateTime startDate = Convert.ToDateTime(_startDate);
            DateTime endDate = Convert.ToDateTime(_endDate);  // endDate is excluded, i.e. [startDate, endDate)
            Console.WriteLine("Starting H4 historical data downloading from {0} to {1}.", startDate.ToShortDateString(), endDate.AddDays(-1).ToShortDateString());
            yearToWork = startDate.Year.ToString();
            try
            {
                Trader trader = new Trader();
                trader.Connect();

                foreach (DateTime day in EachMonth(startDate, endDate))
                {
                    string _enddate = day.ToString("yyyyMMdd");
                    GetHistoricalDataForAMonth(trader, _enddate, "EUR");
                }
                JoinDailyData("EUR");
                Console.WriteLine("Historical data downloading completed!");
                Console.ReadKey();
                trader.Disconnect();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private IEnumerable<DateTime> EachMonth(DateTime from, DateTime thru)
        {
            for (var day = from.Date; day.Date < thru.Date; day = day.AddMonths(1))
                yield return day;
        }

        private void GetHistoricalDataForAMonth(Trader trader, string _enddate, string sym)
        {


            DateTime _startTime = DateTime.SpecifyKind(DateTime.ParseExact(_enddate, "yyyyMMdd", null), DateTimeKind.Local);
            DateTime _endTime = _startTime.AddMonths(1);

            Console.WriteLine("Requesting historical bars for {0} from {1} to {2}...", sym, _startTime.ToShortDateString(), _endTime.ToShortDateString());

            Contract contract = new Contract();
            string duration = "30 D";
            string barSize = "4 hours";
            contract.Symbol = sym;
            contract.SecType = "CASH";
            contract.Exchange = "IDEALPRO";
            contract.Currency = "USD";

            trader.GetHistoricalData((int)MessageType.FxHistoricalAsk, contract, _endTime.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "ASK");
            Thread.Sleep(10000);

            trader.GetHistoricalData((int)MessageType.FxHistoricalBid, contract, _endTime.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "BID");
            Thread.Sleep(10000);

            var filteredList = trader._fxHistoricalDataDict.Where(i => i.Key.Substring(0, 6) == _enddate.Substring(0,6)).Select(i => i.Value.ToString());
            if (filteredList.Count() > 0)
            {
                File.WriteAllLines("E:/GitHub/TWS/Data/" + sym + "/" + yearToWork + "/Monthly/" + _enddate + "_H4.csv", filteredList);
            }
            trader._fxHistoricalDataDict.Clear();
        }

        private void JoinDailyData(string sym)
        {
            if (File.Exists("E:/GitHub/TWS/Data/" + sym + "/" + yearToWork + "/" + yearToWork + "_H4.csv"))
            {
                File.Delete("E:/GitHub/TWS/Data/" + sym + "/" + yearToWork + "/" + yearToWork + "_H4.csv");
            }
            string[] files = Directory.GetFiles("E:/GitHub/TWS/Data/" + sym + "/" + yearToWork + "/Monthly");
            foreach (string file in files)
            {
                File.AppendAllLines("E:/GitHub/TWS/Data/" + sym + "/" + yearToWork + "/" + yearToWork + "_H4.csv", File.ReadAllLines(file));
            }
        }
    }
}
