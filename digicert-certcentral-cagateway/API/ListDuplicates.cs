using System.Collections.Generic;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class ListDuplicatesRequest : CertCentralBaseRequest
	{
		public ListDuplicatesRequest(int orderId)
		{
			this.Resource = $"services/v2/order/certificate/{orderId}/duplicate";
			this.Method = "GET";
		}
	}

	public class ListDuplicatesResponse : CertCentralBaseResponse
	{
		[JsonProperty("certificates")]
		public List<CertificateOrder> certificates { get; set; }
	}
}