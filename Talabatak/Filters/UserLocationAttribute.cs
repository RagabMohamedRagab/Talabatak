using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Talabatak.Filters
{
    public class UserLocationAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var Headers = HttpContext.Current.Request.Headers;
            BaseResponseDTO baseResponse = new BaseResponseDTO();
            try
            {
                var Latitude = Headers.GetValues("latitude").FirstOrDefault();
                double LatitudeDouble = double.Parse(Latitude);
            }
            catch (Exception)
            {
                baseResponse.ErrorCode = Errors.LatitudeFieldIsRequired;
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, baseResponse);
            }
            try
            {
                var Longitude = Headers.GetValues("longitude").FirstOrDefault();
                double LongitudeDouble = double.Parse(Longitude);
            }
            catch (Exception)
            {
                baseResponse.ErrorCode = Errors.LongitudeFieldIsRequired;
                actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, baseResponse);
            }
            //try
            //{
            //    var CityId = Headers.GetValues("cityid").FirstOrDefault();
            //    var CityIdLong = long.Parse(CityId);
            //    ApplicationDbContext db = new ApplicationDbContext();
            //    var City = db.Cities.FirstOrDefault(d => !d.IsDeleted && d.Id == CityIdLong && d.Country.IsDeleted == false);
            //    if (City == null)
            //    {
            //        baseResponse.ErrorCode = Errors.CityNotFound;
            //        actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.NotFound, baseResponse);
            //    }
            //}
            //catch (Exception)
            //{
            //    baseResponse.ErrorCode = Errors.CityFieldIsRequired;
            //    actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest, baseResponse);
            //}
            base.OnActionExecuting(actionContext);
        }
    }
}