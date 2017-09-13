using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;

namespace HistoricalDataDownloader
{
    public class D1Loader
    {
        private string yearToWork;
        private string _startDate;
        private string _endDate;
        private string _symbol;

        public D1Loader(string startDate, string endDate, string symbol)
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
        }
        public void LoadM1Data()
        {
            DateTime startDate = Convert.ToDateTime(_startDate);
            DateTime endDate = Convert.ToDateTime(_endDate);  // endDate is excluded, i.e. [startDate, endDate)
            Console.WriteLine("Starting D1 {2} historical data downloading from {0} to {1}.", startDate.ToShortDateString(), endDate.AddDays(-1).ToShortDateString(), _symbol);
            yearToWork = startDate.Year.ToString();
            try
            {
                Trader trader = new Trader();
                trader.Connect();
                
                string _enddate = endDate.ToString("yyyyMMdd");

                Console.WriteLine("Requesting historical bars for {0} from {1} to {2}...", _symbol, startDate.ToShortDateString(), endDate.ToShortDateString());

                Contract contract = new Contract();
                string duration = "1 Y";
                string barSize = "1 day";
                contract.Symbol = _symbol;
                contract.SecType = "CASH";
                contract.Exchange = "IDEALPRO";
                contract.Currency = "USD";

                trader.GetHistoricalData((int)MessageType.FxHistoricalAsk, contract, endDate.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "ASK");
                Thread.Sleep(10000);

                trader.GetHistoricalData((int)MessageType.FxHistoricalBid, contract, endDate.ToString("yyyyMMdd HH:mm:ss") + " EST", duration, barSize, "BID");
                Thread.Sleep(10000);

                var filteredList = trader._fxHistoricalDataDict.Where(i => i.Key.Substring(0, 4) == _enddate.Substring(0, 4)).Select(i => i.Value.ToString());
                if (true)//(filteredList.Count() > 0)
                {
                    File.WriteAllLines("E:/GitHub/TWS/Data/" + _symbol + "/" + yearToWork + "/" + _enddate + "_D1.csv", trader._fxHistoricalDataDict.Select(i=>i.Value.ToString()));
                }
                trader._fxHistoricalDataDict.Clear();

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
    }
}
