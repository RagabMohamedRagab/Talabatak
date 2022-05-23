using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using Talabatak.Models.Domains;
using Talabatak.Models.Enums;

namespace Talabatak.Helpers
{
    public class ExcelExport
    {
        public static DataTable OrdersExport(List<StoreOrder> order)
        {
            DataTable dt = new DataTable("Report");
            dt.Columns.AddRange(new DataColumn[7] { new DataColumn("المتجر"),
                                            new DataColumn("الحاله"),
                                            new DataColumn("رقم الطلب"),
                                            new DataColumn("العميل"),
                                             new DataColumn("السعر"),
                                              new DataColumn("التوقيت"),
                                               new DataColumn("السائق")});
            foreach (var item in order)
            {
                string status = "";
                switch (item.Status)
                {
                    case StoreOrderStatus.Cancelled:
                        status = "تم الغاءه";
                        break;
                    case StoreOrderStatus.Delivering:
                        status = "جارى التوصيل";
                        break;
                    case StoreOrderStatus.Finished:
                        status = "تم التوصيل";
                        break;
                    case StoreOrderStatus.Preparing:
                        status = "جارى التحضير";
                        break;
                    case StoreOrderStatus.Rejected:
                        status = "مرفوض";
                        break;
                }
                string username = "";
                if (item.UserAddressId.HasValue == true)
                {
                    username = item.UserAddress.Name;
                }
                else
                {
                    username = item.User.Name;
                }
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(item.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                string Date = CreatedOn.ToString("dd MMM yyyy");
                string DriverName = "";
                if (item.DriverId.HasValue == true)
                {
                    DriverName = item.Driver.User.Name;
                }
                else
                {
                    DriverName = "بدون";
                }
                dt.Rows.Add(item.Items.FirstOrDefault().Product.Category.Store.NameAr,
                     status, item.Code, username, item.Total.ToString() + "جنيها", Date, DriverName);
            }
            return dt;
        }
        public static DataTable UsersExport(List<ApplicationUser> users)
        {
            DataTable dt = new DataTable("Report");
            dt.Columns.AddRange(new DataColumn[5] { new DataColumn("الحاله"),
                                            new DataColumn("الاسم"),
                                            new DataColumn("المدينة"),
                                            new DataColumn("رقم الهاتف"),
                                             new DataColumn("تاريخ التسجيل"),});
            foreach(var user in users)
            {
                var status = user.IsDeleted ? "محظور" : "نشط";
                var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(user.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
                string Date = CreatedOn.ToString("dd MMM yyyy");
                dt.Rows.Add(status,user.Name,(user.CityId.HasValue?user.City.NameAr:"بدون"),
                    user.PhoneNumber, CreatedOn);
            }
            return dt;
        }
    }
}