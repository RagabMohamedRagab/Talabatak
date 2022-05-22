using Talabatak.Filters;
using Talabatak.Helpers;
using Talabatak.Models.Data;
using Talabatak.Models.Domains;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using Talabatak.SignalR;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Data.Entity;
namespace Talabatak.Controllers.API
{
    [System.Web.Http.Authorize]
    [RoutePrefix("api/storeOrders")]
    public class StoreOrdersController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private string CurrentUserId;

        public StoreOrdersController()
        {
            baseResponse = new BaseResponseDTO();
        }

        [HttpPost]
        [Route("addtobasket")]
        public async Task<IHttpActionResult> AddItemToBasket([FromBody]AddItemToBasketDTO model)
        {
            CurrentUserId = User.Identity.GetUserId();
            var Validation = ValidateAddItemToBasket(model, CurrentUserId);
            if (Validation != Errors.Success)
            {
                baseResponse.ErrorCode = Validation;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Product = await db.Products.FindAsync(model.ProductId);
            if (Product.Inventory < model.Quantity)
            {
                baseResponse.ErrorCode = Errors.QuntityNotAvilable;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            ProductSize Size = null;
            if (model.SizeId > 0)
            {
                Size = db.ProductSizes.Find(model.SizeId);
            }
            decimal Price = 0;
            if (Product.IsMultipleSize == false)
            {
                if (Product.SingleOfferPrice.HasValue == true)
                    Price = Product.SingleOfferPrice.Value;
                else
                {
                    if (Product.SingleOriginalPrice.HasValue == true)
                        Price = Product.SingleOriginalPrice.Value;
                }
            }
            else
            {
                if (Size != null)
                {
                    if (Size.OfferPrice.HasValue == true)
                        Price = Size.OfferPrice.Value;
                    else
                    {
                        Price = Size.OriginalPrice;
                    }
                }
            }
            var UserOrder = db.StoreOrders.FirstOrDefault(x => x.UserId == CurrentUserId && x.Status == StoreOrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        string Code = RandomGenerator.GenerateNumber(10000, 99999).ToString();
                        StoreOrder order = new StoreOrder()
                        {
                            UserId = CurrentUserId,
                            Code = Code,
                            PaymentMethod = PaymentMethod.Cash,
                            Status = StoreOrderStatus.Initialized,
                            IsDeliveryFeesUpdated = false,
                            PaymentUniqueKey = Guid.NewGuid().ToString()
                        };
                        db.StoreOrders.Add(order);
                        db.SaveChanges();
                        StoreOrderItem orderItem = new StoreOrderItem()
                        {
                            OrderId = order.Id,
                            Price = Price,
                            ProductId = model.ProductId,
                            Quantity = model.Quantity,
                            SubTotal = Price * model.Quantity,
                        };

                        if (Size != null)
                        {
                            orderItem.SizeId = Size.Id;
                        }
                        order.Items = new List<StoreOrderItem>();
                        order.Items.Add(orderItem);
                        StoreOrderActions.CalculateOrderPrice(order);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            else
            {
                if (UserOrder.Items != null && UserOrder.Items.Count(s => s.IsDeleted == false) > 0)
                {
                    var FirstItem = UserOrder.Items.FirstOrDefault(s => s.IsDeleted == false);
                    if (FirstItem.Product.Category.StoreId != Product.Category.StoreId)
                    {
                        baseResponse.ErrorCode = Errors.OrderCannotBeFromMultipleStores;
                        return Content(HttpStatusCode.BadRequest, baseResponse);
                    }
                }
                using (var Transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        StoreOrderItem OrderItem = null;
                        if (Size != null)
                        {
                            OrderItem = db.StoreOrderItems.FirstOrDefault(x => x.OrderId == UserOrder.Id && x.ProductId == model.ProductId && !x.IsDeleted && x.SizeId == Size.Id);
                        }
                        else
                        {
                            OrderItem = db.StoreOrderItems.FirstOrDefault(x => x.OrderId == UserOrder.Id && x.ProductId == model.ProductId && !x.IsDeleted);
                        }
                        if (OrderItem == null)
                        {
                            StoreOrderItem orderItem = new StoreOrderItem()
                            {
                                OrderId = UserOrder.Id,
                                Price = Price,
                                ProductId = model.ProductId,
                                Quantity = model.Quantity,
                                SubTotal = Price * model.Quantity,
                            };

                            if (Size != null)
                            {
                                orderItem.SizeId = Size.Id;
                            }
                            UserOrder.Items.Add(orderItem);
                        }
                        else
                        {
                            OrderItem.Quantity += model.Quantity;
                            OrderItem.SubTotal = OrderItem.Price * OrderItem.Quantity;
                            CRUD<StoreOrderItem>.Update(OrderItem);
                        }
                      
                        StoreOrderActions.CalculateOrderPrice(UserOrder);
                        db.SaveChanges();
                        Transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }
            return Ok(baseResponse);
        }

        private Errors ValidateAddItemToBasket(AddItemToBasketDTO model, string CurrentUserId)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return Errors.UserNotAuthorized;

            var user = db.Users.Find(CurrentUserId);
            if (user == null)
                return Errors.UserNotAuthorized;

            var Product = db.Products.FirstOrDefault(s => s.Id == model.ProductId && s.IsDeleted == false && s.Category.IsDeleted == false && s.Category.Store.IsDeleted == false && s.Category.Store.IsAccepted == true && s.Category.Store.IsBlocked == false && s.Category.Store.IsHidden == false && s.Category.Store.Latitude.HasValue == true && s.Category.Store.Longitude.HasValue == true);
            if (Product == null)
                return Errors.ProductNotFound;

            if (Product.Category.Store.IsOpen == false)
            {
                return Errors.StoreIsClosed;
            }

            if (model.SizeId > 0)
            {
                var Size = db.ProductSizes.FirstOrDefault(s => s.IsDeleted == false && s.Id == model.SizeId && s.ProductId == model.ProductId);
                if (Size == null)
                {
                    return Errors.SizeNotFound;
                }
            }

            return Errors.Success;
        }

        [HttpGet]
        [Route("basket")]
        public async Task<IHttpActionResult> GetBasketDetails(string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var user = db.Users.Find(CurrentUserId);
            if (user == null)
            {
                baseResponse.ErrorCode = Errors.UserNotAuthorized;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var UserOrder = db.StoreOrders.Include(x => x.Items).FirstOrDefault(x => x.UserId == CurrentUserId && x.Status == StoreOrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                return Ok(baseResponse);
            }
            if (UserOrder.Items.Count < 0)
            {
                baseResponse.Data = null;
                return Ok(baseResponse);
            }
            if (UserOrder.IsDeliveryFeesUpdated == false)
            {
                var item = UserOrder.Items.First();
                if (item.Product.Category.Store.DeliveryBySystem)
                {
                    await StoreOrderActions.CalculateDeliveryFees(UserOrder);
                }
                else
                {
                    await StoreOrderActions.CalculateDeliveryFeesForStore(UserOrder, item.Product.Category.Store);
                }
                StoreOrderActions.CalculateOrderPrice(UserOrder);
                db.SaveChanges();
            }
            var ProductId = db.StoreOrderItems.FirstOrDefault(x => x.OrderId == UserOrder.Id).ProductId;
            var CategoryId = db.Products.FirstOrDefault(x => x.Id == ProductId).SubCategoryId;
            var storeId = db.ProductCategories.FirstOrDefault(x => x.Id == CategoryId).StoreId;
            var store = db.Stores.FirstOrDefault(x => x.Id == storeId);
            BasketDetailsDTO basketDetailsDTO = new BasketDetailsDTO()
            {
                CustomerHasAddress = UserOrder.UserAddressId.HasValue ? true : false,
                UserWallet = UserOrder.User.Wallet,
                Address = lang == "en" ? store.AddressEn : store.AddressAr,
                Id = storeId,
                Name = lang == "en" ? store.NameEn : store.NameAr,
                CoverImageUrl = "/Content/Images/Stores/" + store.CoverImageUrl,
                LogoImageUrl = "/Content/Images/Stores/" + store.LogoImageUrl,
                Description = lang == "en" ? store.DescriptionEn : store.DescriptionAr,
                PhoneNumber = store.PhoneNumber,
                Latitude = store.Latitude,
                Longitude = store.Longitude,
                SeenCount = store.SeenCount,
                IsOpen = store.IsOpen,
                Rate = store.Rate.ToString()
            };

            if (UserOrder.UserAddressId.HasValue == true)
            {
                basketDetailsDTO.UserAddress = new UserAddressDTO()
                {
                    Address = UserOrder.UserAddress.Address,
                    AddressInDetails = UserOrder.UserAddress.AddressInDetails,
                    Apartment = UserOrder.UserAddress.Apartment,
                    BuildingNumber = UserOrder.UserAddress.BuildingNumber,
                    Floor = UserOrder.UserAddress.Floor,
                    Id = UserOrder.UserAddress.Id,
                    Latitude = UserOrder.UserAddress.Latitude,
                    Longitude = UserOrder.UserAddress.Longitude,
                    Name = UserOrder.UserAddress.Name,
                    PhoneNumber = UserOrder.UserAddress.PhoneNumber,
                };
            }
            if (UserOrder.Items != null)
            {
                var Product = new Product();
                foreach (var item in UserOrder.Items.Where(d => d.IsDeleted == false))
                {
                    Product = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    BasketItemDTO basketItemDTO = new BasketItemDTO()
                    {
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.DescriptionAr : item.Product.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.NameAr : item.Product.NameEn,
                        BasketItemId = item.Id,
                        Image = item.Product.Images != null && item.Product.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? MediaControl.GetPath(FilePath.Product) + item.Product.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null,
                        Quantity = item.Quantity,
                    };
                    if (lang.ToLower() == "ar")
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        basketItemDTO.SubTotal = item.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        basketDetailsDTO.Total = UserOrder.Total.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        basketDetailsDTO.SubTotal = UserOrder.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        basketDetailsDTO.DeliveryFees = UserOrder.DeliveryFees.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                    }
                    else
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        basketItemDTO.SubTotal = item.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        basketDetailsDTO.Total = UserOrder.Total.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        basketDetailsDTO.SubTotal = UserOrder.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        basketDetailsDTO.DeliveryFees = UserOrder.DeliveryFees.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);

                    }
                    if (item.SizeId.HasValue == true)
                    {
                        basketItemDTO.Size = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Size.SizeAr : item.Size.SizeEn;
                    }
                    basketDetailsDTO.BasketItems.Add(basketItemDTO);
                }
            }

            baseResponse.Data = basketDetailsDTO;
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("increase")]
        public IHttpActionResult IncreaseBasketItem(long BasketItemId)
        {
            CurrentUserId = User.Identity.GetUserId();
            var BasketItem = db.StoreOrderItems.Include(x => x.Product).FirstOrDefault(x => x.Id == BasketItemId && !x.IsDeleted && !x.Order.IsDeleted && x.Order.UserId == CurrentUserId);
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.Status != StoreOrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            BasketItem.Quantity += 1;
            if (BasketItem.Product.Inventory < BasketItem.Quantity)
            {

                baseResponse.ErrorCode = Errors.QuntityNotAvilable;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            BasketItem.SubTotal = BasketItem.Price * BasketItem.Quantity;
            CRUD<StoreOrderItem>.Update(BasketItem);
            StoreOrderActions.CalculateOrderPrice(BasketItem.Order);
            db.SaveChanges();
            baseResponse.Data = new
            {
                BasketItem.SubTotal
            };
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("decrease")]
        public IHttpActionResult DecreaseBasketItem(long BasketItemId)
        {
            CurrentUserId = User.Identity.GetUserId();
            var BasketItem = db.StoreOrderItems.FirstOrDefault(x => x.Id == BasketItemId && !x.IsDeleted && !x.Order.IsDeleted && x.Order.UserId == CurrentUserId);
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.Status != StoreOrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Quantity > 1)
            {
                BasketItem.Quantity -= 1;
                BasketItem.SubTotal = BasketItem.Price * BasketItem.Quantity;
                CRUD<StoreOrderItem>.Update(BasketItem);
                StoreOrderActions.CalculateOrderPrice(BasketItem.Order);
                db.SaveChanges();
                baseResponse.Data = new
                {
                    BasketItem.SubTotal
                };
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.CannotDecrease;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
        }

        [HttpDelete]
        [Route("delete")]
        public IHttpActionResult DeleteBasketItem(long BasketItemId)
        {
            CurrentUserId = User.Identity.GetUserId();
            var BasketItem = db.StoreOrderItems.FirstOrDefault(x => x.Id == BasketItemId && x.Order.UserId == CurrentUserId);
            if (BasketItem == null)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (BasketItem.Order.Status != StoreOrderStatus.Initialized)
            {
                baseResponse.ErrorCode = Errors.BasketItemNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            CRUD<StoreOrderItem>.Delete(BasketItem);
            StoreOrderActions.CalculateOrderPrice(BasketItem.Order);
            if (BasketItem.Order.Items.All(d => d.IsDeleted))
            {
                CRUD<StoreOrder>.Delete(BasketItem.Order);
            }
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [HttpDelete]
        [Route("deleteBasket")]
        public IHttpActionResult DeleteWholeBasket()
        {
            CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.StoreOrders.FirstOrDefault(x => x.UserId == CurrentUserId && x.Status == StoreOrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            CRUD<StoreOrder>.Delete(UserOrder);
            db.SaveChanges();
            return Ok(baseResponse);
        }
        [HttpDelete]
        [Route("Cancel")]
        public async Task<IHttpActionResult> CancelOrder(long OrderId)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.StoreOrders.FirstOrDefault(d => d.IsDeleted == false && (d.Status == StoreOrderStatus.Placed || d.Status == StoreOrderStatus.Delivering || d.Status == StoreOrderStatus.Preparing) && d.UserId == CurrentUserId && d.Id == OrderId);
            if (UserOrder != null)
            {
                UserOrder.Status = StoreOrderStatus.Cancelled;
                UserOrder.IsPaid = false;
                db.SaveChanges();
                var Order = db.StoreOrders.Include(x=>x.User).FirstOrDefault(x => x.Id == OrderId && x.IsDeleted == false);
                Order.IsPaid = false;
                db.SaveChanges();
                if (Order.PaymentMethod == PaymentMethod.Online || Order.PaymentMethod == PaymentMethod.Wallet)
                {
                    Order.User.Wallet += Order.Total;
                    db.UserWallets.Add(new UserWallet()
                    {
                        StoreOrderId = OrderId,
                        TransactionAmount = Order.Total,
                        TransactionType = TransactionType.RefundTheUser,
                        UserId = Order.UserId
                    });
                    db.SaveChanges();
                }
                var OrderDrivers = db.StoreOrderDrivers.Where(s => s.IsDeleted == false && s.OrderId == OrderId && s.IsAccepted.HasValue == false).ToList();
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                foreach (var driver in OrderDrivers)
                {
                    OrdersHub.Clients.Group(driver.Id.ToString()).newOrder(); // To remove the order from these drivers page.
                }
                if (UserOrder.DriverId.HasValue == true)
                {
                    string Title = $"تم الغاء الطلب رقم {UserOrder.Code}";
                    string Body = $"نعتذر ، قام العميل بالغاء طلب التوصيل رقم {UserOrder.Code}";
                    Notifications.SendWebNotification(Title, Body, "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", UserOrder.Id, NotificationType.WebStoreOrderDetailsPage,true);
                    await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, OrderId, true, true);
                    await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, OrderId, true, true);
                }
            }
            return Ok(baseResponse);
        }
        [HttpPost]
        [Route("checkout")]
        public async Task<IHttpActionResult> CheckOut(CheckOutStoreOrderDTO model)
        {
            CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.StoreOrders.Include(w => w.Driver).Include(w => w.Driver.User).Include(w => w.Items)
                .FirstOrDefault(w => w.UserId == CurrentUserId && w.Status == StoreOrderStatus.Initialized && !w.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            var item = UserOrder.Items.First();
            if (db.StoreOrderItems.Include(w => w.Product).Include(w => w.Product.Category).Include(w => w.Product.Category.Store)
                .Any(w => (w.Id == item.Id) && (w.Product.Category.Store.IsBlocked || w.Product.Category.Store.IsDeleted ||
            w.Product.Category.Store.IsHidden)))
            {
                baseResponse.ErrorCode = Errors.StoreIsClosed;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            var Validation = ValidateBasketItems(UserOrder);
            if (Validation != Errors.Success)
            {
                baseResponse.ErrorCode = Validation;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (UserOrder.UserAddressId.HasValue == false)
            {
                if (model.UserAddressId.HasValue == false || model.UserAddressId <= 0)
                {
                    baseResponse.ErrorCode = Errors.UserAddressIsRequired;
                    return Content(HttpStatusCode.BadRequest, baseResponse);
                }
                else
                {
                    var Address = db.UserAddresses.FirstOrDefault(s => s.IsDeleted == false && s.Id == model.UserAddressId && s.UserId == CurrentUserId);
                    if (Address == null)
                    {
                        baseResponse.ErrorCode = Errors.UserAddressNotFound;
                        return Content(HttpStatusCode.NotFound, baseResponse);
                    }
                    else
                    {
                        UserOrder.UserAddressId = model.UserAddressId;
                        db.SaveChanges();
                    }
                }
            }
            else
            {
                if (UserOrder.UserAddress.IsDeleted == true)
                {
                    baseResponse.ErrorCode = Errors.UserAddressNotFound;
                    return Content(HttpStatusCode.NotFound, baseResponse);
                }
            }

            if (model.PaymentMethod != PaymentMethod.Cash && model.PaymentMethod != PaymentMethod.Online && model.PaymentMethod != PaymentMethod.Wallet)
            {
                baseResponse.ErrorCode = Errors.InvalidPaymentMethod;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (model.PaymentMethod == PaymentMethod.Wallet && UserOrder.User.Wallet < UserOrder.Total)
            {
                baseResponse.ErrorCode = Errors.NotEnoughFund;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            UserOrder.IsPaid = false;
            UserOrder.CreatedOn = DateTime.Now.ToUniversalTime();
            if (model.PaymentMethod == PaymentMethod.Online)
            {
                UserOrder.Status = StoreOrderStatus.Initialized;
            }
            else
            {
                UserOrder.Status = StoreOrderStatus.Preparing;
            }
            UserOrder.PaymentMethod = model.PaymentMethod;
            db.SaveChanges();
            CRUD<StoreOrder>.Update(UserOrder);
            db.SaveChanges();

            var Item = UserOrder.Items.FirstOrDefault(s => s.IsDeleted == false);
            Store Store = null;
            if (Item != null)
            {
                Store = Item.Product.Category.Store;
                UserOrder.StoreProfit = Store.Profit;
            }
            db.SaveChanges();
            if (model.PaymentMethod != PaymentMethod.Online)
            {
                if (Store.DeliveryBySystem)
                {
                    var Drivers = StoreOrderActions.GetAvailableDrivers(Store, UserOrder.Id);
                    if (Drivers.Count > 0)
                    {
                        foreach (var driver in Drivers)
                        {
                            if ((db.Users.FirstOrDefault(w => w.Id == driver.UserId).Wallet) > 0)
                            {
                                await StoreOrderActions.AssignOrderToDriverAsync(UserOrder, driver);
                            }
                        }
                    }
                }
                var x = db.SaveChanges() > 0;
                var OrdersHub = GlobalHost.ConnectionManager.GetHubContext<OrdersHub>();
                OrdersHub.Clients.Group("StoreOrdersDashboard").add(UserOrder.Id);
                string Title = $"تم استقبال طلبكم";
                string Body = $"جارى الان تجهيز الطلب واعطائه لاقرب سائق متوفر";
                Notifications.SendWebNotification("تم أستقبال طلب جديد", Body, "6cbac676-8e4a-4e01-ace4-43814d464519", -1, "Amdin", UserOrder.Id, NotificationType.WebStoreOrderDetailsPage, true);
                await Notifications.SendToAllSpecificAndroidUserDevices(UserOrder.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, UserOrder.Id, false, true);
                await Notifications.SendToAllSpecificIOSUserDevices(UserOrder.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, UserOrder.Id, false, true);
                baseResponse.Data = model.PaymentMethod;
                await SMS.SendMessage("", "01024401763", "هناك طلب جديد للمتاجر من فضلك قم بزيارة لوحة التحكم");
                return Ok(baseResponse);
            }
            else
            {
                await SMS.SendMessage("", "01024401763", "هناك طلب جديد للمتاجر من فضلك قم بزيارة لوحة التحكم");
                var PaymentLink = await PaymentGateway.GenerateStoreOrderPaymentLink(UserOrder);
                baseResponse.Data = PaymentLink;
                return Ok(baseResponse);
            }

        }

        private Errors ValidateBasketItems(StoreOrder order)
        {
            CurrentUserId = User.Identity.GetUserId();
            foreach (var item in order.Items.Where(d => d.IsDeleted == false))
            {
                if (item.Product.IsDeleted == true || item.Product.Category.IsDeleted == true)
                    return Errors.ProductNotFound;

                if (item.Product.Category.Store.IsAccepted.HasValue == false || item.Product.Category.Store.IsAccepted == false || item.Product.Category.Store.IsDeleted == true || item.Product.Category.Store.IsHidden == true || item.Product.Category.Store.IsBlocked == true)
                {
                    item.IsDeleted = true;
                    StoreOrderActions.CalculateOrderPrice(order);
                    db.SaveChanges();
                    return Errors.StoreNotFound;
                }

                if (item.Product.Category.Store.IsOpen == false)
                {
                    return Errors.StoreIsClosed;
                }

                if (item.Product.Category.Store.Latitude.HasValue == false || item.Product.Category.Store.Longitude.HasValue == false)
                {
                    return Errors.StoreLocationIsNotDefined;
                }

                if (item.SizeId.HasValue == true)
                {
                    var Size = db.ProductSizes.FirstOrDefault(w => w.IsDeleted == false && w.Id == item.SizeId && w.ProductId == item.ProductId);
                    if (Size == null)
                    {
                        item.IsDeleted = true;
                        StoreOrderActions.CalculateOrderPrice(order);
                        db.SaveChanges();
                        return Errors.SizeNotFound;
                    }
                }
            }

            return Errors.Success;
        }

        [HttpGet]
        [Route("Details")]
        public IHttpActionResult GetOrderDetails(long OrderId, string lang = "en")
        {
            CurrentUserId = User.Identity.GetUserId();
            var Order = db.StoreOrders.FirstOrDefault(s => s.IsDeleted == false && s.Status != StoreOrderStatus.Initialized && s.Id == OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));

            DateTime? DeliverDate = null;
            if (Order.DeliveredOn.HasValue == true)
            {
                DeliverDate = TimeZoneInfo.ConvertTimeFromUtc(Order.CreatedOn, TimeZoneInfo.FindSystemTimeZoneById("Sudan Standard Time"));
            }
            UserStoreOrderDetailsDTO detailsDTO = new UserStoreOrderDetailsDTO()
            {
                OrderId = Order.Id,
                Code = Order.Code,
                CreateDate = CreatedOn.ToString("dd MMMM yyyy"),
                CreateTime = CreatedOn.ToString("hh:mm tt"),
                DeliverDate = DeliverDate.HasValue ? DeliverDate.Value.ToString("dd MMMM yyyy") : null,
                DeliverTime = DeliverDate.HasValue ? DeliverDate.Value.ToString("hh:mm tt") : null,
                PaymentMethod = Order.PaymentMethod,
                UserDriverRate = -1,
                UserStoreRate = -1,
            };
                switch (Order.PaymentMethod)
            {
                case PaymentMethod.Cash:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "الدفع عند الاستلام" : "Cash on Delivery";
                    detailsDTO.PaymentMethod = PaymentMethod.Cash;
                    break;
                case PaymentMethod.Online:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "اونلاين" : "Online";
                    detailsDTO.PaymentMethod = PaymentMethod.Online;
                    break;
                case PaymentMethod.Wallet:
                    detailsDTO.PaymentMethodText = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "من المحفظة" : "Wallet";
                    detailsDTO.PaymentMethod = PaymentMethod.Wallet;
                    break;
                default:
                    break;
            }

            if (Order.UserAddressId.HasValue == true)
            {
                detailsDTO.UserAddress = new UserAddressDTO()
                {
                    Address = Order.UserAddress.Address,
                    AddressInDetails = Order.UserAddress.AddressInDetails,
                    Apartment = Order.UserAddress.Apartment,
                    BuildingNumber = Order.UserAddress.BuildingNumber,
                    Floor = Order.UserAddress.Floor,
                    Id = Order.UserAddress.Id,
                    Latitude = Order.UserAddress.Latitude,
                    Longitude = Order.UserAddress.Longitude,
                    Name = Order.UserAddress.Name,
                    PhoneNumber = Order.UserAddress.PhoneNumber,
                };
            }

            if (Order.Items != null)
            {
                var Item = Order.Items.FirstOrDefault(w => w.IsDeleted == false);
                if (Item != null)
                {
                    var Store = Item.Product.Category.Store;
                    detailsDTO.StoreId = Store.Id;
                    detailsDTO.StoreName = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? Store.NameAr : Store.NameEn;
                    detailsDTO.StoreImageUrl = !string.IsNullOrEmpty(Store.LogoImageUrl) ? MediaControl.GetPath(FilePath.Store) + Store.LogoImageUrl : null;
                    StoreReview StoreReview = null;
                    if (Order.StoreReviews != null)
                    {
                        StoreReview = db.StoreReviews.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.StoreId == Store.Id && s.StoreOrderId == Order.Id);
                    }
                    if (Order.Status == StoreOrderStatus.Finished)
                    {
                        detailsDTO.CanReviewStore = StoreReview != null ? false : true;
                        detailsDTO.UserStoreReview = StoreReview != null ? StoreReview.Review : null;
                        detailsDTO.UserStoreRate = StoreReview != null ? StoreReview.Rate : -1;
                    }
                }
                var Product = new Product();
                foreach (var item in Order.Items.Where(d => d.IsDeleted == false))
                {
                    Product = db.Products.FirstOrDefault(x => x.Id == item.ProductId);
                    BasketItemDTO basketItemDTO = new BasketItemDTO()
                    {
                        Description = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.DescriptionAr : item.Product.DescriptionEn,
                        Name = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Product.NameAr : item.Product.NameEn,
                        BasketItemId = item.Id,
                        Image = item.Product.Images != null && item.Product.Images.FirstOrDefault(d => d.IsDeleted == false) != null ? MediaControl.GetPath(FilePath.Product) + item.Product.Images.FirstOrDefault(d => d.IsDeleted == false).ImageUrl : null,
                        Quantity = item.Quantity,
                    };
                    if (lang.ToLower() == "ar")
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال":Product.CurrencyAr );
                        basketItemDTO.SubTotal = item.SubTotal.ToString()+ " "+ (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        detailsDTO.Total = Order.Total.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        detailsDTO.SubTotal = Order.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                        detailsDTO.DeliveryFees = Order.DeliveryFees.ToString() + " " + (string.IsNullOrEmpty(Product.CurrencyAr) ? "ريال" : Product.CurrencyAr);
                    }
                    else
                    {
                        basketItemDTO.Price = item.Price.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency );
                        basketItemDTO.SubTotal = item.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        detailsDTO.Total = Order.Total.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        detailsDTO.SubTotal = Order.SubTotal.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                        detailsDTO.DeliveryFees = Order.DeliveryFees.ToString() + " " + (string.IsNullOrEmpty(Product.Currency) ? "LE" : Product.Currency);
                    }
                    if (item.SizeId.HasValue == true)
                    {
                        basketItemDTO.Size = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? item.Size.SizeAr : item.Size.SizeEn;
                    }
                    detailsDTO.Items.Add(basketItemDTO);
                }
            }

            if (Order.DriverId.HasValue == true)
            {
                DriverReview DriverReview = null;
                if (Order.DriverReviews != null)
                {
                    DriverReview = db.DriverReviews.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.DriverId == Order.DriverId && s.StoreOrderId == Order.Id);
                }
                if (Order.Status == StoreOrderStatus.Finished)
                {
                    detailsDTO.CanReviewDriver = DriverReview != null ? false : true;
                    detailsDTO.UserDriverReview = DriverReview != null ? DriverReview.Review : null;
                    detailsDTO.UserDriverRate = DriverReview != null ? DriverReview.Rate : -1;
                }
                detailsDTO.HasDriver = true;
                detailsDTO.getDriverOrdersCount = Order.Driver.NumberOfCompletedTrips;
                detailsDTO.DriverId = Order.DriverId;
                detailsDTO.DriverName = Order.Driver.User.Name;
                detailsDTO.DriverPhoneNumber = Order.Driver.User.PhoneNumber;
                detailsDTO.DriverImageUrl = !string.IsNullOrEmpty(Order.Driver.User.ImageUrl) ? MediaControl.GetPath(FilePath.Users) + Order.Driver.User.ImageUrl : null;
                detailsDTO.DriverRate = Order.Driver.Rate.ToString("N1");
            }
            switch (Order.Status)
            {
                case StoreOrderStatus.Placed:
                    detailsDTO.CanCancel = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جديد" : "New";
                    if (Order.DriverId.HasValue == true)
                    {
                        detailsDTO.CanCallDriver = true;
                    }
                    break;
                case StoreOrderStatus.Finished:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم التوصيل" : "Finished";
                    break;
                case StoreOrderStatus.Delivering:
                    detailsDTO.CanCancel = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التوصيل" : "Delivering";
                    if (Order.DriverId.HasValue == true)
                    {
                        detailsDTO.CanCallDriver = true;
                    }
                    break;
                case StoreOrderStatus.Cancelled:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "تم الالغاء" : "Cancelled";
                    break;
                case StoreOrderStatus.Preparing:
                    detailsDTO.CanCancel = true;
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "جارى التجهيز" : "Preparing";
                    if (Order.DriverId.HasValue == true)
                    {
                        detailsDTO.CanCallDriver = true;
                    }
                    break;
                case StoreOrderStatus.Rejected:
                    detailsDTO.OrderStatus = !string.IsNullOrEmpty(lang) && lang.ToLower() == "ar" ? "مرفوض" : "Rejected";
                    break;
                default:
                    break;
            }

            baseResponse.Data = detailsDTO;
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("RateDriver")]
        public async Task<IHttpActionResult> RateDriver(RateDriverDTO rateDriverDTO)
        {
            if (rateDriverDTO.Stars <= 0 || rateDriverDTO.Stars > 5)
            {
                baseResponse.ErrorCode = Errors.InvalidStars;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            string CurrentUserId = User.Identity.GetUserId();
            var Order = db.StoreOrders.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.Status == StoreOrderStatus.Finished && s.Id == rateDriverDTO.OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Driver = db.Drivers.Find(rateDriverDTO.DriverId);
            if (Driver == null)
            {
                baseResponse.ErrorCode = Errors.DriverNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            if (Order.DriverId != rateDriverDTO.DriverId)
            {
                baseResponse.ErrorCode = Errors.DriverDoesNotHaveTheRequiredOrder;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var DriverReview = db.DriverReviews.FirstOrDefault(s => s.DriverId == rateDriverDTO.DriverId && s.StoreOrderId == rateDriverDTO.OrderId && s.UserId == CurrentUserId);
            if (DriverReview != null)
            {
                baseResponse.ErrorCode = Errors.UserAlreadyRated;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            db.DriverReviews.Add(new DriverReview()
            {
                DriverId = rateDriverDTO.DriverId,
                StoreOrderId = rateDriverDTO.OrderId,
                UserId = CurrentUserId,
                Rate = rateDriverDTO.Stars,
                Review = rateDriverDTO.Review,
            });
            db.SaveChanges();

            Driver.Rate = db.DriverReviews.Where(s => s.IsDeleted == false && s.DriverId == Driver.Id).Average(w => w.Rate);
            db.SaveChanges();
            string Title = $"لديك تقييم جديد";
            string Body = $"قام العميل بتقييمك فى الطلب رقم {Order.Code}";
            await Notifications.SendToAllSpecificAndroidUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            await Notifications.SendToAllSpecificIOSUserDevices(Driver.UserId, Title, Body, NotificationType.ApplicationStoreOrderDetails, Order.Id, true, true);
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("RateStore")]
        public IHttpActionResult RateStore(RateStoreDTO rateStoreDTO)
        {
            if (rateStoreDTO.Stars <= 0 || rateStoreDTO.Stars > 5)
            {
                baseResponse.ErrorCode = Errors.InvalidStars;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            string CurrentUserId = User.Identity.GetUserId();
            var Order = db.StoreOrders.FirstOrDefault(s => s.IsDeleted == false && s.UserId == CurrentUserId && s.Status == StoreOrderStatus.Finished && s.Id == rateStoreDTO.OrderId);
            if (Order == null)
            {
                baseResponse.ErrorCode = Errors.OrderNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var Store = db.Stores.Find(rateStoreDTO.StoreId);
            if (Store == null)
            {
                baseResponse.ErrorCode = Errors.StoreNotFound;
                return Content(HttpStatusCode.NotFound, baseResponse);
            }

            var StoreReview = db.StoreReviews.FirstOrDefault(s => s.IsDeleted == false && s.StoreOrderId == rateStoreDTO.OrderId && s.UserId == CurrentUserId && s.StoreId == Store.Id);
            if (StoreReview != null)
            {
                baseResponse.ErrorCode = Errors.UserAlreadyRated;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            db.StoreReviews.Add(new StoreReview()
            {
                StoreOrderId = rateStoreDTO.OrderId,
                UserId = CurrentUserId,
                Rate = rateStoreDTO.Stars,
                Review = rateStoreDTO.Review,
                StoreId = Store.Id,
            });
            db.SaveChanges();

            Store.Rate = db.StoreReviews.Where(s => s.IsDeleted == false && s.StoreId == Store.Id).Average(w => w.Rate);
            db.SaveChanges();
            Notifications.SendWebNotification($"لديكم تقييم جديد من العميل صاحب الطلب رقم {Order.Code}", $"لديكم تقييم جديد من العميل صاحب الطلب رقم {Order.Code}", Id: Order.Id, notificationLinkType: NotificationType.WebStoreOrderDetailsPage, IsSaveInDatabase: true, StoreId: Store.Id);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("GetPaymentLink")]
        public async Task<IHttpActionResult> GetPaymentLink()
        {
            CurrentUserId = User.Identity.GetUserId();
            var UserOrder = db.StoreOrders.FirstOrDefault(x => x.UserId == CurrentUserId && x.Status == StoreOrderStatus.Initialized && !x.IsDeleted);
            if (UserOrder == null)
            {
                baseResponse.ErrorCode = Errors.UserBasketIsEmpty;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var Link = await PaymentGateway.GenerateStoreOrderPaymentLink(UserOrder);
            if (Link != null)
            {
                baseResponse.Data = new
                {
                    Link
                };
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }
    }
}