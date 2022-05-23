using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;

namespace Talabatak.Controllers.API
{
    [RoutePrefix("api/FavoriteProducts")]
    public class FavoriteProductsController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        public FavoriteProductsController()
        {
            baseResponse = new BaseResponseDTO();
        }
        [HttpPost]
        [Authorize]
        public async Task<IHttpActionResult> Post(long ProductId)
        {
            var userid = User.Identity.GetUserId();
            if (await db.UserFavoriteProducts.AnyAsync(x=>x.ProductId==ProductId&&
            x.UserId == userid))
            {
                var favorite = db.UserFavoriteProducts.FirstOrDefault(x => x.ProductId == ProductId &&
             x.UserId == userid);
                favorite.IsDeleted = !favorite.IsDeleted;
            }
            else
            {
                db.UserFavoriteProducts.Add(new UserFavoriteProduct()
                {
                    ProductId = ProductId,
                    UserId = userid
                });
            }
            await db.SaveChangesAsync();
            return Ok(baseResponse);
        }
        [HttpGet]
        [Authorize]
        public IHttpActionResult Get(string lang = "en")
        {
            var userid = User.Identity.GetUserId();
            var Products = db.UserFavoriteProducts.Where(x => x.UserId == userid && !x.IsDeleted)
                .Select(x => x.Product).ToList();
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
                    StoreId = db.ProductCategories.FirstOrDefault(x => x.Id == product.SubCategoryId).StoreId,
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
                if (product.Sizes != null && product.Sizes.Count(s => s.IsDeleted == false) > 0)
                {
                    foreach (var size in product.Sizes.Where(s => s.IsDeleted == false))
                    {
                        productDTO.Sizes.Add(new ProductSizeDTO()
                        {
                            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? size.SizeAr : size.SizeEn,
                            OfferPrice = size.OfferPrice.ToString() + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency),
                            OriginalPrice = size.OriginalPrice.ToString() + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency),
                            SizeId = size.Id
                        });
                    }
                }
                productDTO.IsFavorite = true;
                productDTOs.Add(productDTO);
            }
            baseResponse.Data = productDTOs;
            return Ok(baseResponse);
        }
    }
}
