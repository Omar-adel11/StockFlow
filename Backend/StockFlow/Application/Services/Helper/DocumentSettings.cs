using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using static System.Net.Mime.MediaTypeNames;

namespace Application.Services.Helper
{
    public static class DocumentSettings
    {
        public static string UploadFile(IFormFile file, string rootPath, string folderName)
        {
            //path to save the file
            //D:\WEB\2026\Luna App\Backend\LUNA\LUNA\wwwroot\files\images\
            string path = Path.Combine(rootPath, "files", folderName);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var fileName = Guid.NewGuid().ToString() + file.FileName;
            var filePath = Path.Combine(path, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return fileName;
        }

        public static void DeleteFile(string fileName, string rootPath, string folderName)
        {
            string path = Path.Combine(rootPath, "files", folderName);
            var filePath = Path.Combine(path, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
