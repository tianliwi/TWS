using System;
using System.IO;
using MathNet.Numerics;
using System.Linq;
using System.Collections.Generic;
using IBTrader;

namespace BackTest
{
    class Program
    {
        static void Main(string[] args)
        {
            DataRepo data = new DataRepo("AUD", new string[] { "2015", "2016", "2017" });
            
            LongStrategy longStra = new LongStrategy(data);
            //double pnl = longStra.Start(0.2, 0.4, true);
            ShortStrategy shortStra = new ShortStrategy(data);
            //pnl = shortStra.Start(0.2, 0.4, true);
            /*
            List<double> xdata = new List<double>();
            List<double> ydata = new List<double>();
            double xs = 0;
            foreach(double d in engine.pnlList.Values)
            {
                xdata.Add(xs++);
                ydata.Add(d);
            }
            Tuple<double, double> p = Fit.Line(xdata.ToArray(), ydata.ToArray());
            double la = p.Item1; // == 10; intercept
            double lb = p.Item2; // == 0.5; slope
            Console.WriteLine("{0} {1}", la, lb);
            Console.WriteLine(GoodnessOfFit.RSquared(xdata.Select(x => la + lb * x).ToArray(), ydata.ToArray()));
            return;
            */
            double max = 0.0;
            double ma = -1, mb = -1;
            string row = string.Empty;
            string filename = IBTrader.Constants.BaseDir + "matrix_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv";
            for (double a = 0; a < .4; a += .04)
            {
                for (double b = 0; b < .8; b += .04)
                {
                    double r = longStra.Start(a, b, true);
                    //r += shortStra.Start(a, b, true);
                    if (r > max)
                    {
                        max = r;
                        ma = a;
                        mb = b;
                    }

                    List<double> xdata = new List<double>();
                    List<double> ydata = new List<double>();
                    for (int k = 0; k < longStra.pnlList.Count; k++)
                    {
                        xdata.Add((double)k);
                        double rev = longStra.pnlList.Values.ElementAt(k);
                        //rev += shortStra.pnlList.Values.ElementAt(k);
                        ydata.Add(rev);

                    }

                    Tuple<double, double> p = Fit.Line(xdata.ToArray(), ydata.ToArray());
                    double la = p.Item1; // == 10; intercept
                    double lb = p.Item2; // == 0.5; slope
                    //Console.WriteLine("{0} {1}", la, lb);
                    double R2 = GoodnessOfFit.RSquared(xdata.Select(x => la + lb * x).ToArray(), ydata.ToArray());
                    if (true)//R2 > 0.7 && r > 0)
                    {
                        Console.WriteLine("{0}, {1}: {2}, {3}", a, b, r.ToString(), R2.ToString());
                    }
                    if (string.IsNullOrEmpty(row)) row = R2.ToString();
                    else row = row + "," + Math.Round(R2, 2).ToString();
                }
                File.AppendAllText(filename, row + "\n");
                row = string.Empty;
            }
            Console.WriteLine("\nMax revenue: {0}, {1}: {2}", ma, mb, max);
            //Console.WriteLine(pnl);
            OldEngine oldEngine = new OldEngine();
            oldEngine.Start();
            
        }
    }
}
