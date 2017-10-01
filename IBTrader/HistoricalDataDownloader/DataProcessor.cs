﻿using System;
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
        List<FxHistoricalDataEntry> H4;
        List<FxHistoricalDataEntry> D1;
        private string year;

        public DateTime tickMinDate;
        public DateTime tickMaxDate;

        private string[] timeSlots = {
                "02:00:00", "06:00:00", "10:00:00",
                "14:00:00", "18:00:00", "22:00:00"};

        public DataProcessor(string symbol, string year)
        {
            this.year = year;
            dataRepo = new DataRepo(symbol, new string[] { this.year });
            D1 = new List<FxHistoricalDataEntry>();
            H4 = new List<FxHistoricalDataEntry>();
            
            dataRepo.LoadOneCSV(dataRepo.DataM1, this.year, "M1");
        }

        public void generateH4(string symbol)
        {
            tickMinDate = dataRepo.DataM1.Keys.Min();
            tickMaxDate = dataRepo.DataM1.Keys.Max();
            DateTime curDate = tickMinDate;
            DateTime prevDate = curDate;
            FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
            clearEntry(entry);
            while (curDate <= tickMaxDate)
            {
                if (!dataRepo.DataM1.ContainsKey(curDate))
                {
                    curDate = curDate.AddMinutes(1);
                    continue;
                }
                FxHistoricalDataEntry cur = dataRepo.DataM1[curDate];
                if (entry.OpenAsk < 0)
                {
                    entry.OpenAsk = cur.OpenAsk;
                    entry.openTime = Trader.Date2Rfc(curDate);
                }
                if (entry.OpenBid < 0)
                {
                    entry.OpenBid = cur.OpenBid;
                }
                if (entry.HighAsk < cur.HighAsk)
                {
                    entry.HighAsk = cur.HighAsk;
                }
                if (entry.HighBid < cur.HighBid)
                {
                    entry.HighBid = cur.HighBid;
                }
                if (entry.LowAsk > cur.LowAsk)
                {
                    entry.LowAsk = cur.LowAsk;
                }
                if (entry.LowBid > cur.LowBid)
                {
                    entry.LowBid = cur.LowBid;
                }
                prevDate = curDate;
                curDate = curDate.AddMinutes(1);
                if (getH4Slot(prevDate) != getH4Slot(curDate))
                {
                    FxHistoricalDataEntry d = new FxHistoricalDataEntry();
                    d.openTime = entry.openTime;
                    d.closeTime = Trader.Date2Rfc(prevDate);
                    d.OpenAsk = entry.OpenAsk;
                    d.OpenBid = entry.OpenBid;
                    d.HighAsk = entry.HighAsk;
                    d.HighBid = entry.HighBid;
                    d.LowAsk = entry.LowAsk;
                    d.LowBid = entry.LowBid;
                    d.CloseAsk = cur.CloseAsk;
                    d.CloseBid = cur.CloseBid;
                    H4.Add(d);
                    clearEntry(entry);
                }
            }
            File.WriteAllLines(Constants.BaseDir + symbol + "/" + year + "_H4.csv", H4.Select(i => i.ToString()));
            Console.WriteLine("done");
        }

        public void CheckData()
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
                    Console.WriteLine("Discontinous date: {0}   {1}   {2}", prevDate, curDate, (TimeSpan)(curDate-prevDate));
                }
                FxHistoricalDataEntry temp = dataRepo.DataM1[curDate];
                if (string.IsNullOrEmpty(temp.openTime))
                {
                    Console.WriteLine("Found empty date at {0}: {1}", curDate, temp.ToString());
                }
                if (temp.OpenAsk == 0 || temp.OpenBid == 0 ||
                    temp.HighAsk == 0 || temp.HighBid == 0 ||
                    temp.LowAsk == 0 || temp.LowBid ==0 ||
                    temp.CloseAsk ==0 || temp.CloseBid == 0)
                {
                    Console.WriteLine("Found empty price at {0}: {1}", curDate, temp.ToString());
                }
                /*
                if (getH4Slot(prevDate) != getH4Slot(curDate))
                {
                    Console.WriteLine("{2} {0} {1}", curDate, timeSlots[getH4Slot(curDate)], prevDate);
                }
                */
                prevDate = curDate;
                curDate = curDate.AddMinutes(1);
            }
        }

        public int getH4Slot(DateTime dt)
        {
            string t = dt.ToLocalTime().TimeOfDay.ToString();
            int c = string.Compare(t, timeSlots[0]);
            if (c < 0) return 5;
            for(int k=5; k>=0; k--)
            {
                int cmp = string.Compare(t, timeSlots[k]);
                if (cmp >= 0) return k;
            }
            return 0;
        }

        public void generateD1(string symbol)
        {
            string[] files = Directory.GetFiles(Constants.BaseDir + symbol + "/" + year);
            foreach(string file in files)
            {
                string[] lines = File.ReadAllLines(file);
                FxHistoricalDataEntry d = new FxHistoricalDataEntry();
                clearEntry(d);
                foreach (string line in lines)
                {
                    string[] cols = line.Split(',');
                    // this is the first minute bar
                    if (string.IsNullOrEmpty(d.openTime))
                    {
                        d.openTime = cols[0];
                        d.OpenAsk = Convert.ToDouble(cols[2]);
                        d.OpenBid = Convert.ToDouble(cols[3]);
                    }
                    double ha = Convert.ToDouble(cols[4]);
                    double hb = Convert.ToDouble(cols[5]);
                    double la = Convert.ToDouble(cols[6]);
                    double lb = Convert.ToDouble(cols[7]);
                    d.HighAsk = Math.Max(d.HighAsk, ha);
                    d.HighBid = Math.Max(d.HighBid, hb);
                    d.LowAsk = Math.Min(d.LowAsk, la);
                    d.LowBid = Math.Min(d.LowBid, lb);
                }
                string[] c = lines[lines.Length - 1].Split(',');
                d.closeTime = c[0];
                d.CloseAsk = Convert.ToDouble(c[8]);
                d.CloseBid = Convert.ToDouble(c[9]);
                D1.Add(d);
            }
            File.WriteAllLines(Constants.BaseDir + symbol + "/" + year +"_D1.csv", D1.Select(i => i.ToString()));
            Console.WriteLine("done");
        }

        public void clearEntry(FxHistoricalDataEntry entry)
        {
            entry.openTime = "";
            entry.closeTime = "";
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
