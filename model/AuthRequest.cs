using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudentLibrary.model
{
    internal class AuthRequest
    {
        public string action { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
