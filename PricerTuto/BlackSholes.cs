using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    /// <summary>
    /// Summary description for BlackSholes.
    /// </summary>
    public class BlackSholes
    {
        public static double Price(OptionType optionType, double spot, double strike,
            double time, double rate, double volatility)
        {
            double d1 = 0.0;
            double d2 = 0.0;
            double dBlackScholes = 0.0;

            // Call = actualSpot * N(d1) - strike * e(-rate) * N(d2)
            // Put = -actualSpot * N(-d1) + strike * e(-rate) * N(-d2)
            // d1 = (1 / (volatility * Sqrt(remainingTime)) * (Log(actualSpot / strike) + (rate + (volatility²/2) * remaintingTime)
            // d2 = d1 - volatility * Sqrt(remainingTime)

            d1 = D1(spot, strike, time, rate, volatility);
            d2 = D2(spot, strike, time, rate, volatility);
            if (optionType == OptionType.Call)
            {
                dBlackScholes = spot * CND(d1) - strike * Math.Exp(-rate * time) * CND(d2);
            }
            else if (optionType == OptionType.Put)
            {
                dBlackScholes = strike * Math.Exp(-rate * time) * CND(-d2) - spot * CND(-d1);
            }
            return dBlackScholes;
        }

        private static double D1(double spot, double strike,
            double time, double rate, double volatility)
        {
            return (Math.Log(spot / strike) + (rate + volatility * volatility / 2.0) * time) / (volatility * Math.Sqrt(time));
        }

        private static double D2(double spot, double strike,
            double time, double rate, double volatility)
        {
            return D1(spot, strike, time, rate, volatility) - volatility * Math.Sqrt(time);
        }

        public static double Delta(OptionType optionType, double spot, double strike,
            double time, double rate, double volatility)
        {
            if (optionType == OptionType.Call)
            {
                return CND(D1(spot, strike, time, rate, volatility));
            }
            else //if (optionType == OptionType.Put)
            {
                return -CND(-D1(spot, strike, time, rate, volatility));
            }
        }

        public static double Gamma(OptionType optionType, double spot, double strike,
            double time, double rate, double volatility)
        {
            return F(D1(spot, strike, time, rate, volatility)) / (spot * volatility * Math.Sqrt(time));
        }

        public static double F(double x)
        {

            return Math.Exp(-x * x * 0.5) / Math.Sqrt(2 * Convert.ToDouble(Math.PI.ToString()));
        }

        public static double CND(double X)
        {
            double L = 0.0;
            double K = 0.0;
            double dCND = 0.0;
            const double a1 = 0.31938153;
            const double a2 = -0.356563782;
            const double a3 = 1.781477937;
            const double a4 = -1.821255978;
            const double a5 = 1.330274429;
            L = Math.Abs(X);
            K = 1.0 / (1.0 + 0.2316419 * L);
            dCND = 1.0 - 1.0 / Math.Sqrt(2 * Convert.ToDouble(Math.PI.ToString())) *
                Math.Exp(-L * L / 2.0) * (a1 * K + a2 * K * K + a3 * Math.Pow(K, 3.0) +
                a4 * Math.Pow(K, 4.0) + a5 * Math.Pow(K, 5.0));

            if (X < 0)
            {
                return 1.0 - dCND;
            }
            else
            {
                return dCND;
            }
        }
    }
}
