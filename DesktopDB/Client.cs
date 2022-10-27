using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DesktopDB
{
    public class Client
    {
        public int id { get; set; }
        public String email { get; set; }
        public String psswrd { get; set; }
        public int fk_address { get; set; }
        public String role { get; set; }
    }
}
