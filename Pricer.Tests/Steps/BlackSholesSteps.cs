using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PricerTuto;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Pricer.Tests.Steps
{
    [Binding]
    public class BlackSholesSteps
    {
        private class OptionData
        {
            public OptionType OptionType { get; set; }

            public double Spot { get; set; }

            public double Strike { get; set; }

            public double Time { get; set; }

            public double Rate { get; set; }

            public double Volatility { get; set; }
        }

        private OptionData input;

        private double price;

        [Given(@"the following option:")]
        public void GivenTheFollowingOption(Table table)
        {
            this.input = new OptionData
            {
                OptionType = (OptionType)Enum.Parse(typeof(OptionType), table.Rows[0]["type"]),
                Spot = Double.Parse(table.Rows[0]["spot"], CultureInfo.InvariantCulture),
                Strike = Double.Parse(table.Rows[0]["strike"], CultureInfo.InvariantCulture),
                Rate = Double.Parse(table.Rows[0]["rate"], CultureInfo.InvariantCulture),
                Volatility = Double.Parse(table.Rows[0]["volatility"], CultureInfo.InvariantCulture)
            };

            string maturityString = table.Rows[0]["maturity"];
            var maturityRegex = new Regex("([0-9]*) ([A-Za-z]*)");
            var regexMatch = maturityRegex.Match(maturityString);
            if (!regexMatch.Success)
            {
                throw new ArgumentException("Invalid maturity");
            }

            var maturityType = regexMatch.Groups[2].Value;
            var maturityOffset = int.Parse(regexMatch.Groups[1].Value);

            var maturityTime = 0.0;

            var now = new DateTime(2012, 1, 1);

            if (maturityType.Equals("months", StringComparison.OrdinalIgnoreCase) || maturityType.Equals("month", StringComparison.OrdinalIgnoreCase))
            {
                maturityTime = now.AddMonths(maturityOffset).Subtract(now).TotalDays / 260.0;
            }
            else if (maturityType.Equals("years", StringComparison.OrdinalIgnoreCase) || maturityType.Equals("year", StringComparison.OrdinalIgnoreCase))
            {
                maturityTime = now.AddYears(maturityOffset).Subtract(now).TotalDays / 260.0;
            }
            else
            {
                throw new ArgumentException("Invalid maturity");
            }

            this.input.Time = maturityTime;
        }

        [When(@"I compute the price")]
        public void WhenIComputeThePrice()
        {
            this.price = BlackSholes.Price(this.input.OptionType, this.input.Spot, this.input.Strike, this.input.Time, this.input.Rate, this.input.Volatility);
        }

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(double expected)
        {
            Assert.AreEqual(expected, this.price, 0.01);
        }

    }
}
