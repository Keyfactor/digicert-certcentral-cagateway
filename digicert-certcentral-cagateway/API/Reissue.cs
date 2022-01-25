using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	[Serializable]
	public class ReissueRequest : CertCentralBaseRequest
	{
		public ReissueRequest(uint orderId)
		{
			Method = "POST";
			OrderId = orderId;
			Resource = $"services/v2/order/certificate/{OrderId}/reissue"; // https://www.digicert.com/services/v2/order/certificate/{order_id}/reissue
			Certificate = new CertificateReissueRequest();
		}

		[JsonProperty("certificate")]
		public CertificateReissueRequest Certificate { get; set; }

		[JsonProperty("order_id")]
		public uint OrderId { get; set; }

		[JsonProperty("skip_approval")]
		public bool SkipApproval { get; set; }
	}

	public class CertificateReissueRequest
	{
		[JsonProperty("common_name")]
		public string CommonName { get; set; }

		[JsonProperty("dns_names")]
		public List<string> DnsNames { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("server_platform")]
		public Server_platform ServerPlatform { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }
	}

	public class ReissueResponse : CertCentralBaseResponse
	{
		public ReissueResponse()
		{
			Requests = new List<Requests>();
		}

		[JsonProperty("id")]
		public int OrderId { get; set; }

		[JsonProperty("requests")]
		public List<Requests> Requests { get; set; }

		[JsonProperty("certificate_chain")]
		public List<CertificateChainElement> CertificateChain { get; set; }
	}
}