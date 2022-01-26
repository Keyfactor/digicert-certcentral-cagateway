using Newtonsoft.Json;

using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class StatusChangesRequest : CertCentralBaseRequest
	{
		public StatusChangesRequest(string lastSync, string todayUTC)
		{
			this.Resource = $"services/v2/order/certificate/status-changes?filters[status_last_updated]={lastSync}...{todayUTC}";
			this.Method = "GET";
		}
	}

	public class StatusOrder
	{
		[JsonProperty("order_id")]
		public int order_id { get; set; }

		[JsonProperty("certificate_id")]
		public int certificate_id { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }
	}

	public class StatusChangesResponse : CertCentralBaseResponse
	{
		[JsonProperty("orders")]
		public List<StatusOrder> orders { get; set; }
	}
}