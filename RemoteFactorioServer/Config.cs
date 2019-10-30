using System.Collections.Generic;

namespace RemoteFactorioServer
{
    class Config
    {
        public string RemoteIp { get; set; }
        public int RemotePort { get; set; }
        public IList<string> Servers { get; set; }
        public string ServerFolder { get; set; }
        public string ServerStartPoint { get; set; }
        public IList<string> Usernames { get; set; }
        public IList<string> Passwords { get; set; }
    }
}
