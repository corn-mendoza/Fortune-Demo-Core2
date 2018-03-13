using System;

namespace Pivotal.Utilities
{
    public class StringCleaner
    {
        static public string GetDisplayString(string searchString, string endingString, string stringToClean, string replaceString)
        {
            string ret = stringToClean;

            try
            {
                int startLoc = stringToClean.IndexOf(searchString);
                if (startLoc >= 0)
                {
                    int pwStart = startLoc + searchString.Length;
                    int endLoc = stringToClean.IndexOf(endingString, pwStart);
                    if (endLoc < 0)
                    {
                        endLoc = stringToClean.Length - 1;
                    }
                    string pw = stringToClean.Substring(pwStart, ((endLoc) - pwStart));
                    ret = stringToClean.Replace(pw, replaceString);
                }
            }
            catch
            {

            }

            return ret;
        }
    }
}
