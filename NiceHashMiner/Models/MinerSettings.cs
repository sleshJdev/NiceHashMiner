using Newtonsoft.Json;

namespace NiceHashMiner.Configs
{
    public sealed class MinerSettings
    {
        private readonly string bitcoinAddress;
        public string BitcoinAddress { get; private set; }

        [JsonConstructor]
        public MinerSettings([JsonProperty("address")] string bitcoinAddress)
        {
            this.bitcoinAddress = bitcoinAddress;
        }
    }
}
