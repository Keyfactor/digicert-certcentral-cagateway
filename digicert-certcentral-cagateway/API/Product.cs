using Newtonsoft.Json;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class Product
	{
		[JsonProperty("name_id")]
		public string name_id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("type")]
		public string type { get; set; }

		[JsonProperty("validation_type")]
		public string validation_type { get; set; }

		[JsonProperty("validation_name")]
		public string validation_name { get; set; }

		[JsonProperty("validation_description")]
		public string validation_description { get; set; }
	}
}