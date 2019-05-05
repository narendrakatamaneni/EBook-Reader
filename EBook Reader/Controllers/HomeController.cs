using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using EBook_Reader.Models;
using EBook_Reader.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Reflection;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;
using System.Text;
using EBook_Reader.Helpher;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace EBook_Reader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context_;
        private const string sessionId_ = "SessionId";
        APIHelper aPIHelper = new APIHelper();

        private readonly IHostingEnvironment hostingEnvironment_;
        private string webRootPath = null;
        private string filePath = null;

        public HomeController(ApplicationDbContext context,IHostingEnvironment hostingEnvironment)
        {
            context_ = context;
            hostingEnvironment_ = hostingEnvironment;
            webRootPath = hostingEnvironment_.WebRootPath;
            filePath = Path.Combine(webRootPath, "FileStorage");
        }
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Index()
        {
            //return View(context_.Documents.ToList<Document>());
            return View();
        }

        //public IActionResult Login()
        //{
        //    return RedirectToPage("/Areas/Identity/Pages/Account/Login.cshtml");
        //}
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult HomePage()
        {
            return View(context_.Documents.ToList<Document>());
        }

        public IActionResult Privacy()
        {
            return View();
        }
        [Authorize]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult siteMap()
        {
            return View();
        }

        //----< displays form for Adding a new document >----------------
        [HttpGet]
        public IActionResult addDocument(int id)
        {
            var model = new Document();
            return View(model);
        }

     


        //----< posts back new document details >---------------------

        /* [HttpPost]
         public  async Task<IActionResult> addDocument()
         {
             //var request = HttpContext.Request;
             //string sessionUserName = HttpContext.Session.GetString("SessionUserName");
             //foreach (var file in request.Form.Files)
             //{
             //    if (file.Length > 0)
             //    {
             //        crs.DocumentName = file.FileName;
             //        crs.userName = sessionUserName;
             //        crs.UpdatedDate = DateTime.Today;
             //    }
             //}          
             //context_.Documents.Add(crs);

             //context_.SaveChanges();
             HttpClient client = new HttpClient();

             //HttpClient client = aPIHelper.initial();
             MultipartFormDataContent multiContent = new MultipartFormDataContent();

             var request = HttpContext.Request;
             foreach (var file in request.Form.Files)
             {
                 if (file.Length > 0)
                 {
                     byte[] data = System.IO.File.ReadAllBytes(file.FileName);
                     ByteArrayContent bytes = new ByteArrayContent(data);
                     string fileName = Path.GetFileName(file.ToString());
                     multiContent.Add(bytes, "files", fileName);
                    HttpResponseMessage message= await client.PostAsync("http://localhost:52464/", multiContent);
                 }
             }


             return RedirectToAction("HomePage");
         }*/


        [HttpPost]
        public async Task<IActionResult> AddDocument(List<IFormFile> files)
        {
            //var request = HttpContext.Request;
            string sessionUserName = HttpContext.Session.GetString("SessionUserName");
            Document document = new Document();
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
            context_.Documents.Add(document);
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
                    HttpResponseMessage message = await client.PostAsync("http://localhost:52464/api/files/", multiContent);
                }
            }
            //return Ok();
            return RedirectToAction("HomePage");
        }


        //----< shows details for each document >----------------------
        public ActionResult documentDetails(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            Document document = context_.Documents.Find(id);

            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            var lects = context_.Comments.Where(l => l.Document == document);

            document.Comments = lects.OrderBy(l => l.Comment).Select(l => l).ToList<Comments>();
            //course.Lectures = lects.ToList<Lecture>();

            if (document.Comments == null)
            {
                document.Comments = new List<Comments>();
                Comments lct = new Comments();
                lct.Comment = "none";
                lct.CommentDate = DateTime.MinValue;
                document.Comments.Add(lct);
            }
            return View(document);
        }

        public async Task<IActionResult> viewDocument(int? id)
        {
            bool found = false;
            DocumentView documentView = new DocumentView();
            HttpClient client = new HttpClient();

            Document document = context_.Documents.Find(id);
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
                        byte[] bytes= await result.Content.ReadAsByteArrayAsync();
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
        public IActionResult editDocument(int? id)
        {
            if (id == null)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            Document document = context_.Documents.Find(id);
            if (document == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View(document);
        }

        //----< posts back edited results for specific document >------

        [HttpPost]
        public IActionResult editDocument(int? id, Document crs)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            var document = context_.Documents.Find(id);
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
            return RedirectToAction("HomePage");
        }
        //----< deletes a document by id >-----------------------------
        /*
         * - note that Delete does not send back a view, but
         *   simply redirects back to the Index view.
         */
        public async Task<IActionResult> deleteDocument(int? id)
        {
            Document document=null;
            HttpClient client = new HttpClient();
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                document = context_.Documents.Find(id);
                if (document != null)
                {
                    context_.Remove(document);
                    context_.SaveChanges();
                }
          
            using (var result = await client.DeleteAsync("http://localhost:52464/api/files" + "/" + document.DocumentName))
            {
                if (result.IsSuccessStatusCode)
                {

                }

                }
            }
            catch (Exception)
            {
                // nothing for now
            }
            return RedirectToAction("HomePage");
        }

        //----< show list of lectures, ordered by Title >------------
        public IActionResult viewComment()
        {
            var request = HttpContext.Request;
            string sessionUserName = HttpContext.Session.GetString("SessionUserName");

            // fluent API
            var lects = context_.Comments.Include(l => l.Document).Where(l=>sessionUserName.Equals(l.userName));
            var orderedLects = lects.OrderBy(l => l.Comment)
              .OrderBy(l => l.Document)
              .Select(l => l);
            return View(orderedLects);
        }

        //----< shows form for creating a lecture >------------------

        [HttpGet]
        public IActionResult AddComment(int id)
        {
            HttpContext.Session.SetInt32(sessionId_, id);

            // this works too
            // TempData[sessionId_] = id;

            Document course = context_.Documents.Find(id);
            if (course == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            Comments lct = new Comments();
            return View(lct);
        }

        //----< posts back new courses details >---------------------

        //----< Add new lecture to course >--------------------------

        [HttpPost]
        public IActionResult AddComment(int? id, Comments lct)
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

            var document = context_.Documents.Find(courseId_);

            if (document != null)
            {
                if (document.Comments == null)  // doesn't have any lectures yet
                {
                    List<Comments> comments = new List<Comments>();
                    document.Comments = comments;
                }
                lct.CommentDate = DateTime.Today;
                lct.userName = sessionUserName;

                document.Comments.Add(lct);

                try
                {
                    context_.SaveChanges();
                }
                catch (Exception)
                {
                    // do nothing for now
                }
            }
            return RedirectToAction("HomePage");
        }


        //----< deletes a comment by id >-----------------------------
        /*
         * - note that Delete does not send back a view, but
         *   simply redirects back to the Index view, which 
         *   will not show the deleted comment.
         */
        public IActionResult deleteComment(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                var comment = context_.Comments.Find(id);
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
            return RedirectToAction("HomePage");
        }

        //----< gets form to edit a specific comment via id >---------

        [HttpGet]
        public IActionResult editComment(int? id)
        {
            if (id == null)
            {
                return StatusCode(Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest);
            }
            Comments comment = context_.Comments.Find(id);

            if (comment == null)
            {
                return StatusCode(StatusCodes.Status404NotFound);
            }
            return View(comment);
        }

        //----< posts back edited results for specific comment >------

        [HttpPost]
        public IActionResult editComment(int? id, Comments lct)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            var comment = context_.Comments.Find(id);

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
            return RedirectToAction("HomePage");
        }
 
        public IActionResult Logout()
        {
            HttpContext.Session.SetString("SessionUserName", null);
            //return View(context_.Documents.ToList<Document>());
            return RedirectToAction("Index");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
