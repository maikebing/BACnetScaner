using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BACnetCommonLib
{
    public static class StringHelper
    {
        public static void AppendWithSplit<T>(this StringBuilder sb, T str)
        {
            sb.Append(str);
            sb.Append(Constants.Split);
        }
    }
}
