using Talabatak.Models.DTOs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Net;

namespace Talabatak.Helpers
{
    public static class SMS
    {
        private static string Username = "ksMWRcm2";
        private static string Password = "5s67oBaJGx";
        private static string Sender = "E-Marketing";
        private static string ApiUrl = "https://smsmisr.com/api/webapi/?";

        public static async Task<string> SendMessage(string PhoneCountryCode, string PhoneNumber, string Message)
        {
            if (PhoneNumber.StartsWith("+"))
            {
                PhoneNumber = PhoneNumber.Remove(0, 1);
            }
            if (!PhoneNumber.StartsWith("2"))
            {
                PhoneNumber = "2" + PhoneNumber;
            }

            if (!string.IsNullOrEmpty(PhoneCountryCode) && !string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(Message))
            {
                try
                {
                    var EndPoint = ApiUrl + "username=" + Username + "&password=" + Password + "&language=2" + "&message=" + Message + "&sender=" + Sender + "&mobile=" + PhoneNumber;
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyJS = JsonConvert.SerializeObject(new PaymentStatusDto());
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response =  client.PostAsync(EndPoint,body).GetAwaiter().GetResult();
                        //var x = await response.Content.ReadAsStringAsync();
                        if (response.IsSuccessStatusCode == true)
                        {
                            return "تم ارسال الرسالة بنجاح";
                        }
                        return null;
                    }
                }
                catch (Exception)
                {
                    return "برجاء المحاوله مره اخرى";
                }
            }
            return null;
        }

        public static async Task<string> SendMessageToMultipleUsers(string PhoneCountryCode, List<string> PhoneNumbers, string Message)
        {
            for (int i = 0; i < PhoneNumbers.Count; ++i)
            {

                if (PhoneNumbers[i].StartsWith("+"))
                {
                    PhoneNumbers[i] = PhoneNumbers[i].Remove(0, 1);
                }
                if (!PhoneNumbers[i].StartsWith("2"))
                {
                    PhoneNumbers[i] = "2" + PhoneNumbers[i];
                }

            }
            if (!string.IsNullOrEmpty(PhoneCountryCode) && PhoneNumbers != null && PhoneNumbers.Count() > 0 && !string.IsNullOrEmpty(Message))
            {

                var Mobiles = string.Join(",", PhoneNumbers);
                var EndPoint = ApiUrl + "username=" + Username + "&password=" + Password + "&language=2" + "&message=" + Message + "&sender=" + Sender + "&mobile=" + Mobiles;


                using (HttpClient client = new HttpClient())
                {
                    try
                    {
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyJS = JsonConvert.SerializeObject(new PaymentStatusDto());
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response =  client.PostAsync(EndPoint,body).GetAwaiter().GetResult();
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            return "تم ارسال الرسالة بنجاح";
                        }
                        else
                        {
                            return "عفواً ، برجاء المحاوله لاحقاً ، فى حاله تكرر المشكلة برجاء التواصل مع التقنى.";
                        }
                    }
                    catch (Exception ex)
                    {
                        ExceptionLogging.SendErrorToText(ex);
                        return "عفواً ، برجاء المحاوله لاحقاً ، فى حاله تكرر المشكلة برجاء التواصل مع التقنى.";
                    }
                }
            }
            else
            {
                return "برجاء التأكد من صحة المدخلات";
            }
        }
    }
}