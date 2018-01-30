using System;
using System.Collections.Generic;
using System.Text;

namespace NiceHashMiner.Configs
{
    [Serializable]
    public sealed class MinerSettings
    {
        private readonly string bitcoinAddress;
        public string BitcoinAddress { get; private set; }

        public MinerSettings(string bitcoinAddress)
        {
            this.bitcoinAddress = bitcoinAddress;
        }
    }
}
