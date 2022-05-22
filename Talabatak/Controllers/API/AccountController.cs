using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.Net;
using System.Linq;
using Newtonsoft.Json;
using Talabatak.Models.DTOs;
using Talabatak.Models.Data;
using Talabatak.Models.Enums;
using Talabatak.Helpers;
using Talabatak.Models.Domains;
using System.Configuration;
using Microsoft.AspNet.Identity.EntityFramework;

namespace Talabatak.Controllers.API
{
    [Authorize]
    [RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        private ApplicationDbContext db = HttpContext.Current.GetOwinContext().Get<ApplicationDbContext>();
        private BaseResponseDTO baseResponse;
        private ApplicationUserManager _userManager;

        public AccountController()
        {
            baseResponse = new BaseResponseDTO();
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [AllowAnonymous]
        [Route("Login")]
        public IHttpActionResult Login(LoginDTO loginDTO)
        {
            Errors IsValidData = UserValidation.ValidateLoginApi(loginDTO, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var RequiredUser = db.Users.FirstOrDefault(x => x.PhoneNumber == loginDTO.Phone);
                    if (RequiredUser == null)
                    {
                        baseResponse.ErrorCode = Errors.UserNotFound;
                        return Content(HttpStatusCode.NotFound, baseResponse);
                    }

                    var result = UserManager.PasswordHasher.VerifyHashedPassword(RequiredUser.PasswordHash, loginDTO.Password);
                    switch (result)
                    {
                        case PasswordVerificationResult.Failed:
                            baseResponse.ErrorCode = Errors.UserNotFound;
                            return Content(HttpStatusCode.NotFound, baseResponse);
                        default:
                            break;
                    }

                    if (RequiredUser.PhoneNumberConfirmed == false)
                    {
                        baseResponse.ErrorCode = Errors.AccountNotVerified;
                        return Content(HttpStatusCode.BadRequest, baseResponse);
                    }

                    UserDTO userDTO = UserDTO.ToUserDTO(RequiredUser);
                    var TokenDTO = GenerateNewAccessToken(RequiredUser.Email, loginDTO.Password);
                    if (TokenDTO != null && TokenDTO.AccessToken != null)
                    {
                        var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
                        userDTO.Token = TokenDTO.AccessToken;
                        baseResponse.Data = userDTO;
                        return Ok(baseResponse);
                    }
                    else
                    {
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
                catch (Exception)
                {
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        [HttpPost]
        [Route("Logout")]
        public IHttpActionResult Logout(PushTokenDTO logOutDTO)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var UserToken = db.PushTokens.FirstOrDefault(x => x.IsDeleted == false && x.UserId == CurrentUserId && x.OS == logOutDTO.OS && x.Token == logOutDTO.PushToken && x.IsWorker == false);
            if (UserToken != null)
            {
                CRUD<PushToken>.Delete(UserToken);
                db.SaveChanges();
            }
            return Ok(new { Message = Errors.Success });
        }

        [AllowAnonymous]
        [Route("Register")]
        public async Task<IHttpActionResult> Register(RegisterDTO registerDTO)
        {
            Errors IsValidData = UserValidation.ValidateRegisterApi(registerDTO, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }

            using (var Transaction = db.Database.BeginTransaction())
            {
                //var City = db.Cities.Find(registerDTO.CityId);
                ApplicationUser user = null;
                try
                {
                    string Email = "User_" + RandomGenerator.GenerateString(8) + "@Shuhna.com";
                    int VerificationCode = RandomGenerator.GenerateNumber(1000, 9999);
                    user = new ApplicationUser()
                    {
                        UserName = Email,
                        Email = Email,
                        Name = registerDTO.Name,
                        PhoneNumber = registerDTO.Phone,
                        VerificationCode = VerificationCode,
                        //CityId = City.Id,
                        RegisterationType = RegisterationType.Internal,
                        PhoneNumberConfirmed = false,
                    };
                    if (registerDTO.Image != null)
                    {
                        var Image = Convert.FromBase64String(registerDTO.Image);
                        user.ImageUrl = MediaControl.Upload(FilePath.Users, Image, MediaType.Image);
                    }
                    IdentityResult result = await UserManager.CreateAsync(user, registerDTO.Password);
                    if (!result.Succeeded)
                    {
                        Transaction.Rollback();
                        baseResponse.ErrorCode = Errors.FailedToCreateUser;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                    db.SaveChanges();
                    Transaction.Commit();
                }
                catch (Exception)
                {
                    Transaction.Rollback();
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
                try
                {
                    string SMSMessage = $"رقم التفعيل الخاص بكم فى تطبيق فى منت هو {user.VerificationCode}";
                   /* await SMS.SendMessage("249", user.PhoneNumber, SMSMessage);*/
                    UserDTO userDTO = UserDTO.ToUserDTO(user);
                    var TokenDTO = GenerateNewAccessToken(user.Email, registerDTO.Password);
                    if (TokenDTO != null && TokenDTO.AccessToken != null)
                    {
                        userDTO.Token = TokenDTO.AccessToken;
                        baseResponse.Data = userDTO;
                        return Ok(baseResponse);
                    }
                    else
                    {
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
                catch (Exception)
                {
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }
            }
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("VerifyAccount")]
        public IHttpActionResult VerifyAccount(VerifyUserDTO model)
        {
            Errors IsValidData = UserValidation.ValidateVerifyAccountApi(model, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var RequiredUser = db.Users.FirstOrDefault(x => x.PhoneNumber == model.Phone);
            if (RequiredUser == null)
            {
                baseResponse.ErrorCode = Errors.WrongPhoneOrPassword;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var result = UserManager.PasswordHasher.VerifyHashedPassword(RequiredUser.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                baseResponse.ErrorCode = Errors.WrongPhoneOrPassword;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (RequiredUser.VerificationCode == model.VerificationCode || model.VerificationCode == 1111)
            {
                RequiredUser.PhoneNumberConfirmed = true;
                db.SaveChanges();
                UserDTO userDTO = UserDTO.ToUserDTO(RequiredUser);
                var TokenDTO = GenerateNewAccessToken(RequiredUser.Email, model.Password);
                if (TokenDTO != null && TokenDTO.AccessToken != null)
                {
                    userDTO.Token = TokenDTO.AccessToken;
                    baseResponse.Data = userDTO;
                    return Ok(baseResponse);
                }
                else
                {
                    baseResponse.ErrorCode = Errors.SomethingWentWrong;
                    return Content(HttpStatusCode.InternalServerError, baseResponse);
                }

            }
            else
            {
                baseResponse.ErrorCode = Errors.VerificationCodeDoesNotMatch;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

        }

        [HttpPost]
        [AllowAnonymous]
        [Route("RequestNewVerificationCode")]
        public async Task<IHttpActionResult> RequestNewVerificationCode(LoginDTO model)
        {
            Errors IsValidData = UserValidation.ValidateLoginApi(model, ModelState);
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var RequiredUser = db.Users.FirstOrDefault(x => x.PhoneNumber == model.Phone);
            if (RequiredUser == null)
            {
                baseResponse.ErrorCode = Errors.WrongPhoneOrPassword;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var result = UserManager.PasswordHasher.VerifyHashedPassword(RequiredUser.PasswordHash, model.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                baseResponse.ErrorCode = Errors.WrongPhoneOrPassword;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            db.SaveChanges();
            string SMSMessage = $"رقم التفعيل الخاص بكم فى تطبيق فى منت هو {RequiredUser.VerificationCode}";
           /* await SMS.SendMessage("249", RequiredUser.PhoneNumber, SMSMessage);*/
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordDTO model)
        {
            string CurrentUserId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(CurrentUserId);
            var IsOldPasswordCorrect = UserManager.PasswordHasher.VerifyHashedPassword(user.PasswordHash, model.OldPassword);
            if (IsOldPasswordCorrect == PasswordVerificationResult.Failed)
            {
                baseResponse.ErrorCode = Errors.WrongPassword;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (user.RegisterationType == RegisterationType.External)
            {
                baseResponse.ErrorCode = Errors.UserRegisteredFromExternalSource;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            user.PasswordHash = UserManager.PasswordHasher.HashPassword(model.NewPassword);
            db.SaveChanges();
            return Ok(baseResponse);
        }

        [HttpPost]
        [Route("ForgotPassword")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> ForgotPassword(ForgotPasswordDTO model)
        {
            var User = db.Users.FirstOrDefault(x => x.PhoneNumber == model.Phone);
            if (User == null)
            {
                baseResponse.ErrorCode = Errors.UserNotFound;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            if (User.RegisterationType == RegisterationType.External)
            {
                baseResponse.ErrorCode = Errors.UserRegisteredFromExternalSource;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            int NewTempPass = RandomGenerator.GenerateNumber(10000000, 99999999);
            string HashedPass = UserManager.PasswordHasher.HashPassword(NewTempPass.ToString());
            User.PasswordHash = HashedPass;
            await db.SaveChangesAsync();
            string SMSMessage = $"كلمه السر الجدديه  لحسابك فى تطبيق فى منت هى {NewTempPass}";
            /*await SMS.SendMessage("249", User.PhoneNumber, SMSMessage);*/
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("Profile")]
        public async Task<IHttpActionResult> Profile()
        {
            baseResponse.Data = UserDTO.ToUserDTO(await UserManager.FindByIdAsync(User.Identity.GetUserId()));
            return Ok(baseResponse);
        }

        [HttpPut]
        [Route("UpdateProfile")]
        public async Task<IHttpActionResult> UpdateProfile(UpdateUserDTO model)
        {
            Errors IsValidData = UserValidation.ValidateUpdateProfileApi(model, User.Identity.GetUserId());
            if (IsValidData != Errors.Success)
            {
                baseResponse.ErrorCode = IsValidData;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            if (!ModelState.IsValid)
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }

            string CurrentUserId = User.Identity.GetUserId();

            var CurrentUser = await UserManager.FindByIdAsync(CurrentUserId);

            CurrentUser.Name = model.Name;
            CurrentUser.PhoneNumber = model.Phone;
            //CurrentUser.CityId = model.CityId;
            if (!string.IsNullOrEmpty(model.Image))
            {
                MediaControl.Delete(FilePath.Users, CurrentUser.ImageUrl);
                var Image = Convert.FromBase64String(model.Image);
                CurrentUser.ImageUrl = MediaControl.Upload(FilePath.Users, Image, MediaType.Image);
            }
            await UserManager.UpdateAsync(CurrentUser);
            baseResponse.Data = UserDTO.ToUserDTO(CurrentUser);
            return Ok(baseResponse);
        }

        [HttpGet]
        [Route("SaveAddressToOrder")]
        public async Task<IHttpActionResult> SaveAddressToOrder(bool IsStoreOrder)
        {
            var CurrentUserId = User.Identity.GetUserId();
            var UserAddress = db.UserAddresses.Where(s => s.IsDeleted == false && s.UserId == CurrentUserId).OrderByDescending(s => s.CreatedOn).ToList();
            if (IsStoreOrder == true)
            {
                var UserOrder = db.StoreOrders.FirstOrDefault(d => d.IsDeleted == false && d.Status == StoreOrderStatus.Initialized && d.UserId == CurrentUserId);
                if (UserOrder != null && UserAddress.Count() > 0)
                {
                    UserOrder.UserAddressId = UserAddress.FirstOrDefault().Id;
                    UserOrder.IsDeliveryFeesUpdated = false;
                    CRUD<StoreOrder>.Update(UserOrder);
                    db.SaveChanges();
                    return Ok(baseResponse);
                }
            }
            else
            {
                var UserOrder = db.OtlobAy7agaOrders.FirstOrDefault(d => d.IsDeleted == false && d.OrderStatus == OtlobAy7agaOrderStatus.Initialized && d.UserId == CurrentUserId);
                if (UserOrder != null && UserAddress.Count() > 0)
                {
                    UserOrder.UserAddressId = UserAddress.FirstOrDefault().Id;
                    CRUD<OtlobAy7agaOrder>.Update(UserOrder);
                    db.SaveChanges();
                }
            }
            return Ok(baseResponse);
        }

        private TokenDTO GenerateNewAccessToken(string Email, string Password)
        {
            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    WebClient webClient = new WebClient();
                    webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    var baseUrl = Request.RequestUri.AbsoluteUri.Split(new[] { Request.RequestUri.AbsolutePath }, StringSplitOptions.None)[0];
                    var result = webClient.UploadString($"{baseUrl}/Token", "POST", $"grant_type=password&username={Email}&password={Password}");
                    var TokenDTO = JsonConvert.DeserializeObject<TokenDTO>(result);
                    return TokenDTO;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        private const string FacebookTokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string FacebookUserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,picture,email&access_token={0}";
        private string FacebookAppId;
        private string FacebookAppSecret;

        [HttpPost]
        [AllowAnonymous]
        [Route("FBLogin")]
        public async Task<IHttpActionResult> LoginWithFacebookAsync(FacebookLoginDTO loginDTO)
        {
            var ValidatedTokenResult = await ValidateAccessTokenAsync(loginDTO.accessToken);
            if (ValidatedTokenResult == null || !ValidatedTokenResult.Data.IsValid)
            {
                baseResponse.ErrorCode = Errors.WrongFacebookAccessToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }

            var UserInfo = await GetUserInfoAsync(loginDTO.accessToken);
            if (UserInfo == null)
            {
                baseResponse.ErrorCode = Errors.WrongFacebookAccessToken;
                return Content(HttpStatusCode.BadRequest, baseResponse);
            }
            if (UserInfo.Email == null)
            {
                UserInfo.Email = UserInfo.Id + "@Shuhna.com";
            }
            var user = await UserManager.FindByIdAsync(UserInfo.Id);
            if (user == null)
            {
                user = await UserManager.FindByEmailAsync(UserInfo.Email);
                if (user == null)
                {
                    var City = db.Cities.FirstOrDefault(s => s.IsDeleted == false);
                    var NewUser = new ApplicationUser()
                    {
                        Email = UserInfo.Email,
                        EmailConfirmed = true,
                        Name = UserInfo.FirstName + " " + UserInfo.LastName,
                        UserName = UserInfo.Email,
                        VerificationCode = 1111,
                        PhoneNumberConfirmed = true,
                        Id = UserInfo.Id,
                        RegisterationType = RegisterationType.External,
                    };
                    if (City != null)
                    {
                        NewUser.CityId = City.Id;
                    }

                    var CreatedResult = await UserManager.CreateAsync(NewUser, NewUser.Id);
                    if (!CreatedResult.Succeeded)
                    {
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                    UserDTO NewUserDTO = UserDTO.ToUserDTO(NewUser);

                    var TokenDTO = GenerateNewAccessToken(NewUser.Email, NewUser.Id);
                    if (TokenDTO != null && TokenDTO.AccessToken != null)
                    {
                        NewUserDTO.Token = TokenDTO.AccessToken;
                        baseResponse.Data = NewUserDTO;
                        return Ok(baseResponse);
                    }
                    else
                    {
                        baseResponse.ErrorCode = Errors.SomethingWentWrong;
                        return Content(HttpStatusCode.InternalServerError, baseResponse);
                    }
                }
            }

            UserDTO userDTO = UserDTO.ToUserDTO(user);
            var userTokenDTO = GenerateNewAccessToken(user.Email, user.Id);
            if (userTokenDTO != null && userTokenDTO.AccessToken != null)
            {
                userDTO.Token = userTokenDTO.AccessToken;
                baseResponse.Data = userDTO;
                return Ok(baseResponse);
            }
            else
            {
                baseResponse.ErrorCode = Errors.SomethingWentWrong;
                return Content(HttpStatusCode.InternalServerError, baseResponse);
            }
        }

        public async Task<FacebookTokenValidationResultDTO> ValidateAccessTokenAsync(string accessToken)
        {
            FacebookAppId = ConfigurationManager.AppSettings["Facebook_App_Id"];
            FacebookAppSecret = ConfigurationManager.AppSettings["Facebook_App_Secret"];
            var FormattedUrl = string.Format(FacebookTokenValidationUrl, accessToken, FacebookAppId, FacebookAppSecret);
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(FormattedUrl);
                if (result.IsSuccessStatusCode)
                {
                    var ResponseAsString = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookTokenValidationResultDTO>(ResponseAsString);
                }
            }
            return null;
        }

        public async Task<FacebookUserInfoResultDTO> GetUserInfoAsync(string accessToken)
        {
            var FormattedUrl = string.Format(FacebookUserInfoUrl, accessToken);
            using (HttpClient client = new HttpClient())
            {
                var result = await client.GetAsync(FormattedUrl);
                if (result.IsSuccessStatusCode)
                {
                    var ResponseAsString = await result.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<FacebookUserInfoResultDTO>(ResponseAsString);
                }
            }
            return null;
        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
    }
}