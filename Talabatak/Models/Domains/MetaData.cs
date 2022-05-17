using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Domains
{
    public class MetaData : BaseModel
    {
        public string Email { get; set; }
        public string Facebook { get; set; }
        public string WhatsApp { get; set; }
        public string Twitter { get; set; }
        public string LinkedIn { get; set; }
        public string Instagram { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Fax { get; set; }
        public decimal StoreOrdersDeliveryOpenFarePrice { get;  set; }
        public double StoreOrdersDeliveryOpenFareKilometers { get; set; }
        public decimal StoreOrdersDeliveryEveryKilometerPrice { get; set; }
        public decimal OtlobAy7agaDeliveryOpenFarePrice { get; set; }
        public double OtlobAy7agaDeliveryOpenFareKilometers { get; set; }
        public decimal OtlobAy7agaDeliveryEveryKilometerPrice { get; set; }
        public decimal OtlobAy7agaDeliveryStaticExtraFees { get; set; }
        public int NumberOfAvailableOrdersPerDriver { get; set; }
        public double FineForLate { get; set; }
        public int NumberOfMinutesBeforeCancellingOrderWithoutDriver { get; set; }
        public decimal WalletMinimumAmountToCharge { get; set; }
        public decimal WalletMaximumAmountToCharge { get; set; }
    }
}