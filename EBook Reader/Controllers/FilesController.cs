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
        public IActionResult AddDocument()
        {
            var request = HttpContext.Request;
            foreach (var file in request.Form.Files)
            {
                if (file.Length > 0)
                {
                   
                    var filePath = Path.Combine(hostingEnvironment_.WebRootPath, "FileStorage");
                    String password = null;
                    var files = Directory.GetFiles(filePath).ToList<string>();
                    //PdfDocument pdfDocument = new PdfDocument(files[0], password);
                    //pdfDocument.ToJpegImages(filePath);
                }
                else
                {
                    return BadRequest();
                }
            }
            return RedirectToAction("HomePage");
        }
    }
}