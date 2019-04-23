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

namespace EBook_Reader.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext context_;
        private const string sessionId_ = "SessionId";
        APIHelper aPIHelper = new APIHelper();

        public HomeController(ApplicationDbContext context)
        {
            context_ = context;
        }
        public IActionResult Index()
        {
            //return View(context_.Documents.ToList<Document>());
            return View();
        }

        //public IActionResult Login()
        //{
        //    return RedirectToPage("/Areas/Identity/Pages/Account/Login.cshtml");
        //}

        public IActionResult HomePage()
        {
            return View(context_.Documents.ToList<Document>());
        }

        public IActionResult Privacy()
        {
            return View();
        }
        //----< displays form for Adding a new document >----------------
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult addDocument(int id)
        {
            var model = new Document();
            return View(model);
        }

        //----< posts back new document details >---------------------

        [HttpPost]
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


        public ActionResult viewDocument()
        {
            DocumentView documentView = new DocumentView();
            var filePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"..\..\..\..\wwwroot\FileStorage\";
                var files = Directory.GetFiles(filePath).ToList<string>();
                using (PdfReader reader = new PdfReader(files[0]))
                {
                    StringBuilder text = new StringBuilder();
                    for (int i = 1; i <= reader.NumberOfPages; i++)
                    {
                        text.Append(PdfTextExtractor.GetTextFromPage(reader, i));

                    }
                    
                    documentView.textContent = text.ToString();
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
        public IActionResult deleteDocument(int? id)
        {
            if (id == null)
            {
                return StatusCode(StatusCodes.Status400BadRequest);
            }
            try
            {
                var document = context_.Documents.Find(id);
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
            return RedirectToAction("HomePage");
        }

        //----< show list of lectures, ordered by Title >------------

        public IActionResult viewComment()
        {
            // fluent API
            var lects = context_.Comments.Include(l => l.Document);
            var orderedLects = lects.OrderBy(l => l.Comment)
              .OrderBy(l => l.Document)
              .Select(l => l);
            return View(orderedLects);

            // Linq
            //var lects = context_.Lectures.Include(l => l.Course);
            //var orderedLects = from l in lects
            //                   orderby l.Title
            //                   orderby l.Course
            //                   select l;
            //return View(orderedLects);

            // doesn't return Lecture's course nor order by title
            //return View(context_.Lectures.ToList<Lecture>());
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
