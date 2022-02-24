// Copyright 2022 Keyfactor
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions
// and limitations under the License.

using CAProxy.Common.Config;

using Keyfactor.Extensions.AnyGateway.DigiCert.API;
using Keyfactor.Logging;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using static CSS.PKI.X509.X509Utilities;

namespace Keyfactor.Extensions.AnyGateway.DigiCert.Client
{
	public class CertCentralCredentials
	{
		public CertCentralCredentials()
		{
			this.EndPoint = "https://www.digicert.com/";
			this.APIKey = "";
		}

		public string EndPoint { get; set; }

		public string APIKey { get; set; }

		public string APIRequestorKey { get; set; }
	}

	public class CertCentralClient
	{
		private static ILogger Logger => LogHandler.GetClassLogger<CertCentralClient>();

		public CertCentralClient(string authAPIKey)
		{
			//set in config files
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			CertCentralCreds = new CertCentralCredentials() { APIKey = authAPIKey };
		}

		private CertCentralCredentials CertCentralCreds { get; set; }

		private class CertCentralResponse
		{
			public CertCentralResponse()
			{
				Success = true;
				Response = "";
			}

			public bool Success { get; set; }
			public string Response { get; set; }
		}

		private CertCentralResponse Request(CertCentralBaseRequest request)
		{
			return Request(request, "");
		}

		private CertCentralResponse Request(CertCentralBaseRequest request, string parameters)
		{
			return Request(request, parameters, true);
		}

		private static int RequestIDCounter = 1;

		private CertCentralResponse Request(CertCentralBaseRequest request, string parameters, bool adminuser)
		{
			//set in config files
			//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			CertCentralResponse oCertCertResponse = new CertCentralResponse();

			// Matching up requests and responses
			int reqID = RequestIDCounter++;

			try
			{
				string targetURI;
				if (request.Method == "POST" || request.Method == "PUT")
					targetURI = this.CertCentralCreds.EndPoint + request.Resource;
				else
				{
					if (String.IsNullOrEmpty(parameters))
						targetURI = this.CertCentralCreds.EndPoint + request.Resource;
					else
						targetURI = this.CertCentralCreds.EndPoint + request.Resource + "?" + parameters;
				}

				Logger.LogTrace($"Entered CertCentral Request (ID: {reqID}) Method: {request.Method} - URL: {targetURI}");

				HttpWebRequest objRequest = (HttpWebRequest)WebRequest.Create(targetURI);
				objRequest.Method = request.Method;
				if (adminuser)
					objRequest.Headers.Add("X-DC-DEVKEY", this.CertCentralCreds.APIKey);
				else
					objRequest.Headers.Add("X-DC-DEVKEY", this.CertCentralCreds.APIRequestorKey);

				objRequest.ContentType = "application/json";

				if (!String.IsNullOrEmpty(parameters) && (objRequest.Method == "POST" || objRequest.Method == "PUT"))
				{
					byte[] postBytes = Encoding.UTF8.GetBytes(parameters);
					objRequest.ContentLength = postBytes.Length;
					Stream requestStream = objRequest.GetRequestStream();
					requestStream.Write(postBytes, 0, postBytes.Length);
					requestStream.Close();
				}

				Logger.LogTrace(JsonConvert.SerializeObject(request));

				using (HttpWebResponse objResponse = (HttpWebResponse)objRequest.GetResponse())
				{
					string respString = new StreamReader(objResponse.GetResponseStream()).ReadToEnd();
					oCertCertResponse.Response = respString;
					Logger.LogTrace($"CertCentral CA (Request ID: {reqID}) has returned Response '{objResponse.StatusCode}: {respString}");
				}
			}
			catch (WebException wex)
			{
				if (wex.Response != null)
				{
					using (var errorResponse = (HttpWebResponse)wex.Response)
					{
						if (errorResponse.StatusCode == (HttpStatusCode)429/*Too Many Requests*/)
						{
							Logger.LogInformation($"Request ID: {reqID} was rate-limited. Trying again in 5 seconds");
							// TODO - Figure out how long to wait, then wait that long
							System.Threading.Thread.Sleep(5000);
							return Request(request, parameters, adminuser);
						}
						else
						{
							using (var reader = new StreamReader(errorResponse.GetResponseStream()))
							{
								string errorString = reader.ReadToEnd();
								oCertCertResponse.Response = errorString;
								Logger.LogTrace($"CertCentral CA (Request ID: {reqID}) has returned Response '{errorResponse.StatusCode}: {errorString}");
							}
						}
					}
				}
				else
				{
					Logger.LogDebug("CertCentral Response Error", wex);
					throw new Exception("Unable to establish connection to CertCentral web service", wex);
				}
			}
			catch (Exception ex)
			{
				Logger.LogError("CertCentral Response Error", ex);
				throw new Exception("Unable to establish connection to CertCentral web service", ex);
			}

			return oCertCertResponse;
		}

