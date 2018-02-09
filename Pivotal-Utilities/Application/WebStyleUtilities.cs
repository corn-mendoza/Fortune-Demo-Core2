using System;
using System.Collections.Generic;
using System.Text;

namespace Pivotal.Utilities
{
    public class WebStyleUtilities
    {
        static public string GetColorFromString(string inString, string defaultColor)
        {
            string ret = defaultColor;

            if (inString.Contains("blue"))
                ret = "blue";

            if (inString.Contains("purple"))
                ret = "purple";

            if (inString.Contains("pink"))
                ret = "pink";

            if (inString.Contains("yellow"))
                ret = "yellow";

            if (inString.Contains("green"))
                ret= "green";

            if (inString.Contains("red"))
                ret = "red";
            return ret;
        }
    }
}
