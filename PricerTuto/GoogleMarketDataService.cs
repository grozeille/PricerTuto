using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Spring.Rest.Client;
using System.Globalization;
using CsvHelper;
using System.IO;
using CsvHelper.Configuration;
using Newtonsoft.Json;

namespace PricerTuto
{
    public class GoogleMarketDataService : IMarketDataService
    {
        private RestTemplate restTemplate;

        public GoogleMarketDataService()
        {
            restTemplate = new RestTemplate("http://finance.google.com");
        }

        public double GetSpot(string underlyingName)
        {
            var result = restTemplate.GetForMessage<string>("/finance/info?client=ig&q={code}&infotype=infoquoteall", underlyingName);
            var text = result.Body;
            var indexOfComment = text.IndexOf("//");
            text = text.Substring(indexOfComment + 2);
            var googleResult = JsonConvert.DeserializeObject<GoogleQuote[]>(text);
            return googleResult[0].l;
        }

        public double GetHistoricalVolatility(string underlyingName, DateTime dateTime, int days)
        {
            var historicalVolatilitySet = this.GetHistoricalData(underlyingName, dateTime, days);

            var statistics = new MathNet.Numerics.Statistics.DescriptiveStatistics(historicalVolatilitySet.Skip(1).Select(x => x.Volatility));
            
            // 252: open market days in a year
            return statistics.StandardDeviation * Math.Sqrt(252);
        }

        public IList<HistoricalData> GetHistoricalData(string underlyingName, DateTime dateTime, int days)
        {
            DateTime firstDate = dateTime.AddDays(-days);

            string startDate = firstDate.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);
            string endDate = dateTime.ToString("MMM d, yyyy", CultureInfo.InvariantCulture);

            // get the Quote history for the X Months/Years ago (depending of the Maturity)
            var result = restTemplate.GetForMessage<string>("/finance/historical?q={code}&startdate={startdate}&enddate={enddate}&output=csv", underlyingName, startDate, endDate);
            var csvReader = new CsvReader(new CsvParser(new StringReader(result.Body), new CsvConfiguration { Delimiter = ',', HasHeaderRecord = true }));

            var historicalVolatilitySet = new List<HistoricalData>();

            HistoricalData previousItem = null;

            // read the result, compute the volatility for each days (except the first one)
            while (csvReader.Read())
            {
                var item = new HistoricalData();
                item.Date = DateTime.ParseExact(csvReader.GetField("Date"), "d-MMM-yy", CultureInfo.InvariantCulture);
                item.Close = double.Parse(csvReader.GetField("Close"), CultureInfo.InvariantCulture);

                historicalVolatilitySet.Add(item);

                if (previousItem != null)
                {
                    item.Volatility = Math.Log(item.Close / previousItem.Close);
                }
                previousItem = item;
            }

            return historicalVolatilitySet;
        }

        public IList<MarketOption> GetOptionData(string underlying, DateTime maturity)
        {
            // the product expires at the end of the day, but google assume that it expires the next day
            var googleMaturity = maturity.AddDays(1);

            var result = restTemplate.GetForMessage<string>("finance/option_chain?q={code}&expd={day}&expm={month}&expy={year}&output=json", underlying, googleMaturity.Day, googleMaturity.Month, googleMaturity.Year);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new GoogleDoubleJsonConvert());
            
            var optionDataSet = JsonConvert.DeserializeObject<GoogleOptionData>(result.Body, settings);

            var finalResult = new List<MarketOption>();

            if (optionDataSet.calls != null)
            {
                foreach (var item in optionDataSet.calls)
                {
                    var itemResult = new MarketOption
                    {
                        Price = item.p,
                        OptionType = OptionType.Call,
                        UnderlyingName = underlying,
                        Maturity = maturity,
                        Spot = optionDataSet.underlying_price,
                        Strike = item.strike,
                        Ask = item.a,
                        Bid = item.b
                    };
                    finalResult.Add(itemResult);
                }
            }

            return finalResult;
        }

        public IList<DateTime> GetListedMaturities(string underlying)
        {
            var result = restTemplate.GetForMessage<string>("finance/option_chain?q={code}&output=json", underlying);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new GoogleDoubleJsonConvert());
            
            var optionDataSet = JsonConvert.DeserializeObject<GoogleOptionData>(result.Body, settings);

            var finalResult = new List<DateTime>();

            foreach (var item in optionDataSet.expirations)
            {
                // the product expires at the end of the day, but google assume that it expires the next day
                finalResult.Add(new DateTime(item.y, item.m, item.d).AddDays(-1));
            }

            return finalResult;
        }

        public IList<double> GetListedStrikes(string underlying)
        {
            var result = restTemplate.GetForMessage<string>("finance/option_chain?q={code}&output=json", underlying);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new GoogleDoubleJsonConvert());

            var optionDataSet = JsonConvert.DeserializeObject<GoogleOptionData>(result.Body, settings);

            var finalResult = new List<double>();

            foreach (var item in optionDataSet.puts)
            {
                // the product expires at the end of the day, but google assume that it expires the next day
                finalResult.Add(item.strike);
            }

            return finalResult;
        }
        
        private class GoogleDoubleJsonConvert : JsonConverter
        {
            public override bool CanConvert(Type objectType)
            {
                return objectType.Equals(typeof(double));
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.Value.Equals("-"))
                {
                    return 0.0;
                }

                return double.Parse(reader.Value.ToString(), CultureInfo.InvariantCulture);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                
            }
        }

        public double GetInterestRate()
        {
            return 0.01;
        }
    }
}
