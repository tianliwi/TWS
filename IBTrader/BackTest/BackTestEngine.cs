using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBApi;
using IBTrader;
using System.ComponentModel;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Globalization;

namespace BackTest
{
    public class BackTestEngine
    {
        private DataRepo data;
        List<BTOrder> orderList;

        public BackTestEngine()
        {
            data = new DataRepo("AUD", "2016");
            data.LoadCSV();
            orderList = new List<BTOrder>();
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        public double Start(double a, double b)
        {
            DateTime curTime;
            double PNL = 0;
            int win = 0;
            int loss = 0;
            int id = 0;
            orderList.Clear();
            foreach (var h4Pair in data.DataH4)
            {
                curTime = h4Pair.Key;
                var hl = getAskHighLow(data.DataH4, curTime, 12);
                if (hl.Item1 < 0 || hl.Item2 < 0) continue;
                //var hl2 = getAskHighLow(data.H4, curTime, 18);
                //if (hl2.Item1 < 0 || hl2.Item2 < 0) continue;

                double R = hl.Item1 - hl.Item2;
                //double R2 = hl2.Item1 - hl2.Item2;

                BTOrder order = new BTOrder();
                order.ID = id++;
                order.status = BTOrderType.Pending;
                order.enterTime = curTime;
                order.enterPrice = h4Pair.Value.OpenAsk - R * a;
                order.takeProfit = order.enterPrice + R * b;
                order.stopLoss = order.enterPrice - 0.005;
                order.size = 8000;
                DateTime cur = curTime;
                while (cur <= Trader.Rfc2Date(h4Pair.Value.closeTime))
                {
                    if (!data.DataM1.ContainsKey(cur))
                    {
                        cur = cur.AddMinutes(1);
                        continue;
                    }
                    double askHigh = data.DataM1[cur].HighAsk;
                    double askLow = data.DataM1[cur].LowAsk;
                    double bidHigh = data.DataM1[cur].HighBid;
                    double bidLow = data.DataM1[cur].LowBid;
                    // Open order if target enter price is below askHigh and askLow
                    if (order.status == BTOrderType.Pending && askLow <= order.enterPrice && askHigh >= order.enterPrice)
                    {
                        order.enterTime = cur; // update enter time
                        order.status = BTOrderType.Open; // open order
                    }
                    // Play to next minute if order is not opened yet
                    if (order.status != BTOrderType.Open)
                    {
                        cur = cur.AddMinutes(1);
                        continue;
                    }
                    // stop loss if bid is too low
                    if (bidLow <= order.stopLoss)
                    {
                        order.pnl = order.size * (order.stopLoss - order.enterPrice);
                        order.closeTime = cur;
                        order.closeType = BTOrderCloseType.StopLoss;
                        order.closePrice = order.stopLoss;
                        order.status = BTOrderType.Closed;
                        break;
                    }
                    else if (bidHigh >= order.takeProfit) // take profit
                    {
                        order.pnl = order.size * (order.takeProfit - order.enterPrice);
                        order.closeTime = cur;
                        order.closeType = BTOrderCloseType.TakeProfit;
                        order.closePrice = order.takeProfit;
                        order.status = BTOrderType.Closed;
                        break;
                    }
                    cur = cur.AddMinutes(1);
                }
                if (order.status == BTOrderType.Open)
                {
                    double bidClose = h4Pair.Value.CloseBid;
                    order.pnl = bidClose - order.enterPrice;
                    order.closeTime = cur;
                    order.closeType = BTOrderCloseType.TimeOut;
                    order.closePrice = bidClose;
                    order.status = BTOrderType.Closed;
                }
                if (order.status == BTOrderType.Closed)
                {
                    if (order.pnl > 0) win++;
                    else if (order.pnl < 0) loss++;
                    PNL += order.pnl;
                    orderList.Add(order);
                    Console.WriteLine("{0}, {1}, {2}", win, loss, order.pnl.ToString("C", CultureInfo.CurrentCulture));
                }
            }
            PNL -= orderList.Count * 1.0;
            Console.WriteLine("Win: {0}\nLoss: {1}\nPNL: {2}", win, loss, PNL.ToString("C", CultureInfo.CurrentCulture));
            //File.WriteAllLines(Rest.DataRootDir + @"TestResult\res.csv", orderList.Select(i => i.ToString()));
            return PNL;
        }
        private Tuple<double, double> getAskHighLow(SortedList<DateTime, FxHistoricalDataEntry> list, DateTime d, int n)
        {
            if (!list.ContainsKey(d)) return Tuple.Create(-1.0, -1.0);
            int k = list.IndexOfKey(d);
            if (k < n) return Tuple.Create(-1.0, -1.0);
            double high = -1.0;
            double low = -1.0;
            while (n-- > 0)
            {
                k--;
                //Console.WriteLine(list.Values[k].ToString());
                if (high < list.Values[k].HighAsk)
                {
                    high = list.Values[k].HighAsk;
                }
                if (low < 0 || low > list.Values[k].LowAsk)
                {
                    low = list.Values[k].LowAsk;
                }
            }
            return Tuple.Create(high, low);
        }
    }
}
