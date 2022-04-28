// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using CAProxy.Common;
using CAProxy.Models;

using Keyfactor.Extensions.AnyGateway.DigiCert.API;
using Keyfactor.Extensions.AnyGateway.DigiCert.Client;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using System;
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

		private static readonly ILogger Logger = LogHandler.GetClassLogger<CertCentralCertType>();
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
		/// <param name="proxyConfig"></param>
		/// <returns></returns>
		public static List<CABaseCertType> GetAllTypes(DigiCertCAConfig proxyConfig)
		{
			if (_allTypes == null || !_allTypes.Any())
			{
				_allTypes = RetrieveCertCentralCertTypes(proxyConfig);
			}

			return _allTypes.Cast<CABaseCertType>().ToList();
		}

		/// <summary>
		/// Uses the <see cref="DigiCertCAConfig"/> to build a client and retrieve the product types for the given account.
		/// </summary>
		/// <param name="proxyConfig"></param>
		/// <returns></returns>
		private static List<CertCentralCertType> RetrieveCertCentralCertTypes(DigiCertCAConfig proxyConfig)
		{
			Logger.MethodEntry(LogLevel.Trace);
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(proxyConfig);

			// Get all of the cert types.
			CertificateTypesResponse certTypes = client.GetAllCertificateTypes();
			if (certTypes.Status == API.CertCentralBaseResponse.StatusType.ERROR)
			{
				throw new UnsuccessfulRequestException(string.Join("\n", certTypes.Errors?.Select(x => x.message)), unchecked((uint)HRESULTs.INVALID_DATA));
			}
			Logger.LogDebug($"RetrieveCertCentralTypes: Found {certTypes.Products.Count} product types");
			// Get all the information we need.
			List<CertCentralCertType> types = new List<CertCentralCertType>();
			foreach (var type in certTypes.Products)
			{
				try
				{
					Logger.LogTrace($"RetrieveCertCentralType: Retrieving details for product type: {type.NameId}");
					CertificateTypeDetailsRequest detailsRequest = new CertificateTypeDetailsRequest(type.NameId, proxyConfig.DivisionId);
					CertificateTypeDetailsResponse details = client.GetCertificateTypeDetails(detailsRequest);
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
				catch (Exception ex)
				{
					Logger.LogError($"RetrieveCertCentralType: Unable to retrieve details for product type: {type.NameId}. Skipping...");
					Logger.LogTrace($"RetrieveCertCentralType: Type retrieval error details: {ex.Message}");
				}
			}
			return types;
		}

		#endregion Methods
	}
}