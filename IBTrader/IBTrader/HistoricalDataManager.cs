using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;

namespace IBTrader
{
    public class HistoricalEntry
    {
        public int TicklerId;
        public Contract Contract;
        public string EndTime;
        public string Duratin;
        public string BarSize;
        public string WhatToShow;
        public int UseRTH;
        public HistoricalEntry(
            int tickerId,
            Contract contract,
            string endTime,
            string duration,
            string barSize,
            string whatToShow,
            int useRTH)
        {
            TicklerId = tickerId;
            Contract = contract;
            EndTime = endTime;
            Duratin = duration;
            BarSize = barSize;
            WhatToShow = whatToShow;
            UseRTH = useRTH;
            if(!validateHDEntry())
            {
                throw new Exception("Historical data entry error.");
            }
        }
        private bool validateHDEntry()
        {
            return true;
        }
    }
    public class HistoricalDataManager
    {
        delegate void MessageHandlerDelegate(IBMessage message);
        public HistoricalDataManager()
        {

        }
        public bool SaveCSV(IBClient ibClient, HistoricalEntry entry)
        {
            entry.TicklerId = (int) TickerIdType.BID;
            entry.WhatToShow = "BID";
            getData(ibClient, entry);

            entry.TicklerId = (int) TickerIdType.ASK;
            entry.WhatToShow = "ASK";
            getData(ibClient, entry);
            return true;
        }
        private void getData(IBClient ibClient, HistoricalEntry entry)
        { 
            ibClient.ClientSocket.reqHistoricalData(
                entry.TicklerId, 
                entry.Contract, 
                entry.EndTime,
                entry.Duratin, 
                entry.BarSize, 
                entry.WhatToShow, 
                entry.UseRTH, 
                1, new List<TagValue>());
        }

    }
}
