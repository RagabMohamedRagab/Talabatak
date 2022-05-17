using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.Enums
{
    public enum SMSErrors
    {
        Success = 1901,
        InvalidUrl = 1902,
        WaitForAwhilte = 9999,
        InvalidUsernameOrPassword = 1903,
        InvalidSenderId = 1904,
        InvalidMobile = 1905,
        InsufficientCredit = 1906,
        ServerUnderUpdating = 1907,
        InvalidDate = 1908,
        ErrorInMessage = 1909,
        MobileIsNull = 8001,
        MessageIsNull = 8002,
        LanguageIsNull = 8003,
        SenderIsNull = 8004,
        UsernameIsNull = 8005,
        PasswordIsNull = 8006
    }
}