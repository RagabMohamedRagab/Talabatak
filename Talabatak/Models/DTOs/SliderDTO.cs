using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
   
    public class SliderDTO
    {
        public long ID { get; set; }

        public string Image { get; set; }

        public long  StoreId { get; set; }

        public int SortingNumber { get; set; }
    }
    public class SecondSliderDTO
    {
        public long ID { get; set; }

        public string Image { get; set; }

        public long StoreId { get; set; }

        public int SortingNumber { get; set; }
    }
    public class TwoSliderDTO
    {
        public List<SliderDTO> SliderDTOs { get; set; } = new List<SliderDTO>();
        public List<SecondSliderDTO> SecondSliderDTOs { get; set; } = new List<SecondSliderDTO>();
    }
}