using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;

namespace Talabatak.Models.DTOs
{
    public class CategoryDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
       
    }
}