		private bool IsError(string response, CertCentralBaseResponse.ContentTypes eType)
		{
			switch (eType)
			{
				case CertCentralBaseResponse.ContentTypes.JSON:
					if (response.Contains("errors"))
						return true;
					break;
			}

			return false;
		}

		public ListOrganizationsResponse ListOrganizations(ListOrganizationsRequest request)
		{
			CertCentralResponse response = Request(request, request.BuildParameters());

			ListOrganizationsResponse listOrganizationsResponse = new ListOrganizationsResponse();

			if (IsError(response.Response, listOrganizationsResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				listOrganizationsResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				listOrganizationsResponse.Errors = errors.errors;
			}
			else
				listOrganizationsResponse = JsonConvert.DeserializeObject<ListOrganizationsResponse>(response.Response);

			return listOrganizationsResponse;
		}

		public ListDomainsResponse ListDomains(ListDomainsRequest request)
		{
			CertCentralResponse response = Request(request, request.BuildParameters());

			ListDomainsResponse listDomainsResponse = new ListDomainsResponse();

			if (IsError(response.Response, listDomainsResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				listDomainsResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				listDomainsResponse.Errors = errors.errors;
			}
			else
				listDomainsResponse = JsonConvert.DeserializeObject<ListDomainsResponse>(response.Response);

			return listDomainsResponse;
		}

		public ListContainersResponse ListContainers(ListContainersRequest request)
		{
			CertCentralResponse response = Request(request, request.BuildParameters());

			ListContainersResponse listContainersResponse = new ListContainersResponse();

			if (IsError(response.Response, listContainersResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				listContainersResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				listContainersResponse.Errors = errors.errors;
			}
			else
			{
				listContainersResponse = JsonConvert.DeserializeObject<ListContainersResponse>(response.Response);
			}

			return listContainersResponse;
		}

		public ListDuplicatesResponse ListDuplicates(ListDuplicatesRequest duplicatesRequest)
		{
			CertCentralResponse ccResponse = Request(duplicatesRequest);

			ListDuplicatesResponse duplicatesResponse = new ListDuplicatesResponse();

			if (IsError(ccResponse.Response, duplicatesResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(ccResponse.Response);
				duplicatesResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				duplicatesResponse.Errors = errors.errors;
			}
			else
			{
				duplicatesResponse = JsonConvert.DeserializeObject<ListDuplicatesResponse>(ccResponse.Response);
			}

			return duplicatesResponse;
		}

		public ListRequestsResponse ListRequests(ListRequestsRequest request)
		{
			CertCentralResponse response = Request(request, request.BuildParameters());

			ListRequestsResponse listRequestsResponse = new ListRequestsResponse();

			if (IsError(response.Response, listRequestsResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				listRequestsResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				listRequestsResponse.Errors = errors.errors;
			}
			else
				listRequestsResponse = JsonConvert.DeserializeObject<ListRequestsResponse>(response.Response);

			return listRequestsResponse;
		}

		public ListMetadataResponse ListMetadata(ListMetadataRequest request)
		{
			CertCentralResponse response = Request(request, request.BuildParameters());

			ListMetadataResponse listMetadataResponse = new ListMetadataResponse();

			if (IsError(response.Response, listMetadataResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				listMetadataResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				listMetadataResponse.Errors = errors.errors;
			}
			else
				listMetadataResponse = JsonConvert.DeserializeObject<ListMetadataResponse>(response.Response);

			return listMetadataResponse;
		}

		public OrderResponse OrderCertificate(OrderRequest request, bool adminUser)
		{
			CertCentralResponse response = Request(request, JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), adminUser);

			OrderResponse orderResponse = new OrderResponse();
			if (IsError(response.Response, orderResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				orderResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				orderResponse.Errors = errors.errors;
			}
			else
				orderResponse = JsonConvert.DeserializeObject<OrderResponse>(response.Response);

			return orderResponse;
		}

		public OrderResponse ReissueCertificate(ReissueRequest request, bool adminUser)
		{
			CertCentralResponse response = Request(request, JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), adminUser);

			OrderResponse reissueResponse = new OrderResponse();
			if (IsError(response.Response, reissueResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				reissueResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				reissueResponse.Errors = errors.errors;
			}
			else
			{
				reissueResponse = JsonConvert.DeserializeObject<OrderResponse>(response.Response);
			}

			return reissueResponse;
		}

		[Obsolete("This API endpoint does not seem to work correctly. Use the overload that takes 'RevokeCertificateByOrderRequest'")]
		public RevokeCertificateResponse RevokeCertificate(RevokeCertificateRequest request)
		{
			CertCentralResponse response = Request(request, JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

			RevokeCertificateResponse revokeOrderResponse = new RevokeCertificateResponse();
			if (IsError(response.Response, revokeOrderResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				revokeOrderResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				revokeOrderResponse.Errors = errors.errors;
			}
			else
				revokeOrderResponse = JsonConvert.DeserializeObject<RevokeCertificateResponse>(response.Response);

			return revokeOrderResponse;
		}

		public RevokeCertificateResponse RevokeCertificate(RevokeCertificateByOrderRequest request)
		{
			CertCentralResponse response = Request(request, JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

			RevokeCertificateResponse revokeOrderResponse = new RevokeCertificateResponse();
			if (IsError(response.Response, revokeOrderResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				revokeOrderResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				revokeOrderResponse.Errors = errors.errors;
			}
			else
				revokeOrderResponse = JsonConvert.DeserializeObject<RevokeCertificateResponse>(response.Response);

			return revokeOrderResponse;
		}

		public UpdateRequestStatusResponse UpdateRequestStatus(UpdateRequestStatusRequest request)
		{
			CertCentralResponse response = Request(request, JsonConvert.SerializeObject(request, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }));

			UpdateRequestStatusResponse updateRequestResponse = new UpdateRequestStatusResponse();
			if (IsError(response.Response, updateRequestResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				updateRequestResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				updateRequestResponse.Errors = errors.errors;
			}
			else
			{
				// A successful call to Update Request Status returns 204 No Content, so there is nothing to deserialize
				updateRequestResponse = new UpdateRequestStatusResponse()
				{
					Status = CertCentralBaseResponse.StatusType.SUCCESS
				};
			}

			return updateRequestResponse;
		}

		public DVCheckDCVResponse DVCheckDCV(DVCheckDCVRequest request)
		{
			CertCentralResponse response = Request(request, "");

			DVCheckDCVResponse checkDCVResponse = new DVCheckDCVResponse();
			if (IsError(response.Response, checkDCVResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				checkDCVResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				checkDCVResponse.Errors = errors.errors;
			}
			else
			{
				checkDCVResponse = JsonConvert.DeserializeObject<DVCheckDCVResponse>(response.Response);
			}

			return checkDCVResponse;
		}

		public CertificateChainResponse GetCertificateChain(CertificateChainRequest request)
		{
			CertCentralResponse response = Request(request);
			CertificateChainResponse chainResponse = new CertificateChainResponse();
			if (IsError(response.Response, chainResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				chainResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				chainResponse.Errors = errors.errors;
			}
			else
			{
				chainResponse = JsonConvert.DeserializeObject<CertificateChainResponse>(response.Response);
			}

			return chainResponse;
		}

		public StatusChangesResponse StatusChanges(StatusChangesRequest request)
		{
			CertCentralResponse certResponse = Request(request);
			StatusChangesResponse statusChangeResponse = new StatusChangesResponse();
			if (IsError(certResponse.Response, statusChangeResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(certResponse.Response);
				statusChangeResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				statusChangeResponse.Errors = errors.errors;
			}
			else
			{
				statusChangeResponse = JsonConvert.DeserializeObject<StatusChangesResponse>(certResponse.Response);
			}
			return statusChangeResponse;
		}

		public DownloadCertificateByFormatResponse DownloadCertificateByFormat(DownloadCertificateByFormatRequest request)
		{
			CertCentralResponse response = Request(request, "");
			DownloadCertificateByFormatResponse dlCertificateRequestResponse = new DownloadCertificateByFormatResponse();
			if (IsError(response.Response, dlCertificateRequestResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				dlCertificateRequestResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				dlCertificateRequestResponse.Errors = errors.errors;
			}
			else
			{
				if (request.format_type.Equals("p7b")) //client certificate
				{
					string strCertificate = response.Response;
					string noHeaders = ConfigurationUtils.OnlyBase64CertContent(strCertificate);

					//byte[] certBytes = Convert.FromBase64String(noHeaders);
					//SignedCms certContainer = new SignedCms();
					//certContainer.Decode(certBytes);
					//X509Certificate2Collection certificateCollection = certContainer.Certificates;

					//List<X509Certificate2> certFromPkcs7 = new List<X509Certificate2>(Pkcs7Helpers.GetCertsFromPkcs7(strCertificate));
					//X509Certificate2 endCertificateFromPKCS7 = certFromPkcs7.Last();

					X509Certificate2 endCertificateFromPKCS7 = new X509Certificate2(Pkcs7.NewestCertFromPkcs7(Convert.FromBase64String(noHeaders)));

					byte[] certFromPkcs7_Bytes = endCertificateFromPKCS7.RawData;
					string certFromPkcs7_String = Convert.ToBase64String(certFromPkcs7_Bytes);

					dlCertificateRequestResponse = new DownloadCertificateByFormatResponse() { certificate = certFromPkcs7_String.Replace("\r\n", "") };
				}
				else // for code_signing_certificate and ssl_certificate
				{
					dlCertificateRequestResponse = new DownloadCertificateByFormatResponse() { certificate = response.Response.Replace("\r\n", "") };
				}
			}

			return dlCertificateRequestResponse;
		}

		public ListCertificateOrdersResponse ListAllCertificateOrders()
		{
			int batch = 1000;
			ListCertificateOrdersResponse totalResponse = new ListCertificateOrdersResponse();

			do
			{
				ListCertificateOrdersRequest request = new ListCertificateOrdersRequest()
				{
					limit = batch,
					offset = totalResponse.orders.Count
				};

				CertCentralResponse response = Request(request, request.BuildParameters());

				ListCertificateOrdersResponse listCertificateResponse = new ListCertificateOrdersResponse();
				if (IsError(response.Response, listCertificateResponse.ContentType))
				{
					Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
					listCertificateResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
					listCertificateResponse.Errors = errors.errors;

					return listCertificateResponse;
				}
				else
				{
					ListCertificateOrdersResponse batchResponse = JsonConvert.DeserializeObject<ListCertificateOrdersResponse>(response.Response);
					totalResponse.orders.AddRange(batchResponse.orders);
					totalResponse.page.total = batchResponse.page.total;
				}
			}
			while (totalResponse.orders.Count < totalResponse.page.total);

			return totalResponse;
		}

		public ViewCertificateOrderResponse ViewCertificateOrder(ViewCertificateOrderRequest request)
		{
			ViewCertificateOrderResponse viewCertResponse = new ViewCertificateOrderResponse();

			CertCentralResponse response = Request(request);

			if (IsError(response.Response, viewCertResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				viewCertResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				viewCertResponse.Errors = errors.errors;
			}
			else
			{
				viewCertResponse = JsonConvert.DeserializeObject<ViewCertificateOrderResponse>(response.Response);
				viewCertResponse.RawData = response.Response;
			}

			return viewCertResponse;
		}

		/// <summary>
		/// Gets the details for a certificate type whose name is provided.
		/// </summary>
		/// <param name="nameId">The name ID of the certificate type whose details are required.</param>
		/// <returns></returns>
		public CertificateTypeDetailsResponse GetCertificateTypeDetails(CertificateTypeDetailsRequest detailsRequest)
		{
			CertificateTypeDetailsResponse detailsResponse = new CertificateTypeDetailsResponse();
			CertCentralResponse response = Request(detailsRequest, detailsRequest.BuildParameters());

			if (IsError(response.Response, detailsResponse.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				detailsResponse.Status = CertCentralBaseResponse.StatusType.ERROR;
				detailsResponse.Errors = errors.errors;
			}
			else
			{
				detailsResponse = JsonConvert.DeserializeObject<CertificateTypeDetailsResponse>(response.Response);
			}

			return detailsResponse;
		}

		/// <summary>
		/// Returns a collection of information on certificate types available through DigiCert.
		/// </summary>
		/// <returns></returns>
		public CertificateTypesResponse GetAllCertificateTypes()
		{
			CertificateTypesRequest typesRequest = new CertificateTypesRequest();
			CertificateTypesResponse allTypes = new CertificateTypesResponse();
			CertCentralResponse response = Request(typesRequest);

			if (IsError(response.Response, allTypes.ContentType))
			{
				Errors errors = JsonConvert.DeserializeObject<Errors>(response.Response);
				allTypes.Status = CertCentralBaseResponse.StatusType.ERROR;
				allTypes.Errors = errors.errors;
			}
			else
			{
				allTypes = JsonConvert.DeserializeObject<CertificateTypesResponse>(response.Response);
			}

			return allTypes;
		}
	}
}