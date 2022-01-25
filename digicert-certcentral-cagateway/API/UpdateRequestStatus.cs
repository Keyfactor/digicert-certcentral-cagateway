using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class UpdateRequestStatusRequest : CertCentralBaseRequest
	{
		public UpdateRequestStatusRequest(int requestId)
		{
			RequestID = requestId;
			Resource = $"services/v2/request/{requestId}/status";
			Method = "PUT";
		}

		public UpdateRequestStatusRequest(int requestId, string status)
		{
			RequestID = requestId;
			Resource = $"services/v2/request/{requestId}/status";
			Method = "PUT";
			Status = status;
		}

		[JsonIgnore]
		public int RequestID { get; set; }

		//submitted, pending, approved, rejected
		[JsonProperty("status")]
		public string Status { get; set; }

		[JsonProperty("processor_comment")]
		public string ProcessorComment { get; set; }
	}

	public class UpdateRequestStatusResponse : CertCentralBaseResponse
	{
	}
}