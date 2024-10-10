using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace WebScrapingEcap.models
{
    public class Category
    {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        
        [XmlIgnore]
        public string CategoryLink { get; set; }
        public List<Category> Subcategories { get; set; } = new List<Category>();

        public List<ProductInfo> Products { get; set; } = new List<ProductInfo>();

    }
}
