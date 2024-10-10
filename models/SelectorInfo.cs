using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapingEcap.models
{
    public enum SelectorType
    {
        Image,
        Text,
        Link,
        Attribute

    }
    public class SelectorInfo
    {
        public string ID { get; set; }
        public string Selector { get; set; }
        public SelectorType Type { get; set; }

        public bool IsList { get; set; }



     
    }
}
