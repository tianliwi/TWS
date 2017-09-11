using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Chart
{
    public partial class HistoricalChart : Form
    {
        public HistoricalChart()
        {
            InitializeComponent();
            Load += new EventHandler(Form1_Load);
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            GetData();
        }
        private void GetData()
        {
            var filename = @"E:/GitHub/TWS/Data/EUR/2017/2017_H4.csv";
            string[] lines = File.ReadAllLines(filename);
            DataTable dt = new DataTable();
            dt.Columns.Add("ID", typeof(int));
            dt.Columns.Add("Date", typeof(DateTime));
            dt.Columns.Add("Open", typeof(double));
            dt.Columns.Add("High", typeof(double));
            dt.Columns.Add("Low", typeof(double));
            dt.Columns.Add("Close", typeof(double));
            double yMin = 2;
            double yMax = 0;
            int lineNum = 0;
            foreach (string line in lines)
            {
                string[] col = line.Split(',');
                DataRow row = dt.NewRow();
                row["ID"] = lineNum++;
                row["Date"] = DateTime.SpecifyKind(DateTime.ParseExact(col[0], "yyyyMMdd  HH:mm:ss", null), DateTimeKind.Local);
                row["Open"] = Double.Parse(col[1]);
                row["High"] = Double.Parse(col[3]);
                row["Low"] = Double.Parse(col[5]);
                row["Close"] = Double.Parse(col[7]);
                if ((double)row["Low"] < yMin) yMin = (double)row["Low"];
                if ((double)row["High"] > yMax) yMax = (double)row["High"];
                dt.Rows.Add(row);
            }
            chart.ChartAreas[0].AxisY.Minimum = yMin;
            chart.ChartAreas[0].AxisY.Maximum = yMax;
            //chart.ChartAreas[0].AxisX.Interval = 60;
            //chart.ChartAreas[0].AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Minutes;
            //chart.ChartAreas[0].AxisX.LabelStyle.Format = "yyyy-MM-dd HH:mm:ss";
            chart.ChartAreas[0].AxisX.MajorGrid.LineDashStyle = System.Windows.Forms.DataVisualization.Charting.ChartDashStyle.Dot;
            chart.DataSource = dt;
            chart.Series["M1"].XValueMember = "ID";
            chart.Series["M1"].YValueMembers = "High, Low, Open, Close";
            chart.Series["M1"].CustomProperties = "PriceDownColor=Red, PriceUpColor=Green";
            chart.Series["M1"]["ShowOpenClose"] = "Both";
            chart.DataManipulator.IsStartFromFirst = true;
            chart.DataBind();
        }
    }
}
