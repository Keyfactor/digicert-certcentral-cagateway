using CAProxy.AnyGateway.Interfaces;
using CAProxy.Common;
using CAProxy.Models;

using Keyfactor.Extensions.AnyGateway.DigiCert.API;
using Keyfactor.Extensions.AnyGateway.DigiCert.Client;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using System.Collections.Generic;
using System.Linq;

using static CSS.PKI.PKIConstants.Microsoft;

namespace Keyfactor.Extensions.AnyGateway.DigiCert
{
	/// <summary>
	/// Cert type for DigiCert CertCentral.
	/// </summary>
	public class CertCentralCertType : CABaseCertType
	{
		#region Private Fields

		private static readonly ILogger Logger = LogHandler.GetClassLogger<Conversions>();
		private static List<CertCentralCertType> _allTypes;

		#endregion Private Fields

		#region Properties

		/// <summary>
		/// All of the product types for which we do not support enrollment.
		/// </summary>
		public static List<string> UnsupportedProductTypes => new List<string>
		{
			"Document Signing - Organization (2000)",
			"Document Signing - Organization (5000)",
			"Code Signing",
			"EV Code Signing",
			"Premium SHA256",
			"Premium",
			"Email Security Plus",
			"Email Security Plus SHA256",
			"Digital Signature Plus",
			"Digital Signature Plus SHA256",
			"Grid Premium",
			"Grid Robot FQDN",
			"Grid Robot Name",
			"Grid Robot Email"
		};

		public string signatureAlgorithm { get; set; }
		public bool multidomain { get; set; }

		public string ProductType { get; set; }

		#endregion Properties

		#region Methods

		/// <summary>
		/// Gets all of the product types for DigiCert.
		/// </summary>
		/// <param name="proxyConfigProvider"></param>
		/// <returns></returns>
		public static List<CABaseCertType> GetAllTypes(ICAConnectorConfigProvider proxyConfigProvider)
		{
			if (_allTypes == null || !_allTypes.Any())
			{
				_allTypes = RetrieveCertCentralCertTypes(proxyConfigProvider);
			}

			return _allTypes.Cast<CABaseCertType>().ToList();
		}

		/// <summary>
		/// Uses the <see cref="ICAConnectorConfigProvider"/> to build a client and retrieve the product types for the given account.
		/// </summary>
		/// <param name="proxyConfigProvider"></param>
		/// <returns></returns>
		private static List<CertCentralCertType> RetrieveCertCentralCertTypes(ICAConnectorConfigProvider proxyConfigProvider)
		{
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(proxyConfigProvider);

			// Get all of the cert types.
			CertificateTypesResponse certTypes = client.GetAllCertificateTypes();
			if (certTypes.Status == API.CertCentralBaseResponse.StatusType.ERROR)
			{
				throw new UnsuccessfulRequestException(string.Join("\n", certTypes.Errors?.Select(x => x.message)), unchecked((uint)HRESULTs.INVALID_DATA));
			}

			// Get all the information we need.
			List<CertCentralCertType> types = new List<CertCentralCertType>();
			foreach (var type in certTypes.Products)
			{
				CertificateTypeDetailsResponse details = client.GetCertificateTypeDetails(type.NameId);
				if (details.Status == API.CertCentralBaseResponse.StatusType.ERROR)
				{
					throw new UnsuccessfulRequestException(string.Join("\n", certTypes.Errors?.Select(x => x.message)), unchecked((uint)HRESULTs.INVALID_DATA));
				}

				types.Add(new CertCentralCertType
				{
					DisplayName = $"{details.Name} {(UnsupportedProductTypes.Contains(details.Name) ? "(Enrollment Unavailable)" : string.Empty)}",
					multidomain = details.AdditionalDNSNamesAllowed,
					ProductCode = details.NameId,
					ShortName = details.Name,
					ProductType = details.Type,
					signatureAlgorithm = details.SignatureHashType.DefaultHashTypeId
				});
			}
			return types;
		}

		#endregion Methods
	}
}