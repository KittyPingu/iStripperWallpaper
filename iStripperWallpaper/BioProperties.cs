using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace iStripperWallpaper.BLL
{
    internal class BioProperties
    {
        internal string Name="";
        internal string Bio="";

        public BioProperties(XmlNode? element)
        {            
            Name = element.GetAttribute("dir");
        }
    }
}
