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
	public class ViewCertificateOrderRequest : CertCentralBaseRequest
	{
		public ViewCertificateOrderRequest(uint OrderId)
		{
			this.order_id = OrderId;
			this.Resource = "services/v2/order/certificate/" + this.order_id.ToString();
			this.Method = "GET";
		}

		public uint order_id { get; set; }
	}

	public class Server_platform
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("install_url")]
		public string install_url { get; set; }

		[JsonProperty("csr_url")]
		public string csr_url { get; set; }
	}

	public class CertificateOrder
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("thumbprint")]
		public string thumbprint { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		[JsonProperty("serial_number")]
		public string serial_number { get; set; }

		[JsonProperty("common_name")]
		public string common_name { get; set; }

		[JsonProperty("dns_names")]
		public List<string> dns_names { get; set; }

		[JsonProperty("date_created")]
		public DateTime? date_created { get; set; }

		//YYYY-MM-DD
		[JsonProperty("valid_from")]
		public DateTime? valid_from { get; set; }

		//YYYY-MM-DD
		[JsonProperty("valid_till")]
		public DateTime? valid_till { get; set; }

		[JsonProperty("csr")]
		public string csr { get; set; }

		[JsonProperty("organization")]
		public IdInformation organization { get; set; }

		[JsonProperty("organization_units")]
		public List<string> organization_units { get; set; }

		[JsonProperty("server_platform")]
		public Server_platform server_platform { get; set; }

		[JsonProperty("signature_hash")]
		public string signature_hash { get; set; }

		[JsonProperty("key_size")]
		public int key_size { get; set; }

		[JsonProperty("ca_cert")]
		public IdInformation ca_cert { get; set; }
	}

	public class CertificateOrganization
	{
		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("display_name")]
		public string display_name { get; set; }

		[JsonProperty("is_active")]
		public bool is_active { get; set; }

		[JsonProperty("city")]
		public string city { get; set; }

		[JsonProperty("state")]
		public string state { get; set; }

		[JsonProperty("country")]
		public string country { get; set; }
	}

	public class OrderNote
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("text")]
		public string text { get; set; }
	}

	public class ViewCertificateOrderResponse : CertCentralBaseResponse
	{
		public ViewCertificateOrderResponse()
		{
			this.ContentType = ContentTypes.TEXT;
			this.requests = new List<RequestSummary>();
			this.notes = new List<OrderNote>();
		}

		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("certificate")]
		public CertificateOrder certificate { get; set; }

		[JsonProperty("status")]
		public string status { get; set; }

		//[JsonProperty("is_renewal")]
		//public bool is_renewal { get; set; }

		//[JsonProperty("is_renewed")]
		//public bool is_renewed { get; set; }

		//[JsonProperty("renewed_order_id")]
		//public int renewed_order_id { get; set; }

		[JsonProperty("date_created")]
		public DateTime? date_created { get; set; }

		//[JsonProperty("organization")]
		//public CertificateOrganization organization { get; set; }

		//[JsonProperty("disable_renewal_notifications")]
		//public bool disable_renewal_notifications { get; set; }

		//[JsonProperty("container")]
		//public IdInformation container { get; set; }

		[JsonProperty("product")]
		public Product product { get; set; }

		[JsonProperty("organization_contact")]
		public Contact organization_contact { get; set; }

		//[JsonProperty("technical_contact")]
		//public Contact technical_contact { get; set; }

		//[JsonProperty("user")]
		//public Contact user { get; set; }

		[JsonProperty("requests")]
		public List<RequestSummary> requests { get; set; }

		[JsonProperty("dcv_method")]
		public string dcv_method { get; set; }

		[JsonProperty("notes")]
		public List<OrderNote> notes { get; set; }

		//[JsonProperty("receipt_id")]
		//public int receipt_id { get; set; }

		//[JsonProperty("cs_provisioning_method")]
		//public string cs_provisioning_method { get; set; }

		//[JsonProperty("send_minus_90")]
		//public bool send_minus_90 { get; set; }

		//[JsonProperty("send_minus_60")]
		//public bool send_minus_60 { get; set; }

		//[JsonProperty("send_minus_30")]
		//public bool send_minus_30 { get; set; }

		//[JsonProperty("send_minus_7")]
		//public bool send_minus_7 { get; set; }

		//[JsonProperty("send_plus_7")]
		//public bool send_plus_7 { get; set; }

		//[JsonProperty("public_id")]
		//public string public_id { get; set; }

		//[JsonProperty("allow_duplicates")]
		//public bool allow_duplicates { get; set; }

		//[JsonProperty("user_assignments")]
		//public List<User> user_assignments { get; set; }

		//[JsonProperty("payment_method")]
		//public string payment_method { get; set; }

		[JsonIgnore]
		public string Certificate { get; set; }

		[JsonIgnore]
		public string CertificateTemplate { get; set; }

		[JsonIgnore]
		public string RawData { get; set; }
	}
}