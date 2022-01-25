using System;
using System.Text;
using Newtonsoft.Json;
using System.Web;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class RevokeCertificateRequest : CertCentralBaseRequest
	{
		public RevokeCertificateRequest(int certificate_id)
		{
			this.certificate_id = certificate_id;
			this.Resource = "services/v2/certificate/" + certificate_id.ToString() + "/revoke";
			this.Method = "PUT";
		}

		[JsonIgnore]
		public int certificate_id { get; set; }

		[JsonProperty("comments")]
		public string comments { get; set; }

		public new string BuildParameters()
		{
			StringBuilder sbParamters = new StringBuilder();

			sbParamters.Append("comments=").Append(HttpUtility.UrlEncode(this.comments.ToString()));

			return sbParamters.ToString();
		}
	}

	public class RevokeCertificateByOrderRequest : CertCentralBaseRequest
	{
		public RevokeCertificateByOrderRequest(int order_id)
		{
			this.order_id = order_id;
			this.Resource = "services/v2/order/certificate/" + order_id.ToString() + "/revoke";
			this.Method = "PUT";
		}

		[JsonIgnore]
		public int order_id { get; set; }

		[JsonProperty("comments")]
		public string comments { get; set; }

		public new string BuildParameters()
		{
			StringBuilder sbParamters = new StringBuilder();

			sbParamters.Append("comments=").Append(HttpUtility.UrlEncode(this.comments.ToString()));

			return sbParamters.ToString();
		}
	}

	public class Requester
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

	public class RevokeCertificateResponse : CertCentralBaseResponse
	{
		[JsonProperty("id")]
		public int order_id { get; set; }

		[JsonProperty("date")]
		public DateTime date { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("requester")]
		public Requester requester { get; set; }

		[JsonProperty("comments")]
		public string comments { get; set; }
	}
}