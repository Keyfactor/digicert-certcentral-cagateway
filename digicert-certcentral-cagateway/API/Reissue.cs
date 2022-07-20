// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

using System;
using System.Collections.Generic;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	[Serializable]
	public class ReissueRequest : CertCentralBaseRequest
	{
		public ReissueRequest(uint orderId)
		{
			Method = "POST";
			OrderId = orderId;
			Resource = $"services/v2/order/certificate/{OrderId}/reissue"; // https://www.digicert.com/services/v2/order/certificate/{order_id}/reissue
			Certificate = new CertificateReissueRequest();
		}

		[JsonProperty("certificate")]
		public CertificateReissueRequest Certificate { get; set; }

		[JsonProperty("order_id")]
		public uint OrderId { get; set; }

		[JsonProperty("skip_approval")]
		public bool SkipApproval { get; set; }
	}

	public class CertificateReissueRequest
	{
		[JsonProperty("common_name")]
		public string CommonName { get; set; }

		[JsonProperty("dns_names")]
		public List<string> DnsNames { get; set; }

		[JsonProperty("csr")]
		public string CSR { get; set; }

		[JsonProperty("server_platform")]
		public Server_platform ServerPlatform { get; set; }

		[JsonProperty("signature_hash")]
		public string SignatureHash { get; set; }

		[JsonProperty("ca_cert_id")]
		public string CACertID { get; set; }
	}

	public class ReissueResponse : CertCentralBaseResponse
	{
		public ReissueResponse()
		{
			Requests = new List<Requests>();
		}

		[JsonProperty("id")]
		public int OrderId { get; set; }

		[JsonProperty("requests")]
		public List<Requests> Requests { get; set; }

		[JsonProperty("certificate_chain")]
		public List<CertificateChainElement> CertificateChain { get; set; }
	}
}