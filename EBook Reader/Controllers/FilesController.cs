using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using EBook_Reader.Models;

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
        public async Task<IActionResult> AddDocument()
        {
            var request = HttpContext.Request;
            foreach (var file in request.Form.Files)
            {
                if (file.Length > 0)
                {
                    var path = Path.Combine(filePath, file.FileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
                else
                {
                    return BadRequest();
                }
            }
            return Ok();
        }
    }
}