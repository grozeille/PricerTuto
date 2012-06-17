using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class Pricer
    {
        public IDateService DateService { get; set; }

        public IMarketDataService MarketDataService { get; set; }

        public Pricer()
        {
            this.DateService = new DefaultDateService();
        }
        
        public Option Price(OptionType optionType, string underlyingName, DateTime maturity, double strike, double spot, double volatility, double interestRate)
        {
            Option option = new Option();
            option.OptionType = optionType;
            option.UnderlyingName = underlyingName;
            option.Spot = spot;
            option.Maturity = maturity;
            option.Strike = strike;

            if (optionType == PricerTuto.OptionType.Call)
            {
                option.PayOff = Math.Max(spot - strike, 0);
            }
            else //if (optionType == PricerTuto.OptionType.Put)
            {
                option.PayOff = Math.Max(strike - spot, 0);
            }


            option.Volatility = volatility;
            option.InterestRate = interestRate;
            var timeToExpirationYears = maturity.Subtract(this.DateService.Now).TotalDays / 260.0;
            option.Price = BlackSholes.Price(
                optionType, 
                option.Spot, 
                option.Strike, 
                timeToExpirationYears, 
                option.InterestRate, 
                option.Volatility);
            option.Delta = BlackSholes.Delta(
                optionType,
                option.Spot,
                option.Strike,
                timeToExpirationYears,
                option.InterestRate,
                option.Volatility);
            option.Gamma = BlackSholes.Gamma(
                optionType,
                option.Spot,
                option.Strike,
                timeToExpirationYears,
                option.InterestRate,
                option.Volatility);

            return option;
        }

        public Option PriceWithMarketData(OptionType optionType, string underlyingName, DateTime maturity, double strike)
        {
            double spot = this.MarketDataService.GetSpot(underlyingName);
            double volatility = this.MarketDataService.GetHistoricalVolatility(underlyingName, this.DateService.Now, (int)maturity.Subtract(this.DateService.Now).TotalDays);
            double interestRate = this.MarketDataService.GetInterestRate();

            return this.Price(optionType, underlyingName, maturity, strike, spot, volatility, interestRate);
        }
    }
}
