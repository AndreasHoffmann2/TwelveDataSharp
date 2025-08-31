using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TwelveDataSharp.Library.ResponseModels;

namespace TwelveDataSharp.Interfaces
{
    public interface ITwelveDataClient
    {
        Task<TwelveDataQuote> GetQuoteAsync(string symbol, string interval = "1min");
        Task<TwelveDataPrice> GetRealTimePriceAsync(string symbol);
        Task<TwelveDataTimeSeries> GetTimeSeriesAsync(string symbol, 
            string interval = "1min",             
            string exchange = null,
            string micCode = null,
            string country = null,
            string type = null,
            int outputsize = 30,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
        Task<TwelveDataTimeSeriesAverage> GetTimeSeriesAverageAsync(string symbol, string interval = "1min");
        Task<TwelveDataAdx> GetAdxValuesAsync(string symbol, string interval = "1min");
        Task<TwelveDataBollingerBands> GetBollingerBandsAsync(string symbol, string interval = "1min");
    }
}
