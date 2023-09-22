// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class IdInformation
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[DefaultValue(null)]
		[JsonProperty("name")]
		public string Name { get; set; }
	}

	public class CertificateRequest
	{
		[JsonProperty("common_name")]
		public string CommonName { get; set; }

		[JsonProperty("dns_names")]
		public List<string> DNSNames { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("organization_units")]
		public List<string> OrganizationUnits { get; set; }

		[JsonProperty("server_platform")]
		public IdInformation ServerPlatform { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }

		[JsonProperty("ca_cert_id")]
		public string CACertID { get; set; }
	}

	public class OrderRequest : CertCentralBaseRequest
	{
		public OrderRequest(CertCentralCertType certType)
		{
			Resource = "services/v2/order/certificate/" + certType.ProductCode;
			Method = "POST";
			CertType = certType;
			Certificate = new CertificateRequest();
			CustomExpirationDate = null;
		}

		[JsonIgnore]
		public CertCentralCertType CertType { get; set; }

		[JsonProperty("certificate")]
		public CertificateRequest Certificate { get; set; }

		[JsonProperty("organization")]
		private IdInformation Organization { get; set; } // Set via SetOrganization method

		[JsonProperty("validity_years")]
		public int ValidityYears { get; set; }

		[JsonProperty("custom_expiration_date")] //YYYY-MM-DD
		public DateTime? CustomExpirationDate { get; set; }

		[JsonProperty("comments")]
		public string Comments { get; set; }

		[JsonProperty("disable_renewal_notifications")]
		public bool DisableRenewalNotifications { get; set; }

		[DefaultValue(0)]
		[JsonProperty("renewal_of_order_id")]
		public int RenewalOfOrderId { get; set; }

		[JsonProperty("dcv_method")]
		public string DCVMethod { get; set; }

		[JsonProperty("container")]
		public CertificateOrderContainer Container { get; set; }

		[JsonProperty("custom_fields")]
		public List<MetadataField> CustomFields { get; set; }

		[JsonProperty("payment_method")]
		public string PaymentMethod { get; set; }

		public void SetOrganization(int? organizationId)
		{
			if (organizationId.HasValue)
			{
				Organization = new IdInformation()
				{
					Id = organizationId.Value.ToString()
				};
			}
			else
			{
				Organization = null;
			}
		}
	}

	public class CertificateOrderContainer
	{
		[JsonProperty("id")]
		public int Id { get; set; }
	}

	public class MetadataField
	{
		[JsonProperty("metadata_id")]
		public int MetadataId { get; set; }

		[JsonProperty("value")]
		public string Value { get; set; }
	}

	public class Requests
	{
		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("status")]
		public string Status { get; set; }
	}

	public class OrderResponse : CertCentralBaseResponse
	{
		public OrderResponse()
		{
			this.Requests = new List<Requests>();
			CertificateChain = null;
		}

		[JsonProperty("id")]
		public int OrderId { get; set; }

		[JsonProperty("requests")]
		public List<Requests> Requests { get; set; }

		[JsonProperty("certificate_id")]
		public int? CertificateId { get; set; }

		[JsonProperty("certificate_chain")]
		public List<CertificateChainElement> CertificateChain { get; set; }

		[JsonProperty("dcv_random_value")]
		public string DCVRandomValue { get; set; }
	}

	public class CertificateChainElement
	{
		[JsonProperty("subject_common_name")]
		public string SubjectCommonName { get; set; }

		[JsonProperty("pem")]
		public string PEM { get; set; }
	}
}