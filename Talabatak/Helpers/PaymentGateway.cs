using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;

namespace Talabatak.Helpers
{
    public static class PaymentGateway
    {
        /*
         * Testing Card:
         *
         * Card: 7888450004340823
         *
         * Expiry Date: Any valid date in the future.
         *
         * iPIN: any four digits.
         */
        private const string Payment_TEST_Endpoint = " https://accept.paymob.com/api";
        private const string Payment_LIVE_Endpoint = "";
        private const string Payment_TEST_Status_Endpoint = "http://syberpay.test.sybertechnology.com/syberpay/payment_status";
        private const string Payment_LIVE_Status_Endpoint = "";
        private const string ApplicationID = "ZXlKaGJHY2lPaUpJVXpVeE1pSXNJblI1Y0NJNklrcFhWQ0o5LmV5SmpiR0Z6Y3lJNklrMWxjbU5vWVc1MElpd2ljSEp2Wm1sc1pWOXdheUk2TmpRMk1Td2libUZ0WlNJNkltbHVhWFJwWVd3aWZRLlJNSGdjc05rQ09TQWlKdDFkVGJlNE1kMkpidDE5NktZSHhqYkZZa2hOYmlMUzRmMHN2UjV6anVySUF4dUo2ZnNacE9heWM2Z2RQY1FHTHlXRlZLMWJB";
        private const string PayeeID = "0010040001";
        private const string ServiceID = "T@l@batak388";
        private const string ApplicationKey = "T@l@batakK3y";
        private const string ApplicationSalt = "T@l@batak$alt";
        private const string IFrameLink = "https://accept.paymob.com/api/acceptance/iframes/17760?payment_token=";
        private static string CurrentStatus = ConfigurationManager.AppSettings["PAYMENT_STATUS"];
        private static int IntegrationId = 11186;
        private static async Task<string> GetToken()
        {
            using(HttpClient client = new HttpClient())
            {
                AcceptKey key = new AcceptKey()
                {
                    api_key = ApplicationID
                };
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var bodyJs = JsonConvert.SerializeObject(key);
                var body = new StringContent(bodyJs, Encoding.UTF8, "application/json");
                var reponse = client.PostAsync(Payment_TEST_Endpoint+"/auth/tokens", body).GetAwaiter().GetResult();
                if (reponse.IsSuccessStatusCode)
                {
                    var result = await reponse.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<AcceptTokenDto>(result);
                    if (!string.IsNullOrEmpty(data.token))
                    {
                        return data.token;
                    }          
                }
                
            }
            return string.Empty;
        }
        private static async Task<int> OrderRegistration(AcceptPaymentInfoDto dto)
        {
            using (HttpClient clint = new HttpClient())
            {
                clint.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var bodyJS = JsonConvert.SerializeObject(dto);
                var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                var response = clint.PostAsync(Payment_TEST_Endpoint + "/ecommerce/orders", body).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode == true)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    var data = JsonConvert.DeserializeObject<AcceptPaymentResponseDto>(result);
                    return data.id;
                }
            }
            return 0;
        }
        public static async Task<string> GenerateOtlobAy7agaPaymentLink(OtlobAy7agaOrder order)
        {
            if (order != null)
            {
                SyberPayPaymentDTO dto = new SyberPayPaymentDTO()
                {
                    applicationId = ApplicationID,
                    payeeId = PayeeID,
                    serviceId = ServiceID,
                    amount = order.Total,
                    currency = "SDG",
                    customerRef = order.Id.ToString(),
                };
                dto.paymentInfo.Add("paymentReason", "طلبات من قسم اطلب اى حاجه");
                dto.paymentInfo.Add("subTotal", order.SubTotal.ToString());
                dto.paymentInfo.Add("deliveryFees", order.DeliveryFees.ToString());
                dto.paymentInfo.Add("total", order.Total.ToString());
                dto.paymentInfo.Add("customerName", order.UserAddress.Name == null ? order.User.Name : order.UserAddress.Name);
                dto.paymentInfo.Add("customerPhoneNumber", order.UserAddress.PhoneNumber == null ? order.User.PhoneNumber : order.UserAddress.PhoneNumber);
                string sequence = $"{ApplicationKey}|{ApplicationID}|{ServiceID}|{order.Total.ToString()}|SDG|{dto.customerRef}|{ApplicationSalt}";
                dto.hash = HashCalculations(sequence);

                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        var EndPoint = CurrentStatus.ToLower() == "sandbox" ? Payment_TEST_Endpoint : Payment_LIVE_Endpoint;
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyJS = JsonConvert.SerializeObject(dto);
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync(EndPoint, body).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode == true)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<SyberPayPaymentResponseDTO>(result);
                            if (data.responseCode == 1)
                            {
                                return data.paymentUrl;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public static async Task<string> GenerateChargeWalletPaymentLink(ApplicationUser user)
        {
            string Token = await GetToken();
            if (string.IsNullOrEmpty(Token))
                return null;
            string temp = user.AmountToBePaid.ToString();
            if (!temp.Contains("."))
            {
                temp = temp + ".0";
            }

            if (user != null)
            {
                AcceptPaymentInfoDto dto = new AcceptPaymentInfoDto()
                {
                    amount_cents = temp,
                    currency = "EGP",
                    auth_token = Token,
                    delivery_needed = "false",
                    merchant_order_id = user.PaymentUniqueKey,
                };
                var Id = await OrderRegistration(dto);
                if (Id == 0)
                    return null;

                AcceptKeyRequestDto finalDto = new AcceptKeyRequestDto()
                {
                    amount_cents = temp,
                    currency = "EGP",
                    auth_token = Token,
                    expiration = 3600,
                    order_id = Id.ToString(),
                    integration_id = IntegrationId,
                };
                finalDto.billing_data.Add("first_name", user.Name);
                finalDto.billing_data.Add("last_name", user.Name);
                finalDto.billing_data.Add("email", user.Email);
                finalDto.billing_data.Add("phone_number", "01155608997");
                finalDto.billing_data.Add("apartment", "NA");
                finalDto.billing_data.Add("floor", "NA");
                finalDto.billing_data.Add("street", "NA");
                finalDto.billing_data.Add("building", "NA");
                finalDto.billing_data.Add("shipping_method", "NA");
                finalDto.billing_data.Add("postal_code", "NA");
                finalDto.billing_data.Add("city", "NA");
                finalDto.billing_data.Add("country", "NA");
                finalDto.billing_data.Add("state", "NA");
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyJS = JsonConvert.SerializeObject(finalDto);
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync(Payment_TEST_Endpoint+ "/acceptance/payment_keys", body).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode == true)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<AcceptTokenDto>(result);
                            if (!string.IsNullOrEmpty(data.token))
                            {
                                return IFrameLink + data.token;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
        public static async Task<string> GenerateStoreOrderPaymentLink(StoreOrder order)
        {
            string Token = await GetToken();
            string temp = order.Total.ToString();
            if (!temp.Contains("."))
            {
                temp = temp + ".0";
            }
            if (order != null)
            {
                AcceptPaymentInfoDto dto = new AcceptPaymentInfoDto()
                {
                    amount_cents = temp,
                    currency = "EGP",
                    auth_token = Token,
                    delivery_needed = "false",
                    merchant_order_id = order.Id.ToString(),
                };
                var Id = await OrderRegistration(dto);
                if (Id == 0)
                    return null;
                AcceptKeyRequestDto finalDto = new AcceptKeyRequestDto()
                {
                    amount_cents = temp,
                    currency = "EGP",
                    auth_token = Token,
                    expiration = 3600,
                    order_id = Id.ToString(),
                    integration_id = IntegrationId,
                };
                finalDto.billing_data.Add("first_name", order.User.UserName);
                finalDto.billing_data.Add("last_name", order.User.UserName);
                finalDto.billing_data.Add("email", order.User.Email);
                finalDto.billing_data.Add("phone_number", "01155608997");
                finalDto.billing_data.Add("apartment", "NA");
                finalDto.billing_data.Add("floor", "NA");
                finalDto.billing_data.Add("street", "NA");
                finalDto.billing_data.Add("building", "NA");
                finalDto.billing_data.Add("shipping_method", "NA");
                finalDto.billing_data.Add("postal_code", "NA");
                finalDto.billing_data.Add("city", "NA");
                finalDto.billing_data.Add("country", "NA");
                finalDto.billing_data.Add("state", "NA");
                using (HttpClient httpClient = new HttpClient())
                {
                    try
                    {
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var bodyJS = JsonConvert.SerializeObject(finalDto);
                        var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                        var response = httpClient.PostAsync(Payment_TEST_Endpoint + "/acceptance/payment_keys", body).GetAwaiter().GetResult();
                        if (response.IsSuccessStatusCode == true)
                        {
                            var result = await response.Content.ReadAsStringAsync();
                            var data = JsonConvert.DeserializeObject<AcceptTokenDto>(result);
                            if (!string.IsNullOrEmpty(data.token))
                            {
                                return IFrameLink + data.token;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                }
            }
            return null;
        }
        public static async Task<PaymentStatusResDto> PaymentStatus(PaymentStatusDto data)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    PaymentStatusResDto paymentStatus = new PaymentStatusResDto();
                    data.applicationId = ApplicationID;
                    var EndPoint = CurrentStatus.ToLower() == "sandbox" ? Payment_TEST_Status_Endpoint : Payment_LIVE_Status_Endpoint;
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    var bodyJS = JsonConvert.SerializeObject(data);
                    var body = new StringContent(bodyJS, Encoding.UTF8, "application/json");
                    var response = httpClient.PostAsync(EndPoint, body).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode == true)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        var dataR = JsonConvert.DeserializeObject<SybnerPayStatusDto>(result);
                        if (dataR.responseCode == 1 )
                        {
                            paymentStatus.Status = dataR.responseCode;
                            paymentStatus.Amount = Convert.ToDecimal(dataR.payment.amount);
                            paymentStatus.customerRef = dataR.payment.customerRef;
                            return paymentStatus;
                        }
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            return null;
        }
           
 

        private static string HashCalculations(string value)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(value));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}