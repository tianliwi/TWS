using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using IBTrader;
using BackTest;

namespace DrawPNL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            double k1 = 0.12;
            double k2 = 0.2;
            DataRepo data = new DataRepo("AUD", new string[] { "2016"});
            //ShortStrategy shortStra = new ShortStrategy(data);
            //double pnlShort = shortStra.Start(k1, k2, true);

            LongStrategy longStra = new LongStrategy(data);
            double pnlLong = longStra.Start(k1, k2, true);

            var series = chart1.Series.ElementAt(0);
            series.ChartType = SeriesChartType.Line;
            List<int> x = new List<int>();
            List<double> y = new List<double>();
            for(int k = 0; k < longStra.pnlList.Count; k++)
            {
                x.Add(k);
                y.Add(longStra.pnlList.Values.ElementAt(k));

            }

            File.WriteAllLines(IBTrader.Constants.BaseDir + "orderList.csv", longStra.closedOrders.Select(i => i.ToString()));
            series.Points.DataBindXY(x, y);
        }
    }
}
