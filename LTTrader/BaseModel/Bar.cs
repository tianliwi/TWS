using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseModel
{
    enum BarType
    {
        Min1 = 1,
        Min5 = 2,
        Min30 = 3,
        Day = 4,
        Week = 5
    }
    class Bar
    {
        public BarType Type { get; set; }
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public long Volume { get; set; }

        public Bar()
        {
            Type = BarType.Min1;
            Open = 0;
            High = 0;
            Low = 0;
            Close = 0;
        }

        public Bar(BarType type, string symbol, DateTime date, decimal o, decimal h, decimal l, decimal c, long v)
        {
            Type = type;
            Symbol = symbol;
            Date = date;
            Open = o;
            High = h;
            Low = l;
            Close = c;
            Volume = v;
        }
    }
}
