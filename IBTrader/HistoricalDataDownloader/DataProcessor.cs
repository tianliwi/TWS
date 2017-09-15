using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBTrader;
using System.IO;

namespace HistoricalDataDownloader
{
    public class DataProcessor
    {
        public DataRepo dataRepo;
        SortedList<DateTime, FxHistoricalDataEntry> H4;
        List<FxHistoricalDataEntry> D1;
        private string year;

        public DateTime tickMinDate;
        public DateTime tickMaxDate;

        private string[] timeSlots = {
                "00:00:00", "04:00:00", "08:00:00",
                "12:00:00", "16:00:00", "20:00:00"};

        public DataProcessor(string year)
        {
            this.year = year;
            dataRepo = new DataRepo("EUR");
            D1 = new List<FxHistoricalDataEntry>();
            dataRepo.LoadOneCSV(dataRepo.DataM1, this.year, "M1");
        }

        public void generateData()
        {
            tickMinDate = dataRepo.DataM1.Keys.Min();
            tickMaxDate = dataRepo.DataM1.Keys.Max();
            DateTime curDate = tickMinDate;
            DateTime prevDate = curDate.AddDays(-1);
            FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
            clearEntry(entry);

            while (curDate <= tickMaxDate)
            {
                if(!dataRepo.DataM1.ContainsKey(curDate))
                {
                    curDate = curDate.AddMinutes(1);
                    continue;
                }
                if (prevDate.AddMinutes(1) != curDate)
                {
                    Console.WriteLine("{0}   {1}   {2}", prevDate, curDate, (TimeSpan)(curDate-prevDate));
                }
                if (getH4Slot(prevDate) != getH4Slot(curDate))
                {
                    //Console.WriteLine("{2} {0} {1}", curDate, timeSlots[getH4Slot(curDate)], prevDate);
                }
                prevDate = curDate;
                curDate = curDate.AddMinutes(1);
            }
        }

        public int getH4Slot(DateTime dt)
        {
            string t = dt.TimeOfDay.ToString();
            for(int k=5; k>=0; k--)
            {
                int cmp = string.Compare(t, timeSlots[k]);
                if (cmp >= 0) return k;
            }
            return 0;
        }

        public void generateD1()
        {
            tickMinDate = dataRepo.DataM1.Keys.Min();
            tickMaxDate = dataRepo.DataM1.Keys.Max();
            DateTime curDate = tickMinDate;
            DateTime prevDate = curDate.AddDays(-1);
            FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
            clearEntry(entry);

            while(curDate <= tickMaxDate)
            {
                //Console.WriteLine("{0}  {1}", prevDate, curDate);
                if (curDate.Date > prevDate.Date)
                {
                    if(entry.Date != "" && entry.OpenAsk > 0)
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
                        D1.Add(d);
                    }
                    clearEntry(entry);
                    entry.Date = Convert.ToDateTime(curDate.Date).ToString("yyyyMMdd  HH:mm:ss");
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
            if(entry.OpenAsk > 0)
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
                D1.Add(d);
            }
            File.WriteAllLines(Constants.BaseDir + "EUR/" + year + "/" + year +"_D1.csv", D1.Select(i => i.ToString()));
            Console.WriteLine("done");
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
