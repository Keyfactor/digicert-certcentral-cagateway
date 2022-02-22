using Newtonsoft.Json;

using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	/// <summary>
	/// Request to get the details for a specific certificate type.
	/// </summary>
	public class CertificateTypeDetailsRequest : CertCentralBaseRequest
	{
		/// <summary>
		/// Creates a new <see cref="CertificateTypeDetailsRequest"/> with the appropriate information.
		/// </summary>
		/// <param name="nameId">The name ID of the certificate type desired.</param>
		public CertificateTypeDetailsRequest(string nameId)
		{
			Method = "GET";
			Resource = $"services/v2/product/{nameId}";
			ContainerId = null;
		}

		/// <summary>
		/// Creates a new <see cref="CertificateTypeDetailsRequest"/> with the appropriate information.
		/// </summary>
		/// <param name="nameId">The name ID of the certificate type desired.</param>
		/// <param name="containerId">The ID of the container to use for product information</param>
		public CertificateTypeDetailsRequest(string nameId, int? containerId)
		{
			Method = "GET";
			Resource = $"services/v2/product/{nameId}";
			ContainerId = containerId;
		}

		[JsonProperty("container_id")]
		public int? ContainerId { get; set; }

		public new string BuildParameters()
		{
			StringBuilder sbParameters = new StringBuilder();

			if (ContainerId != null)
			{
				sbParameters.Append("&container_id=").Append(HttpUtility.UrlEncode(ContainerId.ToString()));
			}

			return sbParameters.ToString();
		}
	}

	/// <summary>
	/// An allowed hash type.
	/// </summary>
	public class AllowedHashType
	{
		/// <summary>
		/// The ID of the allowed hash algorithm.
		/// </summary>
		[JsonProperty("id")]
		public string Id { get; set; }

		/// <summary>
		/// The name of the allowed hash algorithm.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	/// <summary>
	/// A collection of allowed hash types.
	/// </summary>
	public class SignatureHashType
	{
		public SignatureHashType()
		{
			AllowedHashTypes = new List<AllowedHashType>();
		}

		/// <summary>
		/// The allowed hash types.
		/// </summary>
		[JsonProperty("allowed_hash_types")]
		public List<AllowedHashType> AllowedHashTypes { get; set; }

		/// <summary>
		/// The default hash type's ID.
		/// </summary>
		[JsonProperty("default_hash_type_id")]
		public string DefaultHashTypeId { get; set; }
	}

	/// <summary>
	/// A class representing a single server platform.
	/// </summary>
	public class ServerPlatform
	{
		/// <summary>
		/// The platform's ID.
		/// </summary>
		[JsonProperty("id")]
		public int Id { get; set; }

		/// <summary>
		/// The platform's name.
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The URL for instructions on installing the certificate.
		/// </summary>
		[JsonProperty("install_url")]
		public string InstallUrl { get; set; }

		/// <summary>
		/// The URL with instructions for creating the CSR.
		/// </summary>
		[JsonProperty("csr_url")]
		public string CSRUrl { get; set; }
	}

	/// <summary>
	/// Response containing the details for a specific certificate type.
	/// </summary>
	public class CertificateTypeDetailsResponse : CertCentralBaseResponse
	{
		public CertificateTypeDetailsResponse()
		{
			AllowedValidityYears = new List<int>();
			ServerPlatforms = new List<ServerPlatform>();
		}

		/// <summary>
		/// The group name for this cert type.
		/// </summary>
		[JsonProperty("group_name")]
		public string GroupName { get; set; }

		/// <summary>
		/// The cert type's name ID.
		/// </summary>
		[JsonProperty("name_id")]
		public string NameId { get; set; }

		/// <summary>
		/// The cert type's display name
		/// </summary>
		[JsonProperty("name")]
		public string Name { get; set; }

		/// <summary>
		/// The cert type's type.
		/// </summary>
		[JsonProperty("type")]
		public string Type { get; set; }

		/// <summary>
		/// How many certs remain in the contract for this type.
		/// </summary>
		[JsonProperty("remaining_certs_in_contract")]
		public int RemainingCertsInContract { get; set; }

		/// <summary>
		/// Whether or not duplicate certs of this type are allowed.
		/// </summary>
		[JsonProperty("duplicates_allowed")]
		public bool DuplicatesAllowed { get; set; }

		/// <summary>
		/// The set of validity years this type allows.
		/// </summary>
		[JsonProperty("allowed_validity_years")]
		public List<int> AllowedValidityYears { get; set; }

		/// <summary>
		/// The hash types allowed on this certificate type.
		/// </summary>
		[JsonProperty("signature_hash_types")]
		public SignatureHashType SignatureHashType { get; set; }

		/// <summary>
		/// Whether or not additional DNS names are allowed.
		/// </summary>
		[JsonProperty("additional_dns_names_allowed")]
		public bool AdditionalDNSNamesAllowed { get; set; }

		/// <summary>
		/// Whether or not this type supports increased compatibility.
		/// </summary>
		[JsonProperty("increased_compatibility_allowed")]
		public bool IncreasedCompatibilityAllowed { get; set; }

		/// <summary>
		/// Whether or not this cert allows custom expiration dates.
		/// </summary>
		[JsonProperty("custom_expiration_date_allowed")]
		public bool CustomExpirationDateAllowed { get; set; }

		/// <summary>
		/// Whether or not a CSR is required to enroll for these certs.
		/// </summary>
		[JsonProperty("csr_required")]
		public bool CSRRequired { get; set; }

		/// <summary>
		/// Whether or not auto-renewals are allowed.
		/// </summary>
		[JsonProperty("allowed_auto_renew")]
		public bool AllowAutoRenew { get; set; }

		/// <summary>
		/// The server platforms allowed for this cert type.
		/// </summary>
		[JsonProperty("server_platforms")]
		public List<ServerPlatform> ServerPlatforms { get; set; }

		/// <summary>
		/// The license agreement.
		/// </summary>
		[JsonProperty("license_agreement")]
		public string LicenseAgreement { get; set; }
	}
}