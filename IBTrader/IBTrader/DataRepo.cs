using System;
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

        public DataRepo()
        {
            DataM1 = new SortedList<DateTime, FxHistoricalDataEntry>();
            DataH4 = new SortedList<DateTime, FxHistoricalDataEntry>();
        }

        public void LoadCSV()
        {
            loadM1();
            loadHelper(DataH4, "E:/GitHub/TWS/Data/AUD/2016/2016_H4.csv");
        }

        public void loadM1()
        {
            loadHelper(DataM1, "E:/GitHub/TWS/Data/AUD/2016/2016_M1.csv");
        }

        private void loadHelper(SortedList<DateTime, FxHistoricalDataEntry> dataList, string filename)
        {
            string[] lines = File.ReadAllLines(filename);
            foreach (string line in lines)
            {
                string[] cols = line.Split(',');
                FxHistoricalDataEntry entry = new FxHistoricalDataEntry();
                entry.Date = cols[0];
                entry.OpenAsk = Convert.ToDouble(cols[1]);
                entry.OpenBid = Convert.ToDouble(cols[2]);
                entry.HighAsk = Convert.ToDouble(cols[3]);
                entry.HighBid = Convert.ToDouble(cols[4]);
                entry.LowAsk = Convert.ToDouble(cols[5]);
                entry.LowBid = Convert.ToDouble(cols[6]);
                entry.CloseAsk = Convert.ToDouble(cols[7]);
                entry.CloseBid = Convert.ToDouble(cols[8]);
                dataList[DateTime.SpecifyKind(DateTime.ParseExact(cols[0], "yyyyMMdd  HH:mm:ss", null), DateTimeKind.Local)] = entry;
            }
        }
    }
}
