using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IBTrader;

namespace BackTest
{
    public class LongStrategy
    {
        private DataRepo data;
        List<BTOrder> pendOrders;
        List<BTOrder> openOrders;
        public List<BTOrder> closedOrders;
        public SortedList<DateTime, double> pnlList;
        private static double commission = 0;
        private static int units = 4000;

        public LongStrategy(DataRepo data)
        {
            this.data = data;
            pendOrders = new List<BTOrder>();
            openOrders = new List<BTOrder>();
            closedOrders = new List<BTOrder>();

            pnlList = new SortedList<DateTime, double>();
            foreach (var p in data.DataH4)
            {
                pnlList[p.Key] = 0.0;
            }
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        }

        public double Start(double a, double b, bool pnlTrack)
        {
            double PNL = 0;
            int win = 0;
            int loss = 0;
            int id = 0;
            openOrders.Clear();
            closedOrders.Clear();

            foreach (var h4Bin in data.DataH4)
            {
                pendOrders.Clear();
                DateTime cur = h4Bin.Key;
                // update pnl curve
                if (pnlTrack)
                {
                    double curPNL = 0;
                    foreach (var o in closedOrders)
                    {
                        curPNL += o.pnl - commission * 2.0;
                    }
                    pnlList[cur] = curPNL;
                }
                // if no open order, set limit order and pend
                if (openOrders.Count == 0)
                {
                    var hl = getAskHighLow(data.DataH4, cur, 6);
                    if (hl.Item1 < 0 || hl.Item2 < 0) continue;
                    double R = hl.Item1 - hl.Item2;

                    //var hl2 = getAskHighLow(data.DataH4, cur, 6);
                    //if (hl2.Item1 < 0 || hl2.Item2 < 0) continue;
                    //double R2 = hl2.Item1 - hl2.Item2;
                    
                    // long order
                    BTOrder longOrder = new BTOrder();
                    longOrder.ID = id++;
                    longOrder.status = BTOrderType.Pending;
                    longOrder.dir = BTOrderDir.Long;
                    longOrder.pendTime = cur;
                    longOrder.openPrice = h4Bin.Value.OpenAsk - R * a;
                    longOrder.takeProfit = longOrder.openPrice + R * b;
                    longOrder.stopLoss = longOrder.openPrice - 0.005;
                    longOrder.size = units;
                    pendOrders.Add(longOrder);
                }

                // scan within the current 4 hours
                while (cur <= Trader.Rfc2Date(h4Bin.Value.closeTime))
                {
                    // quit the current 4 hours if no pend or open orders
                    if (pendOrders.Count == 0 && openOrders.Count == 0) break;
                    if (!data.DataM1.ContainsKey(cur))
                    {
                        cur = cur.AddMinutes(1);
                        continue;
                    }
                    double askHigh = data.DataM1[cur].HighAsk;
                    double askLow = data.DataM1[cur].LowAsk;
                    double bidHigh = data.DataM1[cur].HighBid;
                    double bidLow = data.DataM1[cur].LowBid;

                    // Open order if target enter price is between askHigh and askLow
                    foreach (var curOrder in pendOrders.ToArray())
                    {
                        if (askLow <= curOrder.openPrice
                            && askHigh >= curOrder.openPrice)
                            //&& R2 >= 0.013 && R2 <= 0.019)
                        {
                            curOrder.openTime = cur; // update enter time
                            curOrder.status = BTOrderType.Open; // open order
                            // remove from pendOrders, add to openOrders
                            pendOrders.Remove(curOrder);
                            openOrders.Add(curOrder);
                        }
                    }
                    // check open orders
                    foreach (var curOrder in openOrders.ToArray())
                    {
                        // stop loss if bid is too low
                        if (bidLow <= curOrder.stopLoss && bidHigh >= curOrder.stopLoss)
                        {
                            curOrder.pnl = curOrder.size * (curOrder.stopLoss - curOrder.openPrice);
                            curOrder.closeTime = cur;
                            curOrder.closePrice = curOrder.stopLoss;
                            curOrder.status = BTOrderType.Closed;
                            // close order
                            openOrders.Remove(curOrder);
                            closedOrders.Add(curOrder);
                        }
                        // take profit
                        else if (bidHigh >= curOrder.takeProfit && bidLow <= curOrder.takeProfit)
                        {
                            curOrder.pnl = curOrder.size * (curOrder.takeProfit - curOrder.openPrice);
                            curOrder.closeTime = cur;
                            curOrder.closePrice = curOrder.takeProfit;
                            curOrder.status = BTOrderType.Closed;
                            // close order
                            openOrders.Remove(curOrder);
                            closedOrders.Add(curOrder);
                        }
                    }
                    cur = cur.AddMinutes(1);
                }
            }
            PNL = 0;
            foreach (var order in closedOrders)
            {
                if (order.pnl > 0) win++;
                else if (order.pnl < 0) loss++;
                PNL += order.pnl;
            }
            PNL -= closedOrders.Count * commission * 2;
            //Console.WriteLine("Win: {0}\nLoss: {1}\nPNL: {2}", win, loss, PNL.ToString("C", CultureInfo.CurrentCulture));
            
            double rate = (double)win / (win + loss);
            if (PNL > 100000000)
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
