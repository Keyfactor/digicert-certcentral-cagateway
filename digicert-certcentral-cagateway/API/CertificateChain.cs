using Newtonsoft.Json;

using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class CertificateChainRequest : CertCentralBaseRequest
	{
		public CertificateChainRequest(string certificate_id)
		{
			this.Resource = $"services/v2/certificate/{certificate_id}/chain";
			this.Method = "GET";
		}
	}

	public class CertificateChainResponse : CertCentralBaseResponse
	{
		[JsonProperty("Intermediates")]
		public List<CertificateChainElement> Intermediates { get; set; }
	}
}