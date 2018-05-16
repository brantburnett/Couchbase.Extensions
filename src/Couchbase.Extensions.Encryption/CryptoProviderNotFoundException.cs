using System;

namespace Couchbase.Extensions.Encryption
{
    public class CryptoProviderNotFoundException : Exception
    {
        public CryptoProviderNotFoundException(string providerName)
        {
            ProviderName = providerName;
        }

        public CryptoProviderNotFoundException(string message, string providerName)
            : base(message)
        {
            ProviderName = providerName;
        }

        public CryptoProviderNotFoundException(string message, string providerName, Exception innerException)
            : base(message, innerException)
        {
            ProviderName = providerName;
        }

        public string ProviderName { get; set; }
    }
}
