﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class StoreDetailsDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string CoverImageUrl { get; set; }
        public string LogoImageUrl { get; set; }
        public string Description { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public int SeenCount { get; set; }
        public bool IsOpen { get; set; }
        public string Rate { get; set; }
        public List<ProductCategoryDTO> Categories { get; set; } = new List<ProductCategoryDTO>();
    }
}