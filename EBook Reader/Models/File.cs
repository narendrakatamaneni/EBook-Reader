using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EBook_Reader.Models
{
    public class File
    {
        public int FileId { get; set; }
        public String UserName { get; set; }
        public string FileName { get; set; }

        public ICollection<File> Files { get; set; }
    }
}
