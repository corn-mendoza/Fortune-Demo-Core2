using System.Collections.Generic;

namespace FortuneTeller.Models
{
    public class SentimentResponse
    {
        public List<Dictionary<string, string>> Documents { get; set; }
    }
}
