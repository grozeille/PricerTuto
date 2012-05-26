using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Threading;

namespace PricerTuto.Client
{
    public partial class FormMain : Form
    {
        private static readonly string DateFormat = "MMM d, yyyy";

        private IMarketDataService MarketDataService = new GoogleMarketDataService();

        private MaturityService maturityService = new MaturityService();

        private Pricer Pricer = new Pricer();

        public FormMain()
        {
            InitializeComponent();
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            this.comboBoxOptionType.Text = "Call";

            List<DateTime> maturities = new List<DateTime>();
            
            // add all next maturities of the current year
            DateTime mat1 = maturityService.GetNextMaturityDay(DateTime.Now);
            maturities.Add(mat1);
            DateTime lastMaturity = mat1;
            while (lastMaturity.Year.Equals(DateTime.Now.Year))
            {
                lastMaturity = maturityService.GetNextMaturityDay(lastMaturity.AddDays(1));
                if (lastMaturity.Year.Equals(DateTime.Now.Year))
                {
                    maturities.Add(lastMaturity);
                }
            }

            // add maturity 1year and 2years
            DateTime y1 = maturityService.GetNextMaturityDay(new DateTime(DateTime.Now.Year + 1, 12, 1));
            maturities.Add(y1);
            DateTime y2 = maturityService.GetNextMaturityDay(new DateTime(DateTime.Now.Year + 2, 12, 1));
            maturities.Add(y2);

            this.comboBoxMaturity.Items.Clear();
            foreach (var item in maturities)
            {
                this.comboBoxMaturity.Items.Add(item.ToString(DateFormat, CultureInfo.InvariantCulture));
            }
            
            this.comboBoxMaturity.SelectedIndex = 0;
        }

        private void buttonPrice_Click(object sender, EventArgs e)
        {
            var maturityDay = DateTime.ParseExact(this.comboBoxMaturity.Text, DateFormat, CultureInfo.InvariantCulture);

            chartPrice.Series[0].Points.Clear();
            chartPrice.Series[1].Points.Clear();
            chartDelta.Series[0].Points.Clear();
            chartGamma.Series[0].Points.Clear();

            var variation = 10;

            double[] spotScenarios = new double[11];
            var spot = Convert.ToDouble(this.textBoxSpot.Text, CultureInfo.InvariantCulture);
            spotScenarios[5] = spot;
            for (int i = 1; i <= 5; i++)
            {
                spotScenarios[5 + i] = spot + ((variation * i) / 100.0) * spot;
            }
            for (int i = 1; i <= 5; i++)
            {
                spotScenarios[5 - i] = spot - ((variation * i) / 100.0) * spot;
            }

            int cptScenario = 0;
            foreach (double spotScenario in spotScenarios)
            {
                var optionScenario = Pricer.Price(
                    this.comboBoxOptionType.Text == "Call" ? OptionType.Call : OptionType.Put,
                    this.textBoxUnderlying.Text,
                    maturityDay,
                    Convert.ToDouble(this.comboBoxStrike.Text, CultureInfo.InvariantCulture),
                    spotScenario,
                    Convert.ToDouble(this.textBoxVolatility.Text, CultureInfo.InvariantCulture),
                    Convert.ToDouble(this.textBoxInterestRate.Text, CultureInfo.InvariantCulture));

                chartPrice.Series[0].Points.AddXY(spotScenario, optionScenario.Price);
                chartPrice.Series[1].Points.AddXY(spotScenario, optionScenario.PayOff);
                chartDelta.Series[0].Points.AddXY(spotScenario, optionScenario.Delta);
                chartGamma.Series[0].Points.AddXY(spotScenario, optionScenario.Gamma);
                cptScenario++;
            }

            var option = Pricer.Price(
                this.comboBoxOptionType.Text == "Call" ? OptionType.Call : OptionType.Put,
                this.textBoxUnderlying.Text,
                maturityDay,
                Convert.ToDouble(this.comboBoxStrike.Text, CultureInfo.InvariantCulture),
                Convert.ToDouble(this.textBoxSpot.Text, CultureInfo.InvariantCulture),
                Convert.ToDouble(this.textBoxVolatility.Text, CultureInfo.InvariantCulture),
                Convert.ToDouble(this.textBoxInterestRate.Text, CultureInfo.InvariantCulture));

            this.textBoxPrice.Text = Math.Round(option.Price, 4, MidpointRounding.AwayFromZero).ToString();
            this.textBoxDelta.Text = Math.Round(option.Delta, 4, MidpointRounding.AwayFromZero).ToString();
            this.textBoxGamma.Text = Math.Round(option.Gamma, 4, MidpointRounding.AwayFromZero).ToString();
        }

        private void buttonBuy_Click(object sender, EventArgs e)
        {

        }

        private void AddSpot(double spot)
        {
            this.BeginInvoke(new Action(() =>
                {
                    chartHistoricalSpot.Series[0].Points.AddXY(DateTime.Now.ToString(), spot);
                }));
        }

