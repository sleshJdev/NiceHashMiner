using Newtonsoft.Json;
using System;

namespace NiceHashMiner.Configs
{
    [Serializable]
    public sealed class MinerSettings
    {
        private readonly string bitcoinAddress;
        public string BitcoinAddress { get { return bitcoinAddress; } }

        [JsonConstructor]
        public MinerSettings([JsonProperty("address")] string bitcoinAddress)
        {
            this.bitcoinAddress = bitcoinAddress;
        }
    }
}
