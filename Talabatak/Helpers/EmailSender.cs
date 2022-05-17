using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;

namespace Talabatak.Helpers
{
    public  class EmailSender:Page
    {
        public static bool SendEmail(string Title, string Content, string To)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            SmtpClient smtpClient = new SmtpClient();
            MailMessage message = new MailMessage()
            {
                Subject = Title,
                IsBodyHtml = true,
                Body = Content,
            };

            try
            {
                MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["SendMail.From"]);
                string fromPassword = ConfigurationManager.AppSettings["SendMail.Password"];
                message.From = fromAddress;
                message.To.Add(To);
                SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["SendMail.Host"]);
                NetworkCredential Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SendMail.From"], fromPassword);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = Credentials;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["SendMail.Port"]);    //alternative port number is 8889
                smtp.EnableSsl = false;
                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public void PrepareHTMLandSend(StoreOrder order, string culture)
        {
            string companyURL = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string OrderNumber = order.Id.ToString();

            string filepath = Server.MapPath("~/Content/Templates/CheckoutEmail.html");
            if (culture.ToLower() == "ar")
            {
                filepath = Server.MapPath("~/Content/Templates/CheckoutEmailAr.html");
            }
            string dest = Server.MapPath("~/Content/Templates/");
            string newFileName = Guid.NewGuid() + "CheckoutEmail.html";

            string newPath = Path.Combine(dest, newFileName);
            File.Copy(filepath, newPath, true);

            StreamReader objReader = new StreamReader(newPath);
            string content = objReader.ReadToEnd();
            objReader.Close();

            content = Regex.Replace(content, "{{order_no}}", order.Id.ToString());
            content = Regex.Replace(content, "{{Customer_Name}}", !string.IsNullOrEmpty(order.UserAddress.Name) ? order.UserAddress.Name : "");
            content = Regex.Replace(content, "{{Customer_Phone}}", !string.IsNullOrEmpty(order.UserAddress.PhoneNumber) ? order.UserAddress.PhoneNumber : "");
            content = Regex.Replace(content, "{{Customer_Address}}", !string.IsNullOrEmpty(order.UserAddress.Address) ? order.UserAddress.Address : "");
            content = Regex.Replace(content, "{{Customer_Building}}", !string.IsNullOrEmpty(order.UserAddress.BuildingNumber) ? order.UserAddress.BuildingNumber : "");
            content = Regex.Replace(content, "{{Customer_Floor}}", !string.IsNullOrEmpty(order.UserAddress.Floor) ? order.UserAddress.Floor : "");
            content = Regex.Replace(content, "{{Customer_Apartment}}", !string.IsNullOrEmpty(order.UserAddress.Apartment) ? order.UserAddress.Apartment : "");
            content = Regex.Replace(content, "{{SubTotal}}", order.SubTotal.ToString());
            content = Regex.Replace(content, "{{Total}}", order.Total.ToString());
            content = Regex.Replace(content, "{{PaymentMethod}}", order.PaymentMethod.ToString());
            content = Regex.Replace(content, "{{CreatedOn}}", order.CreatedOn.ToString("dd MMMM yyyy hh:mm tt"));
            string orderItems = "";
            foreach (var item in order.Items.Where(d => d.IsDeleted == false))
            {
                orderItems += $"<tr><td>{item.Product.NameAr}</td><td>{item.Quantity}</td><td>{item.SubTotal} LE</td></tr>";
            }
            content = Regex.Replace(content, "{{orderItems}}", orderItems);

            StreamWriter writer = new StreamWriter(newPath);
            writer.Write(content);
            writer.Close();

            StreamReader EmailBody = new StreamReader(newPath);
            string EmailContent = EmailBody.ReadToEnd();
            EmailBody.Close();
            SendEmail(culture.ToLower() == "ar" ? "طلباتك من FoodStation" : "Your order from Nozha", EmailContent, order.User.Email);
            File.Delete(newPath);
        }

        public static bool SendForgotPasswordEmail(string To, string Guid, string Name)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            SmtpClient smtpClient = new SmtpClient();
            var UrlSite =  HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            var VerificationUrl = UrlSite + "/account/setpassword?v=" + Guid;
            string Content = $"<div style='text-align:left' dir='ltr'><h4>Forgot your password instructions</h4><b>Hello {Name}</b>, <a href='{VerificationUrl}'>Click here</a> to set your new password for your account in Nozha. <p>This email sent to you because you have registered in <a href='{UrlSite}'>{UrlSite}</a> .</p><p>If you did not submit this request, ignore this email.</p></div>";
            MailMessage message = new MailMessage()
            {
                Subject = "Reset your password in Nozha",
                IsBodyHtml = true,
                Body = Content,
            };

            try
            {
                MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["SendMail.From"]);
                string fromPassword = ConfigurationManager.AppSettings["SendMail.Password"];
                message.From = fromAddress;
                message.To.Add(To);
                SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["SendMail.Host"]);
                NetworkCredential Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SendMail.From"], fromPassword);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = Credentials;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["SendMail.Port"]);    //alternative port number is 8889
                smtp.EnableSsl = false;
                smtp.Send(message);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static void SendVerificationEmail(string To, string Vcode, string Name)
        {
            ApplicationDbContext db = new ApplicationDbContext();
            SmtpClient smtpClient = new SmtpClient();
            var UrlSite =  HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string Content = $"<div style='text-align:left' dir='ltr'>Hello <b>{Name}</b>, <p>Thanks a lot for your nice interest in Nozha Super Market.</p><p>To complete your registration and activate your account in Nozha Super Market please use the following Activation code :</p> <strong>{Vcode}</strong> <p>This email sent to you because you have registered in <a href='{UrlSite}'>{UrlSite}</a> .</p><p>If you did not submit this request, ignore this email.</p><p>Best Regards,</p><p>Nozha Super Market</p></div>";
            MailMessage message = new MailMessage()
            {
                Subject = "Verify your account in Nozha",
                IsBodyHtml = true,
                Body = Content,
            };

            try
            {
                MailAddress fromAddress = new MailAddress(ConfigurationManager.AppSettings["SendMail.From"]);
                string fromPassword = ConfigurationManager.AppSettings["SendMail.Password"];
                message.From = fromAddress;
                message.To.Add(To);
                SmtpClient smtp = new SmtpClient(ConfigurationManager.AppSettings["SendMail.Host"]);
                NetworkCredential Credentials = new NetworkCredential(ConfigurationManager.AppSettings["SendMail.From"], fromPassword);
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = Credentials;
                smtp.Port = int.Parse(ConfigurationManager.AppSettings["SendMail.Port"]);    //alternative port number is 8889
                smtp.EnableSsl = false;
                smtp.Send(message);
            }
            catch (Exception ex)
            {
            }
        }
    }
}