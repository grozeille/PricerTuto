using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PricerTuto
{
    public interface IMarketDataService
    {
        double GetSpot(string underlyingName);

        double GetHistoricalVolatility(string underlyingName, DateTime dateTime, int days);

        IList<HistoricalData> GetHistoricalData(string underlyingName, DateTime dateTime, int days);

        IList<MarketOption> GetOptionData(string underlying, DateTime maturity);

        IList<double> GetListedStrikes(string underlying);

        IList<DateTime> GetListedMaturities(string underlying);
    }
}
