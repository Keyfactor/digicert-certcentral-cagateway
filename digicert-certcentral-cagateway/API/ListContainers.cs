using Newtonsoft.Json;

using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	/// <summary>
	/// Request to get the containers available.
	/// </summary>
	public class ListContainersRequest : CertCentralBaseRequest
	{
		/// <summary>
		/// Creates a new <see cref="ListContainersRequest"/> with the appropriate information.
		/// </summary>
		public ListContainersRequest()
		{
			Method = "GET";
			Resource = $"services/v2/container";
		}
	}

	/// <summary>
	/// Response from the containers endpoint.
	/// </summary>
	public class ListContainersResponse : CertCentralBaseResponse
	{
		[JsonProperty("containers")]
		public List<Container> Containers { get; set; }
	}

	/// <summary>
	/// A class representing a single container.
	/// </summary>
	public class Container
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("public_id")]
		public string PublicId { get; set; }

		[JsonProperty("name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("parent_id")]
		public int ParentId { get; set; }

		[JsonProperty("template_id")]
		public int TemplateId { get; set; }

		[JsonProperty("ekey")]
		public string EKey { get; set; }

		[JsonProperty("has_logo")]
		public bool HasLogo { get; set; }

		[JsonProperty("is_active")]
		public bool IsActive { get; set; }

		[JsonProperty("allowed_domain_names")]
		public List<string> AllowedDomainNames { get; set; }
	}
}