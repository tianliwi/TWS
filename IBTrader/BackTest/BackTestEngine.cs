using System;
using System.Collections.Generic;
using System.Linq;
using IBApi;
using IBTrader;
using System.Threading;
using System.IO;
using System.Globalization;

namespace BackTest
{
    public class BackTestEngine
    {
        private DataRepo data;
        List<BTOrder> orderList;
        public SortedList<DateTime, double> pnlList;
        private static double commission = 2;

        public BackTestEngine()
        {
            data = new DataRepo("AUD", new string[] { "2016"});
            orderList = new List<BTOrder>();
            pnlList = new SortedList<DateTime, double>();
            foreach(var p in data.DataH4)
            {
                pnlList[p.Key] = 0.0;
            }
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        public double Start(double a, double b, bool pnlTrack)
        {
            DateTime curTime;
            double PNL = 0;
            int win = 0;
            int loss = 0;
            int id = 0;
            double R, R2=0;
            orderList.Clear();

            foreach (var h4Bin in data.DataH4)
            {
                curTime = h4Bin.Key;
                // if no order or the last order hasn't been closed
                if (orderList.Count == 0 || orderList[orderList.Count - 1].status == BTOrderType.Closed)
                {
                    var hl = getAskHighLow(data.DataH4, curTime, 12);
                    if (hl.Item1 < 0 || hl.Item2 < 0) continue;
                    R = hl.Item1 - hl.Item2;

                    var hl2 = getAskHighLow(data.DataH4, curTime, 18);
                    if (hl2.Item1 < 0 || hl2.Item2 < 0) continue;
                    R2 = hl2.Item1 - hl2.Item2;

                    BTOrder order = new BTOrder();
                    order.ID = id++;
                    order.status = BTOrderType.Pending;
                    order.enterTime = curTime;
                    order.enterPrice = h4Bin.Value.OpenAsk - R * a;
                    order.takeProfit = order.enterPrice + R * b;
                    order.stopLoss = order.enterPrice - 0.005;
                    order.size = 10000;
                    orderList.Add(order);
                }
                DateTime cur = curTime;
                BTOrder curOrder = orderList[orderList.Count - 1];
                if (pnlTrack)
                {
                    double curPNL = 0;
                    foreach (var o in orderList)
                    {
                        curPNL += o.pnl;
                        if (o.status == BTOrderType.Open)
                        {
                            curPNL += data.DataM1[cur].CloseBid - o.enterPrice;
                        }
                        curPNL -= commission * 2.0;
                    }
                    pnlList[cur] = curPNL;
                }
                while (cur <= Trader.Rfc2Date(h4Bin.Value.closeTime))
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
                    if (curOrder.status == BTOrderType.Pending && 
                        askLow <= curOrder.enterPrice && askHigh >= curOrder.enterPrice)// &&
                        //R2 >= 0.013 && R2 <= 0.019)
                    {
                        curOrder.enterTime = cur; // update enter time
                        curOrder.status = BTOrderType.Open; // open order
                        PNL -= commission;
                    }
                    // Play to next minute if order is not opened yet
                    if (curOrder.status != BTOrderType.Open)
                    {
                        cur = cur.AddMinutes(1);
                        continue;
                    }
                // stop loss if bid is too low
                if (bidLow <= curOrder.stopLoss)
                    {
                        curOrder.pnl = curOrder.size * (curOrder.stopLoss - curOrder.enterPrice);
                        curOrder.closeTime = cur;
                        curOrder.closeType = BTOrderCloseType.StopLoss;
                        curOrder.closePrice = curOrder.stopLoss;
                        curOrder.status = BTOrderType.Closed;
                        PNL -= commission;
                        continue;
                    }
                    else if (bidHigh >= curOrder.takeProfit) // take profit
                    {
                        curOrder.pnl = curOrder.size * (curOrder.takeProfit - curOrder.enterPrice);
                        curOrder.closeTime = cur;
                        curOrder.closeType = BTOrderCloseType.TakeProfit;
                        curOrder.closePrice = curOrder.takeProfit;
                        curOrder.status = BTOrderType.Closed;
                        PNL -= commission;
                        continue;
                    }
                    cur = cur.AddMinutes(1);
                }
                if(curOrder.status == BTOrderType.Pending)
                {
                    orderList.RemoveAt(orderList.Count - 1);
                }
            }
            PNL = 0;
            foreach (var order in orderList)
            {
                if (order.pnl > 0) win++;
                else if (order.pnl < 0) loss++;
                PNL += order.pnl;
                //Console.WriteLine(order.ToString());
            }
            PNL -= orderList.Count * commission * 2;
            //Console.WriteLine("Win: {0}\nLoss: {1}\nPNL: {2}", win, loss, PNL.ToString("C", CultureInfo.CurrentCulture));

            //File.WriteAllLines(IBTrader.Constants.BaseDir + "test_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv", pnlList.Select(i=>i.Value.ToString()));
            //return PNL;
            double rate = (double)win / (win + loss);
            if(PNL > 0)
            {
                Console.WriteLine("{0} {1}:{4} {2} {3}", a, b, rate, PNL, win + loss);
            }
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
