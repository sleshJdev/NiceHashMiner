using System;

namespace NiceHashMiner.Models
{
    [Serializable]
    public sealed class AuthDetails
    {
        public Token Token { set; get; }
        public User User { set; get; }
    }
}
