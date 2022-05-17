using Talabatak.Filters;
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
using Talabatak.Models.Domains;
using System.Data.Entity;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/stores")]
    public class StoresController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;


        public StoresController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [Route("StoreReviews")]        public IHttpActionResult GetStoreReviews(long StoreId, string lang = "en")        {            List<StoreReviewDTO> storeReviewDTOs = new List<StoreReviewDTO>();            if (!db.Stores.Any(x => x.Id == StoreId))
            {                baseResponse.ErrorCode = Errors.StoreNotFound;                return Content(HttpStatusCode.NotFound, baseResponse);            }            else            {                foreach(var item in db.StoreReviews.Where(x => x.StoreId == StoreId).ToList())
                {
                    storeReviewDTOs.Add(new StoreReviewDTO()
                    {
                        Rate = item.Rate,
                        StoreId = item.StoreId,
                        Review = item.Review,
                        StoreOrderId = item.StoreOrderId,
                        Name = db.Users.FirstOrDefault(x=>x.Id == item.UserId).Name,
                        Image= "/Content/Images/Users/" + db.Users.FirstOrDefault(x=>x.Id == item.UserId).ImageUrl
                    });
                }                baseResponse.Data =storeReviewDTOs;                return Ok(baseResponse);            }        }
        [Route("GetDistance")]
        public IHttpActionResult GetDistance(long? StoreId)
        {
            var Ulat = double.Parse(Request.Headers.GetValues("latitude").FirstOrDefault());
            var Ulong = double.Parse(Request.Headers.GetValues("longitude").FirstOrDefault());
            if(!db.Stores.Any(x=>x.Id == StoreId))
            {
                baseResponse.ErrorCode = Errors.StoreNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }
            var store = db.Stores.FirstOrDefault(x => x.Id == StoreId);
            baseResponse.Data = Distance.GetDistanceBetweenTwoLocations(Ulat, Ulong, (double)store.Latitude, (double)store.Longitude);
            return Ok(baseResponse);
        }
        [UserLocation]
        [Route("GetAll")]
        public async Task<IHttpActionResult> GetAllStores(long CategoryId = 0 , string filter = "",string lang = "en")
        {

            bool Visiable = db.Categories.Any(x => x.Visiable == true && x.Id == CategoryId);
            var UserLatitude = double.Parse(Request.Headers.GetValues("latitude").FirstOrDefault());
            var UserLongitude = double.Parse(Request.Headers.GetValues("longitude").FirstOrDefault());
            var AreaId = long.Parse(Request.Headers.GetValues("areaId").FirstOrDefault());
            List<Store> Stores = new List<Store>();
            List<Store> ProductGategoryId = new List<Store>();
            if (string.IsNullOrEmpty(filter))
            {
                if (db.Stores.Any(s => s.IsDeleted == false && s.IsAccepted == true &&
                   s.IsBlocked == false && s.IsHidden == false && (CategoryId != 0  ? s.CategoryId == CategoryId : true) &&
                   s.Latitude.HasValue == true && s.Longitude.HasValue == true && ((AreaId > 0  ? s.CityId == AreaId : true)|| Visiable)))
                {
                    Stores = db.Stores.Where(s => s.IsDeleted == false && s.IsAccepted == true &&
                     s.IsBlocked == false && s.IsHidden == false &&
                     (CategoryId!=0?s.CategoryId==CategoryId:true )&&
                    s.Latitude.HasValue == true && s.Longitude.HasValue == true && ((AreaId > 0 ? s.CityId == AreaId : true)||Visiable)).ToList();
                }
            }
            else
            {
                if (db.Stores.Any(s => s.IsDeleted == false && ((AreaId > 0 ? s.CityId == AreaId : true) || Visiable) &&
                  s.IsAccepted == true && s.IsBlocked == false && s.IsHidden == false &&
                   (CategoryId != 0 ? s.CategoryId == CategoryId : true) &&
                  s.Latitude.HasValue == true && s.Longitude.HasValue == true && (s.NameAr.Trim().ToLower().Contains(filter.Trim().ToLower())
                  || s.NameEn.Trim().ToLower().Contains(filter.Trim().ToLower())
                  )))
                {
                    Stores = db.Stores.Where(s => s.IsDeleted == false && ((AreaId > 0 ? s.CityId == AreaId : true) || Visiable) &&
                        s.IsAccepted == true && s.IsBlocked == false && s.IsHidden == false &&
                        (CategoryId != 0 ? s.CategoryId == CategoryId : true) &&
                        s.Latitude.HasValue == true && s.Longitude.HasValue == true && (s.NameAr.Trim().ToLower().Contains(filter.Trim().ToLower())
                        || s.NameEn.Trim().ToLower().Contains(filter.Trim().ToLower()))).ToList();
                }


                if (db.Products.Any(s => s.IsDeleted == false && (s.NameAr.Trim().ToLower().Contains(filter.Trim().ToLower())
                 || s.NameEn.Trim().ToLower().Contains(filter.Trim().ToLower()))))
                {
                    ProductGategoryId = db.Products.Include(x=>x.Category).Include(x=>x.Category.Store).Where(s => s.IsDeleted == false && (s.NameAr.Trim().ToLower().Contains(filter.Trim().ToLower())
                        || s.NameEn.Trim().ToLower().Contains(filter.Trim().ToLower()))).Select(x=>x.Category.Store).Distinct().ToList();

                   var storeProduct = new List<Store>();
                   foreach (var rec in ProductGategoryId)
                    {                                                                                           
                     if (!Stores.Any(x => x.Id == rec.Id))
                        {
                            if(rec.IsDeleted==false && rec.IsHidden == false && rec.IsAccepted == true && rec.IsBlocked == false
                                && ((AreaId > 0 ? rec.CityId == AreaId : true) || Visiable) && (CategoryId != 0 ? rec.CategoryId == CategoryId : true) &&
                        rec.Latitude.HasValue == true && rec.Longitude.HasValue == true)
                            {
                                Stores.Add(rec);
                            }
                       
                        }                                                
                    }
                }
            }
            Stores = Stores.OrderBy(d => Distance.GetDistanceBetweenTwoLocations(UserLatitude, UserLongitude, d.Latitude.Value, d.Longitude.Value)).ToList();
            List<StoreDTO> storeDTOs = new List<StoreDTO>();
           
            foreach (var store in Stores)
            {
              
                
                storeDTOs.Add(new StoreDTO()
                {
                    
                    Id = store.Id,
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? store.NameAr : store.NameEn,
                    ImageUrl = !string.IsNullOrEmpty(store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + store.LogoImageUrl : null,
                    Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? store.DescriptionAr : store.DescriptionEn,
                    IsOpen = store.IsOpen,
                    Rate = store.Rate.ToString("N1"),
                    Distance = await Distance.GetEstimatedDataBetweenTwoLocationsText(UserLatitude, UserLongitude, (double)store.Latitude, (double)store.Longitude)
            });;
                
            }
            baseResponse.Data = storeDTOs;
            return Ok(baseResponse);
        }
        [Route("Details")]
        public IHttpActionResult GetStoreDetails(long StoreId,  string lang = "en")
        {
               var  Store = db.Stores.FirstOrDefault(s => s.IsDeleted == false && s.IsAccepted == true && s.IsBlocked == false && s.IsHidden == false && s.Id == StoreId);
                 
            if (Store == null)
            {
                baseResponse.ErrorCode = Errors.StoreNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            Store.SeenCount += 1;
            db.SaveChanges();

            StoreDetailsDTO storeDTO = new StoreDetailsDTO()
            {
                Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn,
                Address = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.AddressAr : Store.AddressEn,
                Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.DescriptionAr : Store.DescriptionEn,
                SeenCount = Store.SeenCount,
                CoverImageUrl = !string.IsNullOrEmpty(Store.CoverImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.CoverImageUrl : null,
                LogoImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null,
                Id = Store.Id,
                Latitude = Store.Latitude,
                Longitude = Store.Longitude,
                IsOpen = Store.IsOpen,
                PhoneNumber = Store.PhoneNumber,
                Rate = Store.Rate.ToString("N1")
            };

            if (Store.ProductCategories != null && Store.ProductCategories.Count(s => s.IsDeleted == false) > 0)
            {
               /* string Currency = "LE";
                string CurrencyAr = "ريال";
                var sizeprice="";
                var sizeoffer = "";
                bool flag = false;*/
                foreach (var category in Store.ProductCategories.Where(s => s.IsDeleted == false).OrderBy(s => s.SortingNumber))
                { 
                    ProductCategoryDTO productCategoryDTO = new ProductCategoryDTO()
                    {
                        Id = category.Id,
                        ImageUrl = !string.IsNullOrEmpty(category.ImageUrl) ? MediaControl.GetPath(FilePath.Product) + category.ImageUrl : null,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? category.NameAr : category.NameEn,
                    };
                   /* if (category.Products != null && category.Products.Count(w => w.IsDeleted == false) > 0)
                    {
                        foreach (var product in category.Products.Where(s => s.IsDeleted == false).OrderBy(w => w.SortingNumber))
                        {
                            if (!flag) {
                                flag = true;
                            CurrencyAr = (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال" : product.CurrencyAr);
                                Currency = (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);
                            }
                            ProductDTO productDTO = new ProductDTO()
                            {
                                Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.NameAr : product.NameEn,
                                Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.DescriptionAr : product.DescriptionEn,
                                IsMultipleSize = product.IsMultipleSize,
                                ProductId = product.Id,
                            };
                            if (lang.ToLower() == "ar")
                            {
                                productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال" : product.CurrencyAr);
                                productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال": product.CurrencyAr);

                            }
                            else
                            {
                                productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);
                                productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);

                            }
                            if (product.Images != null && product.Images.Count(s => s.IsDeleted == false) > 0)
                            {
                                foreach (var image in product.Images.Where(s => s.IsDeleted == false))
                                {
                                    productDTO.Images.Add(MediaControl.GetPath(FilePath.Product) + image.ImageUrl);
                                }
                            }
                            if (product.Sizes != null && product.Sizes.Count(s => s.IsDeleted == false) > 0)
                            {
                                foreach (var size in product.Sizes.Where(s => s.IsDeleted == false))
                                {
                                    if (size.OfferPrice > (decimal)0.0)
                                    {
                                        sizeoffer = size.OfferPrice + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency);
                                    }
                                    if(size.OriginalPrice>(decimal)0.0)
                                    {
                                        sizeprice = size.OriginalPrice + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency);
                                    }
                                    productDTO.Sizes.Add(new ProductSizeDTO()
                                    {
                                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? size.SizeAr : size.SizeEn,
                                        OfferPrice = sizeoffer ,
                                        OriginalPrice = sizeprice,
                                        SizeId = size.Id
                                    });
                                    sizeprice = "";
                                    sizeoffer = "";
                                }
                            }
                            productCategoryDTO.Products.Add(productDTO);
                        }
                    }*/
                    storeDTO.Categories.Add(productCategoryDTO);
                }
            }
            baseResponse.Data = storeDTO;
            return Ok(baseResponse);
        }
        [Route("StoreProducts")]
        [HttpGet]
        [Authorize]
        public IHttpActionResult StoreProducts(long CategoyId,int page = 1, string lang = "en")
        {
            var dto = new List<ProductDTO>();
            string Currency = "LE";
            string CurrencyAr = "ريال";
            var sizeprice = "";
            var sizeoffer = "";
            bool flag = false;
            List<Product> products = new List<Product>();

            if (db.Products.Where(s => s.IsDeleted == false && s.SubCategoryId == CategoyId).ToList().Count > 50)
            {
                var index = db.Products.Where(s => s.IsDeleted == false && s.SubCategoryId == CategoyId).ToList().Count / 2;
                if (page == 1)
                {
                    products = db.Products.Where(s => s.IsDeleted == false && s.SubCategoryId == CategoyId)
                        .OrderBy(w => w.SortingNumber).Skip((page - 1) * index).Take(index).ToList();
                }
                else
                {
                    products = db.Products.Where(s => s.IsDeleted == false && s.SubCategoryId == CategoyId)
                      .OrderBy(w => w.SortingNumber).Skip(index).ToList();
                }
            }
            else
            {
                if (page == 1)
                {
                    products = db.Products.Where(s => s.IsDeleted == false && s.SubCategoryId == CategoyId).OrderBy(w => w.SortingNumber).ToList();
                }
            }
            foreach (var product in products)
            {
                if (!flag)
                {
                    flag = true;
                    CurrencyAr = (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال" : product.CurrencyAr);
                    Currency = (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);
                }
                ProductDTO productDTO = new ProductDTO()
                {
                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.NameAr : product.NameEn,
                    Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.DescriptionAr : product.DescriptionEn,
                    IsMultipleSize = product.IsMultipleSize,
                    ProductId = product.Id,
                };
                if (lang.ToLower() == "ar")
                {
                    productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال" : product.CurrencyAr);
                    productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.CurrencyAr) ? "ريال" : product.CurrencyAr);

                }
                else
                {
                    productDTO.SingleOfferPrice = product.SingleOfferPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);
                    productDTO.SingleOriginalPrice = product.SingleOriginalPrice.ToString() + " " + (string.IsNullOrEmpty(product.Currency) ? "LE" : product.Currency);

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
                        if (size.OfferPrice > (decimal)0.0)
                        {
                            sizeoffer = size.OfferPrice + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency);
                        }
                        if (size.OriginalPrice > (decimal)0.0)
                        {
                            sizeprice = size.OriginalPrice + " " + (lang.ToLower() == "ar" ? CurrencyAr : Currency);
                        }
                        productDTO.Sizes.Add(new ProductSizeDTO()
                        {
                            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? size.SizeAr : size.SizeEn,
                            OfferPrice = sizeoffer,
                            OriginalPrice = sizeprice,
                            SizeId = size.Id
                        });
                        sizeprice = "";
                        sizeoffer = "";
                    }
                }
                if (User.Identity.IsAuthenticated)
                {
                    var userid = User.Identity.GetUserId();
                    productDTO.IsFavorite =  db.UserFavoriteProducts.Any(x => x.ProductId == product.Id
                      && x.UserId == userid && !x.IsDeleted);
                }
                dto.Add(productDTO);
            }
            baseResponse.Data = dto;
            return Ok(baseResponse);
        }
    }
}