using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EBook_Reader.Models
{
    public class PublicDocument
    {
        public int PublicDocumentId { get; set; }
        public string userName { get; set; }
        public string DocumentName { get; set; }
        public DateTime UpdatedDate { get; set; }
        public string documentType { get; set; }

        public ICollection<PublicComments> PublicComments
        {
            get; set;
        }
    }
}
