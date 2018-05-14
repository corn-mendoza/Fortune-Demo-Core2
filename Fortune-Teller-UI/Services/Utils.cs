using FortuneTeller.Models;
using Pivotal.Helper;
using Steeltoe.Common.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FortuneTeller.Web.Services
{
    public class Utils
    {
        private string _sentimentUrl = "https://eastus2.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";
        private string _cognitiveKey;
        private HttpClient _httpClient;

        public Utils(string cognitiveServicesKey, HttpClient httpClient)
        {
            _cognitiveKey = cognitiveServicesKey;
            _httpClient = httpClient;
        }

        /// <summary>
        /// Analyze text and provide sentiment analysis and color values for each string
        /// </summary>
        /// <param name="texts">Strings to analyze</param>
        /// <returns>Sentiment analysis and color values with original strings</returns>
        internal async Task<List<ColorChangeResponse>> GetColorAndSentimentFromText(IEnumerable<string> texts)
        {
            return GetColorFromTextAndSentiment(await BulkSentiment(texts));
        }

        internal List<ColorChangeResponse> GetColorFromTextAndSentiment(List<ScoredText> texts)
        {
            var toReturn = new List<ColorChangeResponse>();
            foreach (var r in texts)
            {
                toReturn.Add(new ColorChangeResponse(r, HexColorFromDouble(r.Sentiment)));
            }

            return toReturn;
        }

        /// <summary>
        /// Call Azure Cognitive Services' Sentiment Analysis Api
        /// </summary>
        /// <param name="texts">List of strings to score</param>
        /// <returns>List of strings with their sentiment scores</returns>
        internal async Task<List<ScoredText>> BulkSentiment(IEnumerable<string> texts)
        {
            var toReturn = new List<ScoredText>();

            var i = 1;
            var docList = string.Empty;

            foreach (var t in texts)
            {
                // scrub user content so the api call won't fail
                var cleanText = t.Replace("\r", "").Replace("\n", "").Replace('"', '\'');

                // build the request body - Cognitive services requires a unique integer per item
                docList += $"{{\"language\":\"en\",\"id\":\"{i}\",\"text\":\"{cleanText}\"}},";
                i++;
            }
            docList = docList.Substring(0, docList.Length - 1);
            var message = new HttpRequestMessage
            {
                RequestUri = new Uri(_sentimentUrl),
                Method = HttpMethod.Post,
                Headers =
                {
                    { "Ocp-Apim-Subscription-Key", _cognitiveKey },
                    { "Accept", "application/json" }
                },
                Content = new StringContent($"{{\"documents\":[{docList}]}}", Encoding.UTF8, "application/json")
            };
            var sentimentHttpResponse = await _httpClient.SendAsync(message);
            SentimentResponse sentimentResponseData = await sentimentHttpResponse.Content.ReadAsJsonAsync<SentimentResponse>();
            i = 0;
            foreach (var r in sentimentResponseData.Documents)
            {
                var sentimentParsible = r.TryGetValue("score", out string scoreResponse);
                var sentiment = double.Parse(scoreResponse);
                toReturn.Add(new ScoredText { TextInput = texts.ElementAt(i), Sentiment = sentiment });
                i++;
            }

            return toReturn;
        }


        /// <summary>
        /// Convert a decimal (between 0 and 1) to an RGB value
        /// </summary>
        /// <param name="sentiment">Sentiment value</param>
        /// <returns>RGB color value</returns>
        /// <remarks>The scale is red(0) to green(1), with blue values highest at .5 and lower towards 0 or 1</remarks>
        internal string HexColorFromDouble(double sentiment)
        {
            var r = Hexicolor(2.0f * (1 - sentiment));
            var g = Hexicolor(2.0f * sentiment);
            var b = Hexicolor(0);
            return r + g + b;
        }

        /// <summary>
        /// Converts a decimal to a hex value
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Hex value</returns>
        /// <remarks>Values are altered to min 0 and max of 255 if outside those bounds</remarks>
        private string Hexicolor(double value)
        {
            if (value < 0)
            {
                value *= -1;
            }

            value *= 255;

            if (value > 255)
            {
                value = 255;
            }

            return Convert.ToInt16(value).ToString("X2");
        }
    }
}
