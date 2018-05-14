namespace Pivotal.Helper
{
    public class ColorChangeResponse : ScoredText
    {
        public ColorChangeResponse()
        {
        }

        public ColorChangeResponse(ScoredText scoredText, string hexColor)
        {
            TextInput = scoredText.TextInput;
            Sentiment = scoredText.Sentiment;
            HexColor = hexColor;
        }

        public string HexColor { get; set; }
    }
}
