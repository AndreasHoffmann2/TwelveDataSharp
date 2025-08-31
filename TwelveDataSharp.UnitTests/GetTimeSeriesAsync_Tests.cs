using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using OpenFigiClient;
using RichardSzalay.MockHttp;
using TwelveDataSharp.Library.ResponseModels;
using Xunit;

namespace TwelveDataSharp.UnitTests {
	public class GetTimeSeriesAsync_Tests {
		public const string ApiKey = "99e6ab60ef924e9cb3dcc8b8eb279996";
		public const string figiApiKey = "d04548fb-b55c-4047-82ca-19969226fda8";

		[Fact]
		public async Task TestFigi() {
			using (var p = new MappingProvider(figiApiKey)) {
				var r = new MappingRequest();
				var j = new MappingJob() {
					IdType = IdType.ID_ISIN,
					Id = "DE0006916604",
					Currency = "EUR",
				};
				r.Add(j);
				var r2 = await p.RunMappingJobsAsync(r).ConfigureAwait(false);
			}
		}
[Fact]
		public async Task Foo() {
			var keyLookups = new List<MappingKey>();
			keyLookups.Add(MappingKey.CURRENCY);
			keyLookups.Add(MappingKey.EXCH_CODE);
			keyLookups.Add(MappingKey.ID_TYPE);
			keyLookups.Add(MappingKey.MARKET_SECTOR);
			keyLookups.Add(MappingKey.MIC_CODE);
			keyLookups.Add(MappingKey.SECURITY_TYPE_ONE);
			keyLookups.Add(MappingKey.SECURITY_TYPE_TWO);

			var keyValues = new Dictionary<MappingKey, List<string>>();

			using (var p = new MappingProvider()) {
				foreach (var kl in keyLookups) {
					Console.WriteLine($"Looking up {kl} values...");
					var vals = await p.LookupMappingKeyValuesAsync(kl).ConfigureAwait(false);
					Console.WriteLine($"Found {vals?.Values?.Count() ?? 0:N0} values for key {kl}");
					keyValues.Add(vals.Key, vals.Values.ToList());
				}
				var request = new MappingRequest();
				var j1 = new MappingJob() {
					IdType = IdType.ID_EXCH_SYMBOL,
					Id = "MSFT",
					SecurityTypeTwo = "Common Stock",
					ExchangeCode = "US"
				};

				request.Add(j1);

				var j2 = new MappingJob() {
					IdType = IdType.ID_EXCH_SYMBOL,
					Id = "MSFT",
					SecurityTypeTwo = "Option",
					Expiration = new Range<DateTime?>(new DateTime(2018, 11, 1), new DateTime(2019, 04, 01)),
					OptionType = OptionType.Call
				};

				request.Add(j2);

				var r1 = await p.RunMappingJobsAsync(request).ConfigureAwait(false);
				foreach (var eq in r1[0].Records.Take(5)) {
					Console.WriteLine($"Ticker: {eq.Ticker}\t\t\tFIGI:{eq.Id}\tMarket Sector: {eq.MarketSectorDescription}\tDescription: {eq.Name}");
				}
				foreach (var op in r1[1].Records.Take(5)) {
					Console.WriteLine($"Ticker: {op.Ticker}\tFIGI:{op.Id}\tMarket Sector: {op.MarketSectorDescription}\tDescription: {op.Name}");
				}
			}
		}

		[Fact]
		public async Task TestReal() {
			HttpClient client = new HttpClient();
			var c = new TwelveDataClient(ApiKey, client);
			var data = await c.GetTimeSeriesAsync("BFFAF", "1day");
		}
		[Fact]
		public async Task GetTimeSeriesAsync_Success_Test() {
			// Arrange
			var mockHttp = new MockHttpMessageHandler();
			mockHttp
				.When("https://api.twelvedata.com/*")
				.Respond("application/json", "{\"meta\":{\"symbol\":\"AAPL\",\"interval\":\"1min\",\"currency\":\"USD\",\"exchange_timezone\":\"America/New_York\",\"exchange\":\"NASDAQ\",\"type\":\"Common Stock\"},\"values\":[{\"datetime\":\"2021-03-01 15:59:00\",\"open\":\"127.63990\",\"high\":\"127.93000\",\"low\":\"127.63000\",\"close\":\"127.79000\",\"volume\":\"1691294\"}],\"status\":\"ok\"}");

			TwelveDataClient twelveDataClient = new TwelveDataClient("TEST", mockHttp.ToHttpClient());

			// Act
			var response = await twelveDataClient.GetTimeSeriesAsync("TEST");

			// Assert
			response?.ResponseStatus.Should().Be(Enums.TwelveDataClientResponseStatus.Ok);
			response?.ResponseMessage.Should().Be("RESPONSE_OK");
			response?.Values[0]?.Datetime.Should().Be(new DateTime(2021, 3, 1, 15, 59, 0));
			response?.ExchangeTimezone.Should().Be("America/New_York");
			response?.Exchange.Should().Be("NASDAQ");
			response?.Type.Should().Be("Common Stock");
			response?.Values[0]?.Open.Should().Be(127.63990);
			response?.Values[0]?.High.Should().Be(127.93000);
			response?.Values[0]?.Low.Should().Be(127.63000);
			response?.Values[0]?.Close.Should().Be(127.79000);
			response?.Values[0]?.Volume.Should().Be(1691294);
		}

		[Fact]
		public async Task GetTimeSeriesAsync_BadApiKey_Test() {
			// Arrange
			var mockHttp = new MockHttpMessageHandler();
			mockHttp
				.When("https://api.twelvedata.com/*")
				.Respond("application/json",
					"{\"code\":401,\"message\":\"**apikey** parameter is incorrect or not specified. You can get your free API Key instantly following this link: https://twelvedata.com/apikey. If you believe that everything is correct, you can email us at apikey@twelvedata.com\",\"status\":\"error\"}");

			TwelveDataClient twelveDataClient = new TwelveDataClient("TEST", mockHttp.ToHttpClient());

			// Act
			var response = await twelveDataClient.GetTimeSeriesAsync("TEST");

			// Assert
			response?.ResponseStatus.Should().Be(Enums.TwelveDataClientResponseStatus.TwelveDataApiError);
		}

		[Fact]
		public async Task GetTimeSeriesAsync_InvalidSymbol_Test() {
			// Arrange
			var mockHttp = new MockHttpMessageHandler();
			mockHttp
				.When("https://api.twelvedata.com/*")
				.Respond("application/json",
					"{\"code\":400,\"message\":\"**symbol** not found: FAKE. Please specify it correctly according to API Documentation.\",\"status\":\"error\",\"meta\":{\"symbol\":\"FAKE\",\"interval\":\"\",\"exchange\":\"\"}}");

			TwelveDataClient twelveDataClient = new TwelveDataClient("TEST", mockHttp.ToHttpClient());

			// Act
			var response = await twelveDataClient.GetTimeSeriesAsync("TEST");

			// Assert
			response?.ResponseStatus.Should().Be(Enums.TwelveDataClientResponseStatus.TwelveDataApiError);
		}

		[Fact]
		public async Task GetTimeSeriesAsync_NullHttpClient_Test() {
			// Arrange
			TwelveDataClient twelveDataClient = new TwelveDataClient("TEST", null);

			// Act
			var response = await twelveDataClient.GetTimeSeriesAsync("TEST");

			// Assert
			response?.ResponseStatus.Should().Be(Enums.TwelveDataClientResponseStatus.TwelveDataSharpError);
		}
	}
}