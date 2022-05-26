using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;

namespace Talabatak.Controllers.API
{
    [AllowAnonymous]
    [RoutePrefix("api/SubCategories")]
    public class SubCategoriesController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        public SubCategoriesController()
        {
            baseResponse = new BaseResponseDTO();
        }
        [HttpGet]
        [Route("GetSub")]
        public IHttpActionResult GetSubCategoriesBaseCategory(int id, string lang = "en")
        {
            try
            {
                if (id != 0)
                {
                    var SubCategories = db.Categories.Where(b => !b.IsDeleted).ToList();
                    if (SubCategories != null)
                    {
                        var Sub = SubCategories.SingleOrDefault(Pk => Pk.Id == id).ProductCategories.Where(d => !d.IsDeleted).ToList();

                        IEnumerable<ProductCategoryDTO> Data = Sub.Select(cat => new ProductCategoryDTO
                        {
                            Id = cat.Id,
                            Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? cat.NameAr : cat.NameEn,
                            ImageUrl = !string.IsNullOrEmpty(cat.ImageUrl) ? MediaControl.GetPath(FilePath.Category) + cat.ImageUrl : null,
                        });
                        return Ok(Data);

                    }

                }
            }
            catch (Exception)
            {
                return BadRequest("[]");
                throw;
            }

            return NotFound();
        }
        [Route("GetProducts")]
        public IHttpActionResult GetProductsBaseSubCategory(int id, string lang = "en")
        {
            if (id != 0)
            {
                try
                {
                    var ProductsCategories = db.ProductCategories.Where(s => !s.IsDeleted);
                   
                    if (ProductsCategories != null)
                    {

                         var Produts = ProductsCategories.SingleOrDefault(b => b.Id == id).Products.Where(b=>!b.IsDeleted).ToList();

                        List<ProductDTO> products = new List<ProductDTO>();
                        foreach (var product in Produts)
                        {
                            products.Add(new ProductDTO()
                            {
                                ProductId = product.Id,
                                Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.NameAr : product.NameEn,
                                Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.DescriptionAr : product.DescriptionEn,
                                Images = IsFind(product.Id),
                                IsMultipleSize = product.IsMultipleSize,
                                SingleOriginalPrice = product.SingleOriginalPrice.ToString(),
                                SingleOfferPrice = product.SingleOfferPrice.ToString(),
                                Sizes = product.Sizes.Select(b => new ProductSizeDTO
                                {
                                    SizeId = b.Id,
                                    Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? b.SizeAr : b.SizeEn,
                                    OfferPrice = b.OfferPrice.ToString(),
                                    OriginalPrice = b.OriginalPrice.ToString(),
                                }).ToList(),
                                StoreId = product.StoreId,
                                StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? product.Store.NameAr : product.Store.NameAr,

                            });

                        }
                        return Ok(products);


                    }

                }
                catch (Exception)
                {
                    return BadRequest("[]");
                   
                }
               
            } 
            return NotFound();
        }
        
          
    
        // Test For Image
        public string IsFind(long id)
        {
            string productImage = "";
            var Data = db.ProductImages.Where(b => !b.IsDeleted).ToList();
            try
            {
                productImage = MediaControl.GetPath(FilePath.Product) + Data.FirstOrDefault(b => b.ProductId == id).ImageUrl;
            }
            catch (Exception)
            {
                productImage = "";

                
            }
            return productImage;

        }
    }


}







