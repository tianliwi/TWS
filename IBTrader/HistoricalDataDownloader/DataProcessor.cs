using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBTrader;

namespace HistoricalDataDownloader
{
    public class DataProcessor
    {
        public DataRepo dataRepo;
        SortedList<DateTime, FxHistoricalDataEntry> H4;
        SortedList<DateTime, FxHistoricalDataEntry> D1;

        public DateTime tickMinDate;
        public DateTime tickMaxDate;

        public DataProcessor()
        {
            dataRepo = new DataRepo();
            dataRepo.loadM1();
        }

        public void generateData()
        {
            tickMinDate = dataRepo.DataM1.Keys.Min();
            tickMaxDate = dataRepo.DataM1.Keys.Max();
            DateTime curDate = tickMinDate;
            DateTime prevDate = curDate.AddDays(-1);
            FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
            clearEntry(entry);

            while(curDate < tickMaxDate)
            {
                //Console.WriteLine("{0}  {1}", prevDate, curDate);
                if (curDate.Date > prevDate.Date)
                {
                    if(entry.Date != "")
                    {
                        FxHistoricalDataEntry d = new FxHistoricalDataEntry();
                        d.Date = entry.Date;
                        d.OpenAsk = entry.OpenAsk;
                        d.OpenBid = entry.OpenBid;
                        d.HighAsk = entry.HighAsk;
                        d.HighBid = entry.HighBid;
                        d.LowAsk = entry.LowAsk;
                        d.LowBid = entry.LowBid;
                        d.CloseAsk = entry.CloseAsk;
                        d.CloseBid = entry.CloseBid;
                        //D1.Add(Convert.ToDateTime(d.Date), d);
                        Console.WriteLine(d.ToString());
                    }
                    clearEntry(entry);
                    entry.Date = Convert.ToDateTime(curDate.Date).ToString("yyyyMMdd HH:mm:ss");
                }
                if (!dataRepo.DataM1.ContainsKey(curDate))
                {
                    prevDate = curDate;
                    curDate = curDate.AddMinutes(1);
                    continue;
                }
                FxHistoricalDataEntry curEntry = dataRepo.DataM1[curDate];

                if (entry.OpenAsk == -1) entry.OpenAsk = curEntry.OpenAsk;
                if (entry.OpenBid == -1) entry.OpenBid = curEntry.OpenBid;

                if (entry.HighAsk < curEntry.HighAsk)
                {
                    entry.HighAsk = curEntry.HighAsk;
                }
                if (entry.HighBid < curEntry.HighBid)
                {
                    entry.HighBid = curEntry.HighBid;
                }

                if (entry.LowAsk > curEntry.LowAsk)
                {
                    entry.LowAsk = curEntry.LowAsk;
                }
                if (entry.LowBid > curEntry.LowBid)
                {
                    entry.LowBid = curEntry.LowBid;
                }

                entry.CloseAsk = curEntry.CloseAsk;
                entry.CloseBid = curEntry.CloseBid;

                prevDate = curDate;
                curDate = curDate.AddMinutes(1);
            }

            foreach(var p in D1)
            {
                Console.WriteLine(p.Value.ToString());
            }
        }

        public void clearEntry(FxHistoricalDataEntry entry)
        {
            entry.Date = "";
            entry.OpenAsk = -1;
            entry.OpenBid = -1;
            entry.HighAsk = -1;
            entry.HighBid = -1;
            entry.LowAsk = 1000;
            entry.LowBid = 1000;
            entry.CloseAsk = -1;
            entry.CloseBid = -1;
        }
    }
}
