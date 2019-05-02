using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using EBook_Reader.Models;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace EBook_Reader.Controllers
{
    public class FilesController : Controller
    {
        private readonly IHostingEnvironment hostingEnvironment_;
        private string webRootPath = null;
        private string filePath = null;
        public FilesController(IHostingEnvironment hostingEnvironment)
        {
            hostingEnvironment_ = hostingEnvironment;
            webRootPath = hostingEnvironment_.WebRootPath;
            filePath = Path.Combine(webRootPath, "FileStorage");
        }
        [HttpPost]
        public async Task<IActionResult> AddDocument(List<IFormFile> files)
        {
            HttpClient client = new HttpClient();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    byte[] bytes1;
                        using (var reader = new StreamReader(file.OpenReadStream()))
                        {
                            string contentAsString = reader.ReadToEnd();
                            bytes1 = new byte[contentAsString.Length * sizeof(char)];
                            System.Buffer.BlockCopy(contentAsString.ToCharArray(), 0, bytes1, 0, bytes1.Length);
                        }
                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    ByteArrayContent bytes = new ByteArrayContent(bytes1);
                    string fileName = file.FileName;
                    multiContent.Add(bytes, "files", fileName);
                    HttpResponseMessage message = await client.PostAsync("http://localhost:52464/api/files/", multiContent);
                    }
                }
            //return Ok();
            return RedirectToAction("/Home/HomePage");
        }
    }
}