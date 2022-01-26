using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Web;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class ListRequestsRequest : CertCentralBaseRequest
	{
		public ListRequestsRequest()
		{
			this.Resource = "services/v2/request";
			this.Method = "GET";
			this.status = "";
		}

		//pending, approved, rejected
		[JsonProperty("status")]
		public string status { get; set; }

		public new string BuildParameters()
		{
			StringBuilder sbParamters = new StringBuilder();

			if (!String.IsNullOrEmpty(status))
				sbParamters.Append("status=").Append(HttpUtility.UrlEncode(this.status));

			return sbParamters.ToString();
		}
	}

	public class RequestCertificate
	{
		[JsonProperty("common_name")]
		public string common_name { get; set; }
	}

	public class RequestOrder
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("certificate")]
		public RequestCertificate certificate { get; set; }

		[JsonProperty("organization")]
		public IdInformation organization { get; set; }

		[JsonProperty("container")]
		public IdInformation container { get; set; }

		[JsonProperty("product")]
		public Product product { get; set; }
	}

	public class RequestPerson
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("first_name")]
		public string first_name { get; set; }

		[JsonProperty("last_name")]
		public string last_name { get; set; }

		[JsonProperty("email")]
		public string email { get; set; }
	}

	public class Request
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("date")]
		public DateTime date { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("requester")]
		public RequestPerson requester { get; set; }

		[JsonProperty("processor")]
		public RequestPerson processor { get; set; }

		[JsonProperty("order")]
		public RequestOrder order { get; set; }
	}

	public class ListRequestsResponse : CertCentralBaseResponse
	{
		public ListRequestsResponse()
		{
			this.requests = new List<Request>();
		}

		[JsonProperty("requests")]
		public List<Request> requests { get; set; }
	}
}