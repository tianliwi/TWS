using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTest
{
    public enum BTOrderType
    {
        Pending = 0,
        Open = 1,
        Closed = 2,
    }
    public enum BTOrderCloseType
    {
        TakeProfit = 0,
        StopLoss = 1,
        TimeOut = 2,
    }
    public class BTOrder
    {
        public int ID { get; set; }
        public string symbol { get; set; }
        public int size { get; set; }
        public BTOrderType status { get; set; }
        public double enterPrice { get; set; }
        public double closePrice { get; set; }
        public double takeProfit { get; set; }
        public double stopLoss { get; set; }
        public DateTime pendTime { get; set; }
        public DateTime enterTime { get; set; }
        public DateTime closeTime { get; set; }
        public DateTime goodTill { get; set; }
        public double pnl { get; set; }
        public BTOrderCloseType closeType { get; set; }
        public override string ToString()
        {
            return ID + "," + size + "," + enterPrice.ToString() +
                "," + enterTime.ToString("yyyyMMdd HH:mm:ss") + "," + closePrice.ToString() +
                "," + closeTime.ToString("yyyyMMdd HH:mm:ss") + "," + pnl.ToString();
        }
    }
}
