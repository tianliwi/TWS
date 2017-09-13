using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBTrader;

namespace HistoricalDataAnalyzer
{
    class Program
    {
        static void Main(string[] args)
        { 
            DataRepo dataRepo = new DataRepo();
            dataRepo.loadD1();
            List<double> range = new List<double>();
            foreach(var e in dataRepo.DataD1)
            {
                range.Add(e.Value.HighAsk - e.Value.LowAsk);
            }
            double mean = range.Sum()/range.Count;

            double sd = 0;
            foreach(double d in range)
            {
                sd += (d - mean) * (d - mean);
            }
            sd = sd / range.Count;
            sd = Math.Sqrt(sd);

            List<double> autocor = new List<double>();
            for(int k=0;k<20;k++)
            {
                double sum = 0;
                for(int t=0;t<range.Count-k;t++)
                {
                    sum += (range[t] - mean) * (range[t + k] - mean);
                }
                sum = sum / (range.Count - k);
                sum = sum / (sd * sd);
                autocor.Add(sum);
            }
            foreach(double d in autocor)
            {
                Console.WriteLine(d);
            }
        }
    }
}
