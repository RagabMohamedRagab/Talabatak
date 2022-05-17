using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;

namespace Talabatak.Helpers
{
    public class LCTBulkSMS
    {
       private const string BaseUrl = "http://ivorycell.info/";
       private const string User = "TALABATAK";
       private const string Pwd = "TALABATAK@123";
        private const string Sender = "TALABATAK";
       public static bool SendActivationcode(string PhoneNumber , string ActicationCode)
        {
            using(var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                string Body = $"Your activation code is {ActicationCode}";
                var Response = client.GetAsync($"webacc.aspx?user={User}&pwd={Pwd}&smstext={Body}&Sender={Sender}&Nums=249{PhoneNumber}");
                Response.Wait();
                var Result = Response.Result;
                if (Result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;

            }
        }
        public static bool ChangePassword(string PhoneNumber, string NewPassword)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);
                string Body = $"Your new password is {NewPassword}";
                var Response = client.GetAsync($"webacc.aspx?user={User}&pwd={Pwd}&smstext={Body}&Sender={Sender}&Nums=249{PhoneNumber}");
                Response.Wait();
                var Result = Response.Result;
                if (Result.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;

            }
        }

    }
}