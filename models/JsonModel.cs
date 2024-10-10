using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebScrapingEcap.models
{
    internal class JsonModel
    {
        public string ProductName { get; set; }

        public string ProductAvailability { get; set; }

        public string ProductPrice { get; set; }

        public string ProductModel { get; set; }

        public string ProductDescription { get; set; }
        public List<Variants> ColorList { get; set; } = new List<Variants>();

        public List<string> PhotosList { get; set; } = new List<string>();

    }
}
