using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TechTalk.SpecFlow;
using PricerTuto;
using System.Globalization;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Rhino.Mocks;

namespace PricerTuto.Tests.Steps
{
    [Binding]
    public class BlackSholesSteps
    {
        private Pricer pricer;

        private IDateService dateService;

        private OptionArgs input;

        private Option result;
        
        public BlackSholesSteps()
        {
            dateService = new FixedDateService { Now = new DateTime(2012, 1, 1) };
            pricer = new Pricer();
            pricer.DateService = dateService;
            pricer.MarketDataService = Rhino.Mocks.MockRepository.GenerateStub<IMarketDataService>();
        }

        [Given(@"the following option:")]
        public void GivenTheFollowingOption(Table table)
        {
            this.input = new OptionArgs
            {
                OptionType = (OptionType)Enum.Parse(typeof(OptionType), table.Rows[0]["type"]),
                Spot = Double.Parse(table.Rows[0]["spot"], CultureInfo.InvariantCulture),
                Strike = Double.Parse(table.Rows[0]["strike"], CultureInfo.InvariantCulture),
                Rate = Double.Parse(table.Rows[0]["rate"], CultureInfo.InvariantCulture),
                Volatility = Double.Parse(table.Rows[0]["volatility"], CultureInfo.InvariantCulture)
            };

            string maturityString = table.Rows[0]["maturity"];

            this.input.Maturity = this.ParseMaturity(maturityString);
        }

        [Given("the market rate: (.*)")]
        public void GivenTheMarketRate(double rate)
        {
            this.pricer.MarketDataService.Stub(m => m.GetInterestRate()).Return(rate);
        }

        [Given("the market spot for underlying \"(.*)\": (.*)")]
        public void GivenTheMarketSpotForUnderlying(string underlying, double spot)
        {
            this.pricer.MarketDataService.Stub(m => m.GetSpot(underlying)).Return(spot);
        }

        [Given("the market volatility for underlying \"(.*)\" since (.*): (.*)")]
        public void GivenTheMarketVolatilityForUnderlying(string underlying, DateOffset since, double volatility)
        {
            int days = 0;
            if(since.Type == DateOffsetType.MONTH)
            {
                days = (int)this.dateService.Now.AddMonths(since.Offset).Subtract(this.dateService.Now).TotalDays;
            }
            else if(since.Type == DateOffsetType.YEAR)
            {
                days = (int)this.dateService.Now.AddYears(since.Offset).Subtract(this.dateService.Now).TotalDays;
            }

            this.pricer.MarketDataService.Stub(m => m.GetHistoricalVolatility(underlying, this.dateService.Now, days)).Return(volatility);
        }

        [When(@"I compute the price")]
        public void WhenIComputeThePrice()
        {
            this.result = pricer.Price(this.input.OptionType, this.input.Underlying, this.input.Maturity, this.input.Strike, this.input.Spot, this.input.Volatility, this.input.Rate);
        }

        [When(@"I compute the price with market data the option:")]
        public void WhenIComputeThePriceWithMarketData(Table table)
        {
            var optionType = (OptionType)Enum.Parse(typeof(OptionType), table.Rows[0]["type"]);
            var maturityString = table.Rows[0]["maturity"];
            var strike = Double.Parse(table.Rows[0]["strike"], CultureInfo.InvariantCulture);
            var underlyingName = table.Rows[0]["underlying"];

            var maturity = this.ParseMaturity(maturityString);

            this.result = pricer.PriceWithMarketData(optionType, underlyingName, maturity, strike);
        }

        [Then(@"the result should be (.*)")]
        public void ThenTheResultShouldBe(double expected)
        {
            Assert.AreEqual(expected, this.result.Price, 0.01);
        }

        [StepArgumentTransformation]
        private DateOffset ParseOffset(string offsetString)
        {
            var result = new DateOffset();

            var maturityRegex = new Regex("([0-9]*) ([A-Za-z]*)");
            var regexMatch = maturityRegex.Match(offsetString);
            if (!regexMatch.Success)
            {
                throw new ArgumentException("Invalid maturity");
            }

            var typeString = regexMatch.Groups[2].Value;
            result.Offset = int.Parse(regexMatch.Groups[1].Value);

            var now = this.dateService.Now;

            if (typeString.Equals("months", StringComparison.OrdinalIgnoreCase) || typeString.Equals("month", StringComparison.OrdinalIgnoreCase))
            {
                result.Type = DateOffsetType.MONTH;
            }
            else if (typeString.Equals("years", StringComparison.OrdinalIgnoreCase) || typeString.Equals("year", StringComparison.OrdinalIgnoreCase))
            {
                result.Type = DateOffsetType.YEAR;
            }
            else
            {
                throw new ArgumentException("Invalid maturity");
            }

            return result;
        }

        private DateTime ParseMaturity(string maturityString)
        {
            var offset = ParseOffset(maturityString);

            if (offset.Type == DateOffsetType.MONTH)
            {
                return this.dateService.Now.AddMonths(offset.Offset);
            }
            else
            {
                return this.dateService.Now.AddYears(offset.Offset);
            }
        }
    }
}
