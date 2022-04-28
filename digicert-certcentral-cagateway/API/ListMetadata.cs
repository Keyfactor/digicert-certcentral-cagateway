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
	/// Request to get the metadata fields available.
	/// </summary>
	public class ListMetadataRequest : CertCentralBaseRequest
	{
		/// <summary>
		/// Creates a new <see cref="ListMetadataRequest"/> with the appropriate information.
		/// </summary>
		public ListMetadataRequest()
		{
			Method = "GET";
			Resource = $"services/v2/account/metadata";
		}
	}

	/// <summary>
	/// Response from the metadata endpoint.
	/// </summary>
	public class ListMetadataResponse : CertCentralBaseResponse
	{
		[JsonProperty("metadata")]
		public List<Metadata> MetadataFields { get; set; }
	}

	/// <summary>
	/// A class representing a single metadata field.
	/// </summary>
	public class Metadata
	{
		[JsonProperty("id")]
		public int Id { get; set; }

		[JsonProperty("label")]
		public string Label { get; set; }

		[JsonProperty("is_required")]
		public bool Required { get; set; }

		[JsonProperty("is_active")]
		public bool Active { get; set; }

		[JsonProperty("data_type")]
		public string DataType { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }
	}
}