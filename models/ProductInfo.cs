using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebScrapingEcap.models
{
    public  class ProductInfo
    {
        public string ProductName { get; set; }

        public string Category { get; set; }
        public string SubCategory { get; set; } 
        [XmlIgnore]
        public string ProductLink { get; set; }


        public List<Variants> Variante { get; set; } = new List<Variants>();


        public SerializableDictionary<string, object> DynamicAttributes { get; set; } = new SerializableDictionary<string, object>();
    }
}
