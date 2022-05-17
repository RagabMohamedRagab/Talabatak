using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class BasketDetailsDTO
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
        public string SubTotal { get; set; }
        public string DeliveryFees { get; set; }
        public string Total { get; set; }
        public bool CustomerHasAddress { get; set; }
        public UserAddressDTO UserAddress { get; set; }
        public decimal UserWallet { get; set; }
        public List<BasketItemDTO> BasketItems { get; set; } = new List<BasketItemDTO>();
    }
}