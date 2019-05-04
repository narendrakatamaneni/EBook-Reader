using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EBook_Reader.Models
{
    public class PublicComments
    {
        public int PublicCommentsId { get; set; }
        public DateTime CommentDate { get; set; }
        public string Comment { get; set; }
        public string userName { get; set; }

        public int? PublicDocumentId { get; set; }
        public PublicDocument PublicDocument { get; set; }
    }
}
