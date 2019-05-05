using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;

namespace RestAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IHostingEnvironment hostingEnvironment_;
        private string webRootPath = null;
        private string filePath = null;

        public FilesController(IHostingEnvironment hostingEnvironment)
        {
            hostingEnvironment_ = hostingEnvironment;
            webRootPath = hostingEnvironment_.ContentRootPath;
            filePath = Path.Combine(webRootPath, "Private FileStorage");
        }
        // GET: api/<controller>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            List<string> files = null;
            try
            {
                files = Directory.GetFiles(filePath).ToList<string>();
                for (int i = 0; i < files.Count; ++i)
                    files[i] = Path.GetFileName(files[i]);
            }
            catch
            {
                files = new List<string>();
                files.Add("404 - Not Found");
            }
            return files;
        }

        // GET api/<controller>/5
        [HttpGet("{name}")]
        public async Task<IActionResult> Download(string name)
        {
            List<string> files = null;
            string file = "";
            bool found = false;
            try
            {
                files = Directory.GetFiles(filePath).ToList<string>();
                foreach (var item in files)
                {
                    if (Path.GetFileName(item).Equals(name))
                    {
                        file = item;
                        found = true;
                    }
                }

                if (found)
                {
                    var memory = new MemoryStream();
                    //file = files[id];
                    using (var stream = new FileStream(file, FileMode.Open))
                    {
                        await stream.CopyToAsync(memory);
                    }
                    memory.Position = 0;
                    return File(memory, GetContentType(file), Path.GetFileName(file));
                }
                else
                {
                    return NotFound();
                }
            }
            catch
            {
                return NotFound();
            }

        }

        private string GetContentType(string path)
        {
            var types = GetMimeTypes();
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return types[ext];
        }

        private Dictionary<string, string> GetMimeTypes()
        {
            return new Dictionary<string, string>
      {
        {".cs", "application/C#" },
        {".txt", "text/plain"},
        {".pdf", "application/pdf"},
        {".doc", "application/vnd.ms-word"},
        {".docx", "application/vnd.ms-word"},
        {".xls", "application/vnd.ms-excel"},
        {".xlsx", "application/vnd.openxmlformatsofficedocument.spreadsheetml.sheet"},
        {".png", "image/png"},
        {".jpg", "image/jpeg"},
        {".jpeg", "image/jpeg"},
        {".gif", "image/gif"},
        {".csv", "text/csv"}
      };
        }

        [HttpPost]
        public async Task<IActionResult> Post(IList<IFormFile> files)
        {
            var dummy = HttpContext.Request;  // statement for debugging
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var path = Path.Combine(filePath, file.FileName);
                    if (file.Length > 0)
                    {
                        using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                    }
                }
            }
            return Ok();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> publicdocument(IList<IFormFile> files)
        {
            var dummy = HttpContext.Request;  // statement for debugging

            string filePath = Path.Combine(webRootPath, "Public FileStorage");
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var path = Path.Combine(filePath, file.FileName);
                    using (var fileStream = new FileStream(path, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }
                }
            }
            return Ok();
        }




        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
            // ToDo
        }

        // DELETE api/<controller>/5
        [HttpDelete("{name}")]
        public void Delete(string name)
        {
            try
            {
                // Check if file exists with its full path    
                if (System.IO.File.Exists(Path.Combine(filePath, name)))
                {
                    // If file found, delete it    
                    System.IO.File.Delete(Path.Combine(filePath, name));
                }
                else
                {

                }
            }
            catch (IOException ioExp)
            {
                Console.WriteLine(ioExp.Message);
            }
        }
        [Route("[action]")]
        [HttpDelete("{name}")]
        public void DeletePublicDocument(string name)
        {
            string filePath = Path.Combine(webRootPath, "Public FileStorage");
            try
            {
                // Check if file exists with its full path    
                if (System.IO.File.Exists(Path.Combine(filePath, name)))
                {
                    // If file found, delete it    
                    System.IO.File.Delete(Path.Combine(filePath, name));
                }
                else
                {

                }
            }
            catch (Exception)
            {

            }
        }
    }
}
