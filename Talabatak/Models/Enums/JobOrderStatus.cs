using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Enums
{
    public enum JobOrderStatus
    {
        Placed,
        AcceptedByWorker,
        RejectedByWorker,
        CancelledByAdmin,
        CancelledByUser,
        Finished
    }
}