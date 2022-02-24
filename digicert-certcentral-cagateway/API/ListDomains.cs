// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.API
{
	public class ListDomainsRequest : CertCentralBaseRequest
	{
		public ListDomainsRequest()
		{
			this.Resource = "services/v2/domain";
			this.Method = "GET";
			this.include_validation = false;
		}

		[JsonProperty("container_id")]
		public int container_id { get; set; }

		[JsonProperty("include_validation")]
		public bool include_validation { get; set; }

		public bool ShouldSerializecontainer_id()
		{
			return container_id == 0 ? false : true;
		}

		public new string BuildParameters()
		{
			StringBuilder sbParamters = new StringBuilder();

			sbParamters.Append("include_validation=").Append(HttpUtility.UrlEncode(this.include_validation.ToString()));
			if (container_id > 0)
				sbParamters.Append("&container_id=").Append(HttpUtility.UrlEncode(this.container_id.ToString()));

			return sbParamters.ToString();
		}
	}

	public class DomainOrganization
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("assumed_name")]
		public string assumed_name { get; set; }

		[JsonProperty("display_name")]
		public string display_name { get; set; }
	}

	public class DomainDetails
	{
		[JsonProperty("id")]
		public int id { get; set; }

		[JsonProperty("is_active")]
		public bool is_active { get; set; }

		[JsonProperty("name")]
		public string name { get; set; }

		[JsonProperty("date_created")]
		public DateTime date_created { get; set; }

		[JsonProperty("organization")]
		public DomainOrganization organization { get; set; }

		[JsonProperty("container")]
		public IdInformation container { get; set; }
	}

	public class ListDomainsResponse : CertCentralBaseResponse
	{
		public ListDomainsResponse()
		{
			this.domains = new List<DomainDetails>();
		}

		[JsonProperty("domains")]
		public List<DomainDetails> domains { get; set; }
	}
}