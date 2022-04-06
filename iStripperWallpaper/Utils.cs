using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iStripperWallpaper
{
    internal static class Utils
    {
        public static string GetAttribute(this XmlNode? e, string AttributeName)
        {
            if (e == null) return "";
            if (e.Attributes == null) return "";
            if (string.IsNullOrEmpty(AttributeName)) return "";
            var attribute = e.Attributes[AttributeName];
            string r = attribute != null ? attribute.Value : "";
            return r;
        }

    }
}
