using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace EBook_Reader.Helpher
{
    public class APIHelper
    {
        public HttpClient initial()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("http://localhost:52464/");
            return client;
        }
    }
}
