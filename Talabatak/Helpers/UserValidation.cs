using Talabatak.Models.Data;
using Talabatak.Models.DTOs;
using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http.ModelBinding;

namespace Talabatak.Helpers
{
    public static class UserValidation
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static string EmailSyntax = @"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?";
        //private static string EgyptPhoneNumberSyntax = @"^01[0-3|5]{1}[0-9]{8}";
        public static bool IsEmail(string Email)
        {
            bool isEmail = Regex.IsMatch(Email, EmailSyntax, RegexOptions.IgnoreCase);
            return isEmail;
        }

        public static bool IsEmailExists(string Email, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower());
            else
                return db.Users.Any(x => x.Email.ToLower() == Email.ToLower() && x.Id != CurrentUserId);
        }

        //public static bool IsIsEgyptianPhoneNumber(string PhoneNumber)
        //{
        //    bool IsPhoneNumber = Regex.IsMatch(PhoneNumber, EgyptPhoneNumberSyntax, RegexOptions.IgnoreCase);
        //    if (IsPhoneNumber)
        //    {
        //        if (PhoneNumber.Length != 11)
        //        {
        //            IsPhoneNumber = false;
        //        }
        //    }
        //    return IsPhoneNumber;
        //}

        public static bool IsPhoneExists(string Phone, string CurrentUserId = null)
        {
            if (string.IsNullOrEmpty(CurrentUserId))
                return db.Users.Any(x => x.PhoneNumber == Phone);
            else
                return db.Users.Any(x => x.PhoneNumber == Phone && x.Id != CurrentUserId);
        }

        public static Errors ValidateLoginApi(LoginDTO loginDTO, ModelStateDictionary Model)
        {
            if (loginDTO == null)
            {
                return Errors.LoginDataIsRequired;
            }
            var ModelErrors = Model.Values.SelectMany(v => v.Errors.Where(x => !string.IsNullOrEmpty(x.ErrorMessage))).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the phone field is required"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
                else if (ModelErrors[i].ToLower().Contains("the password field is required"))
                {
                    return Errors.PasswordIsRequiredOrMinimumLength;
                }
                else if (ModelErrors[i].ToLower().Contains("the field password must be a string with a minimum length"))
                {
                    return Errors.PasswordIsRequiredOrMinimumLength;
                }
                else
                    return Errors.SomethingWentWrong;
            }
            if (ModelErrors == null || ModelErrors.Count <= 0)
            {
                var IsValidPhoneNumber = loginDTO.Phone.All(char.IsDigit);
                if (IsValidPhoneNumber == false || loginDTO.Phone.StartsWith("+"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
            }

            return Errors.Success;
        }

        public static Errors ValidateVerifyAccountApi(VerifyUserDTO model, ModelStateDictionary Model)
        {
            var ModelErrors = Model.Values.SelectMany(v => v.Errors.Where(x => !string.IsNullOrEmpty(x.ErrorMessage))).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the phone field is required"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
                else if (ModelErrors[i].ToLower().Contains("the verificationcode field is required"))
                {
                    return Errors.VerificationCodeDoesNotMatch;
                }
                else if (ModelErrors[i].ToLower().Contains("the password field is required"))
                {
                    return Errors.PasswordIsRequiredOrMinimumLength;
                }
                else if (ModelErrors[i].ToLower().Contains("the field password must be a string with a minimum length"))
                {
                    return Errors.PasswordIsRequiredOrMinimumLength;
                }
                else
                    return Errors.SomethingWentWrong;
            }
            if (ModelErrors == null || ModelErrors.Count <= 0)
            {
                var IsValidPhoneNumber = model.Phone.All(char.IsDigit);
                if (IsValidPhoneNumber == false || model.Phone.StartsWith("+"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
            }

            if (model.VerificationCode.ToString().Length < 4 || model.VerificationCode.ToString().Length > 4)
            {
                return Errors.WrongVerificationCode;
            }

            return Errors.Success;
        }

        public static Errors ValidateRegisterApi(RegisterDTO registerDTO, ModelStateDictionary Model)
        {
            if (registerDTO == null)
            {
                return Errors.RegisterDataIsRequired;
            }

            if (!string.IsNullOrEmpty(registerDTO.Phone))
            {
                var IsValidPhoneNumber = registerDTO.Phone.All(char.IsDigit);
                if (IsValidPhoneNumber == false || registerDTO.Phone.StartsWith("+"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
            }
            else
            {
                return Errors.PhoneIsRequiredAndInCorrectFormat;
            }

            var ModelErrors = Model.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            for (int i = 0; i < ModelErrors.Count; i++)
            {
                if (ModelErrors[i].ToLower().Contains("the name field is required"))
                    return Errors.NameIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the email field is required"))
                    return Errors.EmailIsRequiredAndInCorrectFormat;

                else if (ModelErrors[i].ToLower().Contains("the email field is not a valid e-mail address"))
                    return Errors.EmailIsRequiredAndInCorrectFormat;

                else if (ModelErrors[i].ToLower().Contains("the password field is required"))
                    return Errors.PasswordIsRequiredOrMinimumLength;

                else if (ModelErrors[i].ToLower().Contains("the password must be at least 6 characters long"))
                    return Errors.PasswordIsRequiredOrMinimumLength;

                else if (ModelErrors[i].ToLower().Contains("the cityid field is required"))
                    return Errors.CityFieldIsRequired;

                else if (ModelErrors[i].ToLower().Contains("the phone field is required"))
                    return Errors.PhoneIsRequiredAndInCorrectFormat;

                else
                    return Errors.SomethingWentWrong;
            }

            //var City = db.Cities.FirstOrDefault(s => s.IsDeleted == false && s.Country.IsDeleted == false && s.Id == registerDTO.CityId);
            //if (City == null)
            //{
            //    return Errors.CityNotFound;
            //}
            //else
            //{
            //    if (City.Country.NameAr.ToLower() == "مصر" || City.Country.NameEn.ToLower() == "egypt")
            //    {
            //        if (IsIsEgyptianPhoneNumber(registerDTO.Phone) == false)
            //        {
            //            return Errors.PhoneIsRequiredAndInCorrectFormat;
            //        }
            //    }
            //}

            //if (IsIsEgyptianPhoneNumber(registerDTO.Phone) == false)
            //{
            //    return Errors.PhoneIsRequiredAndInCorrectFormat;
            //}

            //if (IsEmailExists(registerDTO.Email))
            //    return Errors.EmailExists;

            if (IsPhoneExists(registerDTO.Phone))
                return Errors.MobileExists;

            if (!string.IsNullOrEmpty(registerDTO.Image))
            {
                try
                {
                    var Image = Convert.FromBase64String(registerDTO.Image);
                    if (Image == null || Image.Length <= 0)
                    {
                        return Errors.FailedToUpload;
                    }
                }
                catch (Exception)
                {
                    return Errors.FailedToUpload;
                }

            }
            return Errors.Success;
        }

        public static Errors ValidateUpdateProfileApi(UpdateUserDTO updateProfileDTO, string currentUserId)
        {
            if (string.IsNullOrEmpty(updateProfileDTO.Phone))
            {
                return Errors.PhoneIsRequiredAndInCorrectFormat;
            }
            else
            {
                var IsValidPhoneNumberOne = updateProfileDTO.Phone.All(char.IsDigit);
                if (IsValidPhoneNumberOne == false || updateProfileDTO.Phone.StartsWith("+"))
                {
                    return Errors.PhoneIsRequiredAndInCorrectFormat;
                }
            }

            //if (string.IsNullOrEmpty(updateProfileDTO.Email))
            //{
            //    return Errors.EmailIsRequiredAndInCorrectFormat;
            //}
            //else
            //{
            //    var IsValidEmail = IsEmail(updateProfileDTO.Email);
            //    if (IsValidEmail == false)
            //    {
            //        return Errors.EmailIsRequiredAndInCorrectFormat;
            //    }
            //}

            if (string.IsNullOrEmpty(updateProfileDTO.Name))
            {
                return Errors.NameIsRequired;
            }

            if (!string.IsNullOrEmpty(updateProfileDTO.Image))
            {
                try
                {
                    var Image = Convert.FromBase64String(updateProfileDTO.Image);
                    if (Image == null || Image.Length <= 0)
                    {
                        return Errors.FailedToUpload;
                    }
                }
                catch (Exception)
                {
                    return Errors.FailedToUpload;
                }
            }

            //if (IsEmailExists(updateProfileDTO.Email, currentUserId) == true)
            //{
            //    return Errors.EmailExists;
            //}

            if (IsPhoneExists(updateProfileDTO.Phone, currentUserId) == true)
            {
                return Errors.MobileExists;
            }

            //var City = db.Cities.FirstOrDefault(w => w.IsDeleted == false && w.Country.IsDeleted == false && w.Id == updateProfileDTO.CityId);
            //if (City == null)
            //{
            //    return Errors.CityNotFound;
            //}

            return Errors.Success;
        }
    }
}