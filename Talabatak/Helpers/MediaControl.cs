using Talabatak.Models.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Talabatak.Helpers
{
    public class MediaControl
    {
        public static string Upload(FilePath filePath, HttpPostedFileBase File)
        {
            string FolderPath = string.Empty;
            if (File != null)
            {
                string FileExtension = Path.GetExtension(File.FileName);
                string Name = Guid.NewGuid().ToString() + FileExtension;
                switch (filePath)
                {
                    case FilePath.Category:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Categories");
                        break;
                    case FilePath.Job:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Jobs");
                        break;
                    case FilePath.Product:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Products");
                        break;
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    case FilePath.Store:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Stores");
                        break;
                    case FilePath.Country:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Countries");
                        break;
                    case FilePath.Slider:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Slider");
                        break;
                    case FilePath.Other:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Other");
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(FolderPath))
                {
                    if (!Directory.Exists(FolderPath))
                        Directory.CreateDirectory(FolderPath);

                    File.SaveAs(Path.Combine(FolderPath, Name));
                    return Name;
                }
                return null;
            }
            return null;
        }

        public static string Upload(FilePath filePath, byte[] FileBytes, MediaType mediaType)
        {
            string FolderPath = string.Empty;
            string FileName = string.Empty;
            if (FileBytes != null && FileBytes.Length > 0)
            {
                switch (mediaType)
                {
                    case MediaType.Image:
                        FileName = Guid.NewGuid().ToString() + ".jpg";
                        break;
                    case MediaType.Excel:
                        FileName = Guid.NewGuid().ToString() + ".xlsx";
                        break;
                    case MediaType.PDF:
                        FileName = Guid.NewGuid().ToString() + ".pdf";
                        break;
                    case MediaType.Video:
                        FileName = Guid.NewGuid().ToString() + ".mp4";
                        break;
                    case MediaType.Word:
                        FileName = Guid.NewGuid().ToString() + ".docx";
                        break;
                    default:
                        break;
                }
                switch (filePath)
                {
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    case FilePath.Other:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Other");
                        break;
                    case FilePath.Job:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Jobs");
                        break;
                    default:
                        break;
                }
                if (!string.IsNullOrEmpty(FolderPath))
                {
                    if (!Directory.Exists(FolderPath))
                        Directory.CreateDirectory(FolderPath);
                    File.WriteAllBytes(Path.Combine(FolderPath, FileName), FileBytes);
                    return FileName;
                }
                return null;
            }
            return null;
        }

        public static void Delete(FilePath filePath, string FileName)
        {
            string FolderPath = string.Empty;
            if (FileName != null)
            {
                switch (filePath)
                {
                    case FilePath.Category:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Categories");
                        break;
                    case FilePath.Job:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Jobs");
                        break;
                    case FilePath.Product:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Products");
                        break;
                    case FilePath.Users:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Users");
                        break;
                    case FilePath.Store:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Stores");
                        break;
                    case FilePath.Country:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Countries");
                        break;
                    case FilePath.Other:
                        FolderPath = HttpContext.Current.Server.MapPath("~/Content/Images/Other");
                        break;
                    default:
                        break;
                }
                if (FolderPath != null)
                {
                    string FullPath = Path.Combine(FolderPath, FileName);
                    if (File.Exists(FullPath))
                        File.Delete(FullPath);
                }
            }
        }

        public static string GetPath(FilePath filePath)
        {
            switch (filePath)
            {
                case FilePath.Category:
                    return "/Content/Images/Categories/";
                case FilePath.Job:
                    return "/Content/Images/Jobs/";
                case FilePath.Product:
                    return "/Content/Images/Products/";
                case FilePath.Users:
                    return "/Content/Images/Users/";
                case FilePath.Store:
                    return "/Content/Images/Stores/";
                case FilePath.Country:
                    return "/Content/Images/Countries/";
                case FilePath.Other:
                    return "/Content/Images/Other/";
                default:
                    return null;
            }
        }

        public static string CopyFile(string FileName, FilePath FromFilePath, FilePath ToFilePath, MediaType MediaType)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                string NewFileName = null;
                string ToFullPath = null;
                string FromFullPath = null;
                switch (MediaType)
                {
                    case MediaType.Image:
                        NewFileName = Guid.NewGuid().ToString() + ".jpg";
                        break;
                    case MediaType.Excel:
                        NewFileName = Guid.NewGuid().ToString() + ".xlsx";
                        break;
                    case MediaType.PDF:
                        NewFileName = Guid.NewGuid().ToString() + ".pdf";
                        break;
                    case MediaType.Video:
                        NewFileName = Guid.NewGuid().ToString() + ".mp4";
                        break;
                    case MediaType.Word:
                        NewFileName = Guid.NewGuid().ToString() + ".docx";
                        break;
                    default:
                        break;
                }
                switch (ToFilePath)
                {
                    case FilePath.Category:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Categories"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Categories"), FileName);
                        break;
                    case FilePath.Job:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Jobs"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Jobs"), FileName);
                        break;
                    case FilePath.Product:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Products"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Products"), FileName);
                        break;
                    case FilePath.Users:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Users"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Users"), FileName);
                        break;
                    case FilePath.Store:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Stores"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Stores"), FileName);
                        break;
                    case FilePath.Country:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Countries"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Countries"), FileName);
                        break;
                    case FilePath.Other:
                        ToFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Other"), NewFileName);
                        FromFullPath = Path.Combine(HttpContext.Current.Server.MapPath("~/Content/Images/Other"), FileName);
                        break;
                    default:
                        break;
                }
                if (File.Exists(FromFullPath))
                {
                    File.Copy(FromFullPath, ToFullPath);
                    return NewFileName;
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        public static string ConvertImageToBase64(HttpPostedFileBase image)
        {
            if (image != null)
            {
                Stream fs = image.InputStream;
                BinaryReader br = new BinaryReader(fs);
                Byte[] bytes = br.ReadBytes((int)fs.Length);
                return Convert.ToBase64String(bytes, 0, bytes.Length);
            }
            else
                return null;
        }

        internal static string Upload(FilePath users, object identityPhoto)
        {
            throw new NotImplementedException();
        }
    }

    public enum FilePath
    {
        Category,
        Product,
        Users,
        Store,
        Country,
        Other,
        Job,
        Slider,
    }
}
