using System;
using System.Collections.Generic;
using System.Text;

namespace RemoteFactorioServer
{
    class Config
    {
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public IList<string> Servers { get; set; }
        public IList<string> Usernames { get; set; }
        public IList<string> Passwords { get; set; }
    }
}
