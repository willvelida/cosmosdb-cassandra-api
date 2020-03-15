using System;
using System.Collections.Generic;
using System.Text;

namespace CassandraCats.API.Models
{
    public class Cat
    {
        public string cat_id { get; set; }
        public string cat_name { get; set; }
        public string cat_type { get; set; }
        public int cat_age { get; set; }
    }
}
