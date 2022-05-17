using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class BaseResponseDTO<T> : BaseResponseDTO where T : new()
    {
        public T Data { get; set; }

    }
    public class BaseResponseDTO
    {
        private Errors Error = Errors.Success;
        public Errors ErrorCode
        {
            get
            {
                return Error;
            }
            set
            {
                Error = value;
                ErrorMessage = value.ToString();
            }
        }
        public string ErrorMessage { get; set; } = Errors.Success.ToString();
        public object Data { get; set; }
    }
}