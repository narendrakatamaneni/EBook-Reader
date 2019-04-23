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
            var request = HttpContext.Request;
            HttpClient client = new HttpClient();
            foreach (var file in request.Form.Files)
            {
                if (file.Length > 0)
                {
                    //HttpClient client = aPIHelper.initial();
                    MultipartFormDataContent multiContent = new MultipartFormDataContent();
                    //var path = Path.Combine(filePath,file.FileName);
                    byte[] data =System.IO.File.ReadAllBytes(Path.Combine(filePath, file.FileName));
                    ByteArrayContent bytes = new ByteArrayContent(data);
                    string fileName = file.FileName;
                    multiContent.Add(bytes, "files", fileName);
                    HttpResponseMessage message = await client.PostAsync("http://localhost:52464/api/files/", multiContent);
                    }
                }
            return Ok();
        }
    }
}