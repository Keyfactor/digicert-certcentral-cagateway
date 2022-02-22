using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
			public const string DIVISION_ID = "DivisionId";
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