using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class CityDTO
    {
        public long Id { get; set; }
        public long CountryId { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
    }
}