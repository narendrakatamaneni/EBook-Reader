using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using EBook_Reader.Data;
using EBook_Reader.Helpher;
using EBook_Reader.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EBook_Reader.Controllers
{
    public class PublicController : Controller
    {

        private readonly ApplicationDbContext context_;
        private const string sessionId_ = "SessionId";
        APIHelper aPIHelper = new APIHelper();

        private readonly IHostingEnvironment hostingEnvironment_;
        private string webRootPath = null;
        private string filePath = null;

        public PublicController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            context_ = context;
            hostingEnvironment_ = hostingEnvironment;
            webRootPath = hostingEnvironment_.WebRootPath;
            filePath = Path.Combine(webRootPath, "FileStorage");
        }

        [HttpGet]
        public IActionResult addPublicDocument(int id)
        {
            var model = new PublicDocument();
            return View(model);
        }

        public IActionResult PublicHomePage()
        {
            return View(context_.PublicDocuments.ToList<PublicDocument>());
        }

        [HttpPost]
        public async Task<IActionResult> addPublicDocument(List<IFormFile> files)
        {
            //var request = HttpContext.Request;
            string sessionUserName = HttpContext.Session.GetString("SessionUserName");
            PublicDocument document = new PublicDocument();
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    document.DocumentName = file.FileName;
                    document.userName = sessionUserName;
                    document.UpdatedDate = DateTime.Today;
                    document.documentType = Path.GetExtension(file.FileName);
                }
            }
            context_.PublicDocuments.Add(document);
            context_.SaveChanges();

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
                    HttpResponseMessage message = await client.PostAsync("http://localhost:52464/api/files/publicdocument", multiContent);
                }
            }
            //return Ok();
            return RedirectToAction("PublicHomePage");
        }

        //----< shows details for each document >----------------------
        public ActionResult documentDetails(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            PublicDocument document = context_.PublicDocuments.Find(id);

            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            var lects = context_.PublicComments.Where(l => l.PublicDocument == document);

            document.PublicComments = lects.OrderBy(l => l.Comment).Select(l => l).ToList<PublicComments>();
            //course.Lectures = lects.ToList<Lecture>();

            if (document.PublicComments == null)
            {
                document.PublicComments = new List<PublicComments>();
                PublicComments lct = new PublicComments();
                lct.Comment = "none";
                lct.CommentDate = DateTime.MinValue;
                document.PublicComments.Add(lct);
            }
            return View(document);
        }

        public async Task<IActionResult> viewPublicDocument(int? id)
        {
            bool found = false;
            DocumentView documentView = new DocumentView();
            HttpClient client = new HttpClient();

            PublicDocument document = context_.PublicDocuments.Find(id);
            var filePath1 = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"..\..\..\..\wwwroot\FileStorage\";
            var files = Directory.GetFiles(filePath).ToList<string>();
            foreach (var item in files)
            {
                if (Path.GetFileName(item).Equals(document.DocumentName))
                {
                    found = true;
                    break;
                }
            }
            if (found)
            {
                documentView.filePath = Path.Combine("/FileStorage/", document.DocumentName);
            }
            else
            {
                using (var result = await client.GetAsync("http://localhost:52464/api/files" + "/" + document.DocumentName))
                {
                    if (result.IsSuccessStatusCode)
                    {
                        byte[] bytes = await result.Content.ReadAsByteArrayAsync();
                        // System.IO.File.WriteAllBytes(filePath, bytes);
                        var path = Path.Combine(filePath, document.DocumentName);
                        /*using (var fileStream = new FileStream(path, FileMode.Create))
                        {
                            //await System.IO.File.CopyToAsync(fileStream);
                            fileStream.Write(bytes, 0, bytes.Length);
                            fileStream.Flush();
                            fileStream.Close();
                            
                        }*/
                        FileStream fs = new FileStream(path, FileMode.OpenOrCreate);
                        //File(bytes, "application/pdf");
                        fs.Write(bytes, 0, bytes.Length);
                        fs.Close();
                    }

                }
                documentView.filePath = Path.Combine("/FileStorage/", document.DocumentName);
            }
            return View(documentView);
        }


        //----< gets form to edit a specific document via id >---------

        [HttpGet]
        public IActionResult editPublicDocument(int? id)
        {
            if (id == null)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            PublicDocument document = context_.PublicDocuments.Find(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View(document);
        }

        //----< posts back edited results for specific document >------

        [HttpPost]
        public IActionResult editPublicDocument(int? id, Document crs)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            var document = context_.PublicDocuments.Find(id);
            if (document != null)
            {
                document.DocumentName = crs.DocumentName;
                document.UpdatedDate = crs.UpdatedDate;
                try
                {
                    context_.SaveChanges();
                }
                catch (Exception)
                {
                    // do nothing for now
                }
            }
            return RedirectToAction("PublicHomePage");
        }
        //----< deletes a document by id >-----------------------------
        /*
         * - note that Delete does not send back a view, but
         *   simply redirects back to the Index view.
         */
        public IActionResult deletePublicDocument(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                var document = context_.PublicDocuments.Find(id);
                if (document != null)
                {
                    context_.Remove(document);
                    context_.SaveChanges();
                }
            }
            catch (Exception)
            {
                // nothing for now
            }
            return RedirectToAction("PublicHomePage");
        }

        //----< show list of lectures, ordered by Title >------------
        public IActionResult viewPublicComment()
        {
            var request = HttpContext.Request;
            string sessionUserName = HttpContext.Session.GetString("SessionUserName");

            // fluent API
            var lects = context_.PublicComments.Include(l => l.PublicDocument).Where(l => sessionUserName.Equals(l.userName));
            var orderedLects = lects.OrderBy(l => l.Comment)
              .OrderBy(l => l.PublicDocument)
              .Select(l => l);
            return View(orderedLects);
        }

        //----< shows form for creating a lecture >------------------

        [HttpGet]
        public IActionResult AddPublicComment(int id)
        {
            HttpContext.Session.SetInt32(sessionId_, id);

            // this works too
            // TempData[sessionId_] = id;

            PublicDocument course = context_.PublicDocuments.Find(id);
            if (course == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            PublicComments lct = new PublicComments();
            return View(lct);
        }

        //----< posts back new courses details >---------------------

        //----< Add new lecture to course >--------------------------

        [HttpPost]
        public IActionResult AddPublicComment(int? id, PublicComments lct)
        {
            var request = HttpContext.Request;
            string sessionUserName = HttpContext.Session.GetString("SessionUserName");

            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            // retreive the target course from static field

            int? courseId_ = HttpContext.Session.GetInt32(sessionId_);

            // this works too
            // int courseId_ = (int)TempData[sessionId_];

            var document = context_.PublicDocuments.Find(courseId_);

            if (document != null)
            {
                if (document.PublicComments == null)  // doesn't have any lectures yet
                {
                    List<PublicComments> comments = new List<PublicComments>();
                    document.PublicComments = comments;
                }
                lct.CommentDate = DateTime.Today;
                lct.userName = sessionUserName;

                document.PublicComments.Add(lct);

                try
                {
                    context_.SaveChanges();
                }
                catch (Exception)
                {
                    // do nothing for now
                }
            }
            return RedirectToAction("PublicHomePage");
        }


        //----< deletes a comment by id >-----------------------------
        /*
         * - note that Delete does not send back a view, but
         *   simply redirects back to the Index view, which 
         *   will not show the deleted comment.
         */
        public IActionResult deletePublicComment(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                var comment = context_.PublicComments.Find(id);
                if (comment != null)
                {
                    context_.Remove(comment);
                    context_.SaveChanges();
                }
            }
            catch (Exception)
            {
                // nothing for now
            }
            return RedirectToAction("PublicHomePage");
        }

        //----< gets form to edit a specific comment via id >---------

        [HttpGet]
        public IActionResult editPublicComment(int? id)
        {
            if (id == null)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            PublicComments comment = context_.PublicComments.Find(id);

            if (comment == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View(comment);
        }

        //----< posts back edited results for specific comment >------

        [HttpPost]
        public IActionResult editPublicComment(int? id, PublicComments lct)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            var comment = context_.PublicComments.Find(id);

            if (comment != null)
            {
                comment.Comment = lct.Comment;
                comment.CommentDate = lct.CommentDate;

                try
                {
                    context_.SaveChanges();
                }
                catch (Exception)
                {
                    // do nothing for now
                }
            }
            return RedirectToAction("PublicHomePage");
        }

    }
}