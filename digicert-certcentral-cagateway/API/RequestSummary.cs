using System;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class RequestSummary
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("date")]
		public DateTime date { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		//pending, approved, rejected
		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("comments")]
		public string comments { get; set; }
	}
}