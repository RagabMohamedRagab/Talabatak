using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class CountryDTO
    {
        public long Id { get; set; }
        public string NameAr { get; set; }
        public string NameEn { get; set; }
        public string Flag { get; set; }
        public List<CityDTO> Areas { get; set; } = new List<CityDTO>();
        public static List<CountryDTO> toListOfCountryDTOs(List<Country> countries)
        {
            var CountriesDTO = new List<CountryDTO>();
            foreach (var country in countries)
            {
                var CountryDTO = new CountryDTO()
                {
                    Flag = !string.IsNullOrEmpty(country.ImageUrl) ? MediaControl.GetPath(FilePath.Country) + country.ImageUrl : null,
                    Id = country.Id,
                    NameAr = country.NameAr,
                    NameEn = country.NameEn
                };

                if (country.Cities != null && country.Cities.Count > 0)
                {
                    foreach (var city in country.Cities)
                    {
                        CountryDTO.Areas.Add(new CityDTO()
                        {
                            CountryId = country.Id,
                            Id = city.Id,
                            NameAr = city.NameAr,
                            NameEn = city.NameEn
                        });
                    }
                }
                CountriesDTO.Add(CountryDTO);
            }
            return CountriesDTO;
        }
    }
}