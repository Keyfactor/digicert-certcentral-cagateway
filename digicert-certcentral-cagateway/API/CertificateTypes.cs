using Newtonsoft.Json;

using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	/// <summary>
	/// Request to get a certificate type.
	/// </summary>
	public class CertificateTypesRequest : CertCentralBaseRequest
	{
		/// <summary>
		/// Creates a new <see cref="CertificateTypesRequest"/> with default values.
		/// </summary>
		public CertificateTypesRequest()
		{
			Method = "GET";
			Resource = "services/v2/product";
		}
	}

	/// <summary>
	/// A single product.
	/// </summary>
	public class TypesProduct
	{
		/// <summary>
		/// The overall group this product type belongs to.
		/// </summary>
		[JsonProperty("group_name")]
		public string GroupName { get; set; }

		/// <summary>
		/// The name ID of the certificate type.
		/// </summary>
		[JsonProperty("name_id")]
		public string NameId { get; set; }

		/// <summary>
		/// The name of the certificate type.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The type of the certificate type.
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }
	}

	/// <summary>
	/// Response for the request to get the certificate types.
	/// </summary>
	public class CertificateTypesResponse : CertCentralBaseResponse
	{
		/// <summary>
		/// Creates a new <see cref="CertificateTypesResponse"/> with a default list.
		/// </summary>
		public CertificateTypesResponse()
		{
			Products = new List<TypesProduct>();
		}

		/// <summary>
		/// The certificate types that can be found on DigiCert.
		/// </summary>
		[JsonProperty("products")]
		public List<TypesProduct> Products { get; set; }
	}
}