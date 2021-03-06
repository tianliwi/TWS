﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBTrader
{
    public class DataRepo
    {
        public SortedList<DateTime, FxHistoricalDataEntry> DataM1;
        public SortedList<DateTime, FxHistoricalDataEntry> DataH4;
        public SortedList<DateTime, FxHistoricalDataEntry> DataD1;
        private string symbol;

        public DataRepo(string symbol, string[] years)
        {
            DataM1 = new SortedList<DateTime, FxHistoricalDataEntry>();
            DataH4 = new SortedList<DateTime, FxHistoricalDataEntry>();
            DataD1 = new SortedList<DateTime, FxHistoricalDataEntry>();
            this.symbol = symbol;
            foreach(string year in years)
            {
                LoadCSV(year);
            }
        }

        public void LoadCSV(string year)
        {
            LoadOneCSV(DataD1, year, "D1");
            LoadOneCSV(DataH4, year, "H4");
            LoadOneCSV(DataM1, year, "M1");
        }

        public void LoadOneCSV(SortedList<DateTime, FxHistoricalDataEntry> list, string year, string resolution)
        {
            loadHelper(list, Constants.BaseDir + symbol + "/" + year + "_" + resolution + ".csv");
        }

        private void loadHelper(SortedList<DateTime, FxHistoricalDataEntry> dataList, string filename)
        {
            if(!File.Exists(filename))
            {
                throw new IOException("Cannot find file " + filename);
            }
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
                entry.openTime = cols[0];
                entry.closeTime = cols[1];
                entry.OpenAsk = Convert.ToDouble(cols[2]);
                entry.OpenBid = Convert.ToDouble(cols[3]);
                entry.HighAsk = Convert.ToDouble(cols[4]);
                entry.HighBid = Convert.ToDouble(cols[5]);
                entry.LowAsk = Convert.ToDouble(cols[6]);
                entry.LowBid = Convert.ToDouble(cols[7]);
                entry.CloseAsk = Convert.ToDouble(cols[8]);
                entry.CloseBid = Convert.ToDouble(cols[9]);
                dataList[Trader.Rfc2Date(cols[0])] = entry;
            }
        }
    }
}
