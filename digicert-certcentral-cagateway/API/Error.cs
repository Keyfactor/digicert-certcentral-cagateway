using System.Collections.Generic;

using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class Error
	{
		[JsonProperty("code")]
		public string code { get; set; }

		[JsonProperty("message")]
		public string message { get; set; }
	}

	public class Errors
	{
		[JsonProperty("errors")]
		public List<Error> errors { get; set; }
	}
}