        private void buttonSpot_Click(object sender, EventArgs e)
        {
            var spot = MarketDataService.GetSpot(this.textBoxUnderlying.Text);

            this.textBoxSpot.Text = spot.ToString(CultureInfo.InvariantCulture);
            
            var maturityDay = DateTime.ParseExact(this.comboBoxMaturity.Text, DateFormat, CultureInfo.InvariantCulture);

            var volatility = MarketDataService.GetHistoricalVolatility(this.textBoxUnderlying.Text, DateTime.Now, (int)maturityDay.Subtract(DateTime.Now).TotalDays);

            this.textBoxVolatility.Text = volatility.ToString(CultureInfo.InvariantCulture);

            var historicalData = MarketDataService.GetHistoricalData(this.textBoxUnderlying.Text, DateTime.Now, (int)maturityDay.Subtract(DateTime.Now).TotalDays);

            chartHistoricalSpot.Series[0].Points.Clear();
            chartHistoricalVol.Series[0].Points.Clear();

            foreach (var item in historicalData)
            {
                chartHistoricalSpot.Series[0].Points.AddXY(item.Date, item.Close);
                chartHistoricalVol.Series[0].Points.AddXY(item.Date, item.Volatility);
            }

            chartHistoricalSpot.ChartAreas[0].AxisY.Minimum = historicalData.Min(x => x.Close);

            var strikes = this.MarketDataService.GetListedStrikes(this.textBoxUnderlying.Text);
            this.comboBoxStrike.Items.Clear();
            this.comboBoxStrike.Items.AddRange(strikes.Select(x => (object)x.ToString(CultureInfo.InvariantCulture)).ToArray());
        }
        
        private void buttonMarketPrice_Click(object sender, EventArgs e)
        {
            OptionType optionType = this.comboBoxOptionType.Text.Equals("Call") ? OptionType.Call : OptionType.Put;

            var marketOptions = this.MarketDataService.GetOptionData(this.textBoxUnderlying.Text, DateTime.ParseExact(this.comboBoxMaturity.Text, DateFormat, CultureInfo.InvariantCulture));
            var sameMarketOption = marketOptions
                .Where(x => x.OptionType.Equals(optionType) && x.Strike.Equals(double.Parse(this.comboBoxStrike.Text))).FirstOrDefault();
            if (sameMarketOption != null)
            {
                this.textBoxMarketPrice.Text = sameMarketOption.Price.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                this.textBoxMarketPrice.Text = "0.0";
            }
        }

        private void buttonDiff_Click(object sender, EventArgs e)
        {
            chartDiff.Series[0].Points.Clear();
            chartDiff.Series[1].Points.Clear();

            IList<DateTime> maturities = new List<DateTime>();            
            foreach (var item in this.comboBoxMaturity.Items)
            {
                maturities.Add(DateTime.ParseExact(item.ToString(), DateFormat, CultureInfo.InvariantCulture));
            }
            
            var optionType = this.comboBoxOptionType.Text == "Call" ? OptionType.Call : OptionType.Put;
            
            var spot = this.MarketDataService.GetSpot(this.textBoxUnderlying.Text);

            // find the price In The Money
            //var listedStrikes = this.MarketDataService.GetListedStrikes(this.textBoxUnderlying.Text);
            //var inTheMoney = 0.0;
            
            //int cpt = 0;

            //listedStrikes = listedStrikes.OrderBy(x => x).ToArray();

            //foreach (var item in listedStrikes)
            //{                
            //    if (item >= spot)
            //    {
            //        inTheMoney = item;
            //        break;
            //    }
            //    cpt++;
            //}

            //var previous1 = listedStrikes[cpt - 1];
            //var previous2 = listedStrikes[cpt - 2];
            //var next1 = listedStrikes[cpt + 1];
            //var next2 = listedStrikes[cpt + 2];

            //IList<double> strikes = new double[] { previous2, previous1, inTheMoney, next1, next2 };

            var strike = double.Parse(this.comboBoxStrike.Text, CultureInfo.InvariantCulture);

            foreach (var maturity in maturities)
            {
                var vol = this.MarketDataService.GetHistoricalVolatility(this.textBoxUnderlying.Text, DateTime.Now, (int)maturity.Subtract(DateTime.Now).TotalDays);

                var marketOptions = this.MarketDataService.GetOptionData(this.textBoxUnderlying.Text, maturity);

                //foreach (var strike in strikes)
                //{
                    var option = this.Pricer.Price(optionType, this.textBoxUnderlying.Text, maturity, strike, spot, vol, Double.Parse(this.textBoxInterestRate.Text, CultureInfo.InvariantCulture));
                    var marketOption = marketOptions.Where(x => x.OptionType.Equals(optionType) && x.Strike.Equals(strike)).FirstOrDefault();

                    chartDiff.Series[0].Points.AddXY(maturity, option.Price);
                    if (marketOption != null)
                    {
                        chartDiff.Series[1].Points.AddXY(maturity, marketOption.Price);
                    }
                    else
                    {
                        chartDiff.Series[1].Points.AddXY(maturity, 0.0);
                    }
                //}
            }
        }

        private void buttonInTheMoney_Click(object sender, EventArgs e)
        {
            var spot = double.Parse(this.textBoxSpot.Text, CultureInfo.InvariantCulture);
            foreach (var item in this.comboBoxStrike.Items)
            {
                double value = double.Parse(item.ToString(), CultureInfo.InvariantCulture);
                if (value >= spot)
                {
                    this.comboBoxStrike.Text = value.ToString(CultureInfo.InvariantCulture);
                    break;
                }
            }
        }
    }
}
