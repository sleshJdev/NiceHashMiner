using System;

namespace NiceHashMiner.Models
{
    [Serializable]
    public sealed class Token
    {
        public string Name { set; get; }
        public string Value { set; get; }
    }
}