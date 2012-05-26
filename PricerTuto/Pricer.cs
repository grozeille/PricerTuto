using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public class Pricer
    {
        public Option Price(OptionType optionType, string underlyingName, DateTime maturity, double strike)
        {
            return Price(optionType, underlyingName, maturity, strike, this.GetSpot(underlyingName), this.GetVolatility(underlyingName), this.GetInterestRate());
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
            var timeToExpirationYears = maturity.Subtract(DateTime.Now).TotalDays / 260.0;
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

        private double GetInterestRate()
        {
            throw new NotImplementedException();
        }

        private double GetVolatility(string underlyingName)
        {
            throw new NotImplementedException();
        }

        private double GetSpot(string underlyingName)
        {
            throw new NotImplementedException();
        }
    }
}
