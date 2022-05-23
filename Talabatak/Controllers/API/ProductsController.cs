using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Microsoft.AspNet.Identity.Owin;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
using Microsoft.AspNet.Identity;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/products")]
    public class ProductsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;

        public ProductsController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("Offers")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult GetOffers(long CategoryId= -1,string lang = "en" , int Page = 1 )
        {
            int PageSize = 15;
            int Skip = (Page - 1) * PageSize;
            if (Page <= 0)
            {
                baseResponse.ErrorCode = Errors.PageMustBeGreaterThanZero;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            var Products = db.Products.Include(d=>d.Sizes).Where(s => s.IsDeleted == false && (CategoryId == -1 ? true: s.SubCategoryId == CategoryId)).OrderBy(w => w.SortingNumber).Skip(Skip).Take(PageSize).ToList();
            List<ProductDTO> productDTOs = new List<ProductDTO>();
            string Currency = "جنيها";
            string CurrencyAr = "جنيها";
            bool flag = false;
            foreach (var product in Products)
            {
                if (!flag)
                {
                    flag = true;
                    CurrencyAr = (string.IsNullOrEmpty(product.CurrencyAr) ? "جنيها" : product.CurrencyAr);
                    Currency = (string.IsNullOrEmpty(product.Currency) ? "جنيها" : product.Currency);
                }
                ProductDTO productDTO = new ProductDTO()
                {
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.NameAr : product.NameEn,
                    Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.DescriptionAr : product.DescriptionEn,
                    IsMultipleSize = product.IsMultipleSize,
                    ProductId = product.Id,
                    StoreId= db.ProductCategories.FirstOrDefault(x=>x.Id==product.SubCategoryId).StoreId,
                    Sizes = product.Sizes.Select(b => new ProductSizeDTO
                    {
                        SizeId = b.Id,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? b.SizeAr : b.SizeEn,
                        OfferPrice = b.OfferPrice.ToString() + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency),
                        OriginalPrice = b.OriginalPrice.ToString() + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency),
                    }).ToList()
                };
                if (lang.ToLower() == "ar")
                {
                    productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "جنيها" : product.CurrencyAr);
                    productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "جنيها" : product.CurrencyAr);

                }
                else
                {
                    productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "جنيها" : product.Currency);
                    productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "جنيها" : product.Currency);

                }
                if (lang.ToLower() == "ar")
                {
                    productDTO.StoreName = db.Stores.FirstOrDefault(x => x.Id == productDTO.StoreId).NameAr;
                }
                else
                {
                    productDTO.StoreName = db.Stores.FirstOrDefault(x => x.Id == productDTO.StoreId).NameEn;

                }

                if (product.Images != null && product.Images.Count(s => s.IsDeleted == false) > 0)
                {
                    foreach (var image in product.Images.Where(s => s.IsDeleted == false))
                    {
                        productDTO.Images=MediaControl.GetPath(FilePath.Product) + image.ImageUrl;
                    }
                }
                //if (product.Sizes != null && product.Sizes.Count(s => s.IsDeleted == false) > 0)
                //{
                //    product.Sizes.Select(b => new ProductSizeDTO
                //    {
                //        SizeId = b.Id,
                //        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? b.SizeAr : b.SizeEn,
                //        OfferPrice = b.OfferPrice.ToString(),
                //        OriginalPrice = b.OriginalPrice.ToString(),
                //    });
                //}
                if (User.Identity.IsAuthenticated)
                {
                    var userid = User.Identity.GetUserId();
                    productDTO.IsFavorite = db.UserFavoriteProducts.Any(x => x.ProductId == product.Id
                     && x.UserId == userid && !x.IsDeleted);
                }
                productDTOs.Add(productDTO);
            }
            baseResponse.Data = productDTOs;
            return Ok(baseResponse);
        }
    }
}