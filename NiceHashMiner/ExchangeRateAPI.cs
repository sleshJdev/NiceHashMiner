using Newtonsoft.Json;
using NiceHashMiner.Configs;
using NiceHashMiner.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace NiceHashMiner
{
    class ExchangeRateAPI
    {
        public class Result
        {
            public Object algorithms { get; set; }
            public Object servers { get; set; }
            public Object idealratios { get; set; }
            public List<Dictionary<string, string>> exchanges { get; set; }
            public Dictionary<string, double> exchanges_fiat { get; set; }
        }

        public class ExchangeRateJSON
        {
            public Result result { get; set; }
            public string method { get; set; }
        }

        const string apiUrl = "https://api.nicehash.com/api?method=nicehash.service.info";

        private static Dictionary<string, double> exchanges_fiat = null;
        private static double USD_BTC_rate = -1;
        public static string ActiveDisplayCurrency = "USD";

        private static bool ConverterActive
        {
            get { return ConfigManager.GeneralConfig.DisplayCurrency != "USD"; }
        }


        public static double ConvertToActiveCurrency(double amount)
        {
            if (!ConverterActive)
            {
                return amount;
            }

            // if we are still null after an update something went wrong. just use USD hopefully itll update next tick
            if (exchanges_fiat == null || ActiveDisplayCurrency == "USD")
            {
                Helpers.ConsolePrint("CurrencyConverter", "Unable to retrieve update, Falling back to USD");
                return amount;
            }

            //Helpers.ConsolePrint("CurrencyConverter", "Current Currency: " + ConfigManager.Instance.GeneralConfig.DisplayCurrency);
            double usdExchangeRate = 1.0;
            if (exchanges_fiat.TryGetValue(ActiveDisplayCurrency, out usdExchangeRate))
                return amount * usdExchangeRate;
            else
            {
                Helpers.ConsolePrint("CurrencyConverter", "Unknown Currency Tag: " + ActiveDisplayCurrency + " falling back to USD rates");
                ActiveDisplayCurrency = "USD";
                return amount;
            }
        }

        public static double GetUSDExchangeRate()
        {
            if (USD_BTC_rate > 0)
            {
                return USD_BTC_rate;
            }
            return 0.0;
        }

        public static void UpdateAPI(string worker)
        {
            string resp = NiceHashStats.GetNiceHashAPIData(apiUrl, worker);
            if (resp != null)
            {
                try
                {
                    var LastResponse = JsonConvert.DeserializeObject<ExchangeRateJSON>(resp, Globals.JsonSettings);
                    // set that we have a response
                    if (LastResponse != null)
                    {
                        Result last_result = LastResponse.result;
                        ActiveDisplayCurrency = ConfigManager.GeneralConfig.DisplayCurrency;
                        exchanges_fiat = last_result.exchanges_fiat;
                        // ActiveDisplayCurrency = "USD";
                        // check if currency avaliable and fill currency list
                        foreach (var pair in last_result.exchanges)
                        {
                            if (pair.ContainsKey("USD") && pair.ContainsKey("coin") && pair["coin"] == "BTC" && pair["USD"] != null)
                            {
                                USD_BTC_rate = Helpers.ParseDouble(pair["USD"]);
                                break;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Helpers.ConsolePrint("ExchangeRateAPI", "UpdateAPI got Exception: " + e.Message);
                }
            }
            else
            {
                Helpers.ConsolePrint("ExchangeRateAPI", "UpdateAPI got NULL");
            }
        }

        static string host = "http://localhost:8080";
        public static AuthDetails Login(string username, string password)
        {
            HttpWebRequest request = WebRequest.Create(host + "/api/sign-in") as HttpWebRequest;            
            request.ContentType = "application/json";
            request.Method = "POST";
            using (StreamWriter upstream = new StreamWriter(request.GetRequestStream()))
            {
                upstream.WriteLine(JsonConvert.SerializeObject(new { username = username, password = password }));
            }
            return MakeRequest<AuthDetails>(request);
        }

        public static R MakeRequest<R>(HttpWebRequest request)
        {
            using (WebResponse response = request.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                using (StreamReader reader = new StreamReader(responseStream))
                {
                    string content = reader.ReadToEnd();
                    return JsonConvert.DeserializeObject<R>(content);
                }
            }
        }

        public static R MakePost<R>(string url, object data)
        {
            HttpWebRequest request = CreateRequest(url);
            request.ContentType = "application/json";
            request.Method = "POST";
            using (StreamWriter upstream = new StreamWriter(request.GetRequestStream()))
            {
                upstream.WriteLine(JsonConvert.SerializeObject(data));
            }
            return MakeRequest<R>(request);
        }

        public static R MakeGet<R>(string url)
        {
            HttpWebRequest request = CreateRequest(url);
            return MakeRequest<R>(request);
        }

        private static HttpWebRequest CreateRequest(string url)
        {
            Token token = ConfigManager.GeneralConfig.AuthDetails.Token;
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers[token.Name] = token.Value;
            return request;
        }


        public static MinerSettings FetchMinerSettings()
        {
            return MakeGet<MinerSettings>(host + "/api/wallet");
        }

        public static void SaveAlgorithmProfit(string algorithmName, double profit, double interval)
        {
            MakePost<string>(host + "/api/profit", new {
                miningInterval = interval,
                algorithmType = algorithmName,
                profit = profit
            });
        }
    }
}
