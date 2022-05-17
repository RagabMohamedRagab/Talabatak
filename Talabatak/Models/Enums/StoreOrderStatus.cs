using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Enums
{
    public enum StoreOrderStatus
    {
        Initialized,
        Placed,
        Preparing, // when store accept it
        Rejected, // if store or admin rejected it
        Delivering,
        Finished,
        Cancelled
    }
}