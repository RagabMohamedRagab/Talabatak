using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Talabatak.Models.DTOs
{
    public class UploadChatAttachmentFileDTO
    {
        public OrderType OrderType { get; set; }
        public bool IsDriver { get; set; }
        public long ChatId { get; set; }
        public string FileBase64 { get; set; }
        public string FileName { get; set; }
        public MediaType FileType { get; set; }
    }
}