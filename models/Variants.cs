using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebScrapingEcap.models;

namespace WebScrapingEcap.models
{
    public class Variants
    {
        public string Name { get; set; }
        public string Link { get; set; }
        public string Availability { get; set; }

        public string Sku { get; set; }

        public string Description { get; set; }

        public List<string> Culori { get; set; }
    }
}
