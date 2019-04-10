using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace EBook_Reader.Models
{
    public class Comments
    {
        public int CommentsId { get; set; }
        public DateTime CommentDate { get; set; }
        public string Comment { get; set; }

        public int? DocumentId { get; set; }
        public Document Document { get; set; }
    }
}
