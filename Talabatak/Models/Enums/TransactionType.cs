using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Enums
{
    public enum TransactionType
    {
        CompanyMustPayToStoreForOrder,
        StoreMustPayToCompanyTheCommission,
        RefundTheUser,
        DriverMustPayToTheCompanyTheCommission,
        CompanyMustPayToDriverForDeliveringStoreOrder,
        AddedByAdminManually,
        SubtractedByAdminManually,
        UserChargedTheWallet,
        UserPaidForStoreOrderByWallet
    }
}