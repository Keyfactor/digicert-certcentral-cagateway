// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

namespace Keyfactor.Extensions.AnyGateway.DigiCert
{
	public class Constants
	{
		public class Status
		{
			public const string ISSUED = "issued";
			public const string PENDING = "pending";
			public const string APPROVED = "approved";
			public const string REJECTED = "rejected";
			public const string NEEDS_APPROVAL = "needs_approval";
		}

		public class Config
		{
			public const string APIKEY = "APIKey";
			public const string REGION = "Region";
			public const string DIVISION_ID = "DivisionId";
			public const string LIFETIME = "LifetimeDays";
			public const string CA_CERT_ID = "CACertId";
			public const string PAYMENT_METHOD = "PaymentMethod";
		}

		public class RequestAttributes
		{
			public const string ORGANIZATION_NAME = "Organization-Name";
			public const string DCV_METHOD = "DCV-Method";
		}

		public class ProductTypes
		{
			public const string DV_SSL_CERT = "dv_ssl_certificate";
		}
	}
}