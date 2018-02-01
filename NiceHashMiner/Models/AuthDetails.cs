using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Models
{
    public sealed class AuthDetails
    {
        public Token token { set; get; }
        public User user { set; get; }
    }
}
