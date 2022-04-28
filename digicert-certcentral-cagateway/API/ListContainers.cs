// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

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