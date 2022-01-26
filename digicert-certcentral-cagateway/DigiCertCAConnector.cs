﻿using CAProxy.AnyGateway;
using CAProxy.AnyGateway.Interfaces;
using CAProxy.AnyGateway.Models;
using CAProxy.AnyGateway.Models.Configuration;
using CAProxy.Common;
using CAProxy.Common.Config;
using CAProxy.Models;

using CSS.Common;

using Keyfactor.Extensions.AnyGateway.DigiCert.API;
using Keyfactor.Extensions.AnyGateway.DigiCert.Client;

using Microsoft.Extensions.Logging;

using Org.BouncyCastle.Asn1.X509;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

using static CAProxy.Common.RequestUtilities;
using static CSS.PKI.PKIConstants.Microsoft;

using DigiCertConstants = Keyfactor.Extensions.AnyGateway.DigiCert.Constants;

namespace Keyfactor.Extensions.AnyGateway.DigiCert
{
	public partial class DigiCertCAConnector : BaseCAConnector, ICAConnectorConfigInfoProvider
	{
		#region Fields and Constructors

		/// <summary>
		/// Provides configuration information for the <see cref="DigiCertCAConnector"/>.
		/// </summary>
		private ICAConnectorConfigProvider ConfigProvider { get; set; }

		private Dictionary<int, string> DCVTokens { get; } = new Dictionary<int, string>();

		#endregion Fields and Constructors

		#region ICAConnector Methods

		public override void Initialize(ICAConnectorConfigProvider configProvider)
		{
			ConfigProvider = configProvider;
		}

		/// <summary>
		/// Enrolls for a certificate.
		/// </summary>
		/// <param name="csr">The CSR being used to enroll</param>
		/// <param name="subject">The subject of the certificate.</param>
		/// <param name="san">The collection of SANs associated with the request as attributes.</param>
		/// <param name="productInfo">Information about the product being enrolled for.</param>
		/// <param name="requestFormat">The format the CSR is in.</param>
		/// <param name="enrollmentType">The type of enrollment being performed.</param>
		/// <returns></returns>
		[Obsolete]
		public override EnrollmentResult Enroll(string csr, string subject, Dictionary<string, string[]> san, EnrollmentProductInfo productInfo, CSS.PKI.PKIConstants.X509.RequestFormat requestFormat, EnrollmentType enrollmentType)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Enrolls for a certificate through the DigiCert API.
		/// </summary>
		/// <param name="certificateDataReader">Reads certificate data from the database.</param>
		/// <param name="csr">The certificate CSR in PEM format.</param>
		/// <param name="subject">The subject of the certificate request.</param>
		/// <param name="san">Any sans added to the request.</param>
		/// <param name="productInfo">Information about the CA product type.</param>
		/// <param name="requestFormat">The format of the request.</param>
		/// <param name="enrollmentType">The type of the enrollment, i.e. new, renew, or reissue.</param>
		/// <returns></returns>
		public override EnrollmentResult Enroll(ICertificateDataReader certificateDataReader, string csr, string subject, Dictionary<string, string[]> san, EnrollmentProductInfo productInfo, CSS.PKI.PKIConstants.X509.RequestFormat requestFormat, EnrollmentType enrollmentType)
		{
			OrderResponse orderResponse = new OrderResponse();
			CertCentralCertType certType = (CertCentralCertType)CertCentralCertType.GetAllTypes(ConfigProvider).FirstOrDefault(x => x.ProductCode.Equals(productInfo.ProductID));
			OrderRequest orderRequest = new OrderRequest(certType);

			var days = (productInfo.ProductParameters.ContainsKey("LifetimeDays")) ? int.Parse(productInfo.ProductParameters["LifetimeDays"]) : 365;
			// Determining if this is a yearly validity or a specific date
			int validityYears = 0;
			DateTime? customExpirationDate = null;
			switch (days)
			{
				case 365:
				case 730:
				case 1095:
					validityYears = days / 365;
					break;

				default:
					customExpirationDate = DateTime.Now.AddDays(days);
					break;
			}

			// Parse subject
			X509Name subjectParsed = new X509Name(subject);
			string commonName = subjectParsed.GetValueList(X509Name.CN).Cast<string>().LastOrDefault();
			string organization = subjectParsed.GetValueList(X509Name.O).Cast<string>().LastOrDefault();

			if (productInfo.ProductParameters.TryGetValue(DigiCertConstants.RequestAttributes.ORGANIZATION_NAME, out string orgName))
			{
				// If org name is provided as a parameter, it overrides whatever is in the CSR
				organization = orgName;
			}

			string signatureHash = certType.signatureAlgorithm;

			List<string> dnsNames = new List<string>();
			if (san.ContainsKey("Dns"))
			{
				dnsNames = new List<string>(san["Dns"]);
			}

			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(ConfigProvider);
			int? organizationId = null;
			// DV certs have no organization, so only do the org check if its a non-DV cert
			if (!string.Equals(productInfo.ProductID, DigiCertConstants.ProductTypes.DV_SSL_CERT, StringComparison.OrdinalIgnoreCase))
			{
				ListOrganizationsResponse organizations = client.ListOrganizations(new ListOrganizationsRequest());
				if (organizations.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					Logger.Error($"Error from CertCentral client: {organizations.Errors.First().message}");
				}

				Organization org = organizations.Organizations.FirstOrDefault(x => x.Name.Equals(organization, StringComparison.OrdinalIgnoreCase));
				if (org != null)
				{
					organizationId = org.Id;
				}
				else
				{
					throw new Exception($"Organization '{organization}' is invalid for this account, please check name");
				}
			}

			// Set up request
			orderRequest.Certificate.CommonName = commonName;
			orderRequest.Certificate.CSR = csr;
			orderRequest.Certificate.SignatureHash = signatureHash;
			orderRequest.Certificate.DNSNames = dnsNames;
			orderRequest.SetOrganization(organizationId);

			string dcvMethod = "email";

			// AnyGateway Core does not currently support retreiving DCV tokens, the following code block can be uncommented once support is added.

			//if (productInfo.ProductParameters.TryGetValue(DigiCertConstants.RequestAttributes.DCV_METHOD, out string rawDCV))
			//{
			//	Logger.Trace($"Parsing DCV method: {rawDCV}");
			//	if (rawDCV.IndexOf("mail", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'email'");
			//		dcvMethod = "email";
			//	}
			//	else if (rawDCV.IndexOf("dns", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'dns-txt-token'");
			//		dcvMethod = "dns-txt-token";
			//	}
			//	else if (rawDCV.IndexOf("http", StringComparison.OrdinalIgnoreCase) >= 0)
			//	{
			//		Logger.Trace("Selecting DCV method 'http-token'");
			//		dcvMethod = "http-token";
			//	}
			//	else
			//	{
			//		Logger.Warn($"Unexpected DCV method '{rawDCV}'. Falling back to default of 'email'");
			//	}
			//}

			orderRequest.DCVMethod = dcvMethod;
			if (customExpirationDate != null)
			{
				orderRequest.CustomExpirationDate = customExpirationDate;
			}
			else
			{
				orderRequest.ValidityYears = validityYears;
			}

			Logger.Trace("Making request to Enroll");
			switch (enrollmentType)
			{
				case EnrollmentType.New:
					return EnrollForCertificate(client, orderRequest, commonName);

				case EnrollmentType.Reissue:
					return Reissue(client, productInfo, certificateDataReader, commonName, csr, dnsNames, signatureHash);

				case EnrollmentType.Renew:
					return Renew(client, orderRequest, productInfo, commonName);

				default:
					throw new Exception($"The enrollment type '{enrollmentType}' is invalid for the DigiCert gateway.");
			}
		}

		/// <summary>
		/// Returns a single certificate record by its serial number.
		/// </summary>
		/// <param name="caRequestID">The CA request ID for the certificate (presently the serial number).</param>
		/// <returns></returns>
		public override CAConnectorCertificate GetSingleRecord(string caRequestId)
		{
			// Split ca request id into order and cert id
			string[] idParts = caRequestId.Split('-');
			int orderId = Int32.Parse(idParts.First());
			string certId = idParts.Last();

			// Get status of cert and the cert itself from Digicert
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(ConfigProvider);

			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));
			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				string errorMessage = String.Format("Request {0} was not found in CertCentral database or is not valid", orderId);
				Logger.Info(errorMessage);
				throw new COMException(errorMessage, HRESULTs.PROP_NOT_FOUND);
			}

			string certificate = null;
			int status = GetCertificateStatusFromCA(orderResponse);
			if (status == (int)RequestDisposition.ISSUED || status == (int)RequestDisposition.REVOKED || status == (int)RequestDisposition.UNKNOWN)
			{
				// We have a status where there may be a cert to download, try to download it
				CertificateChainResponse certificateChainResponse = client.GetCertificateChain(new CertificateChainRequest(certId));
				if (certificateChainResponse.Status == CertCentralBaseResponse.StatusType.SUCCESS)
				{
					certificate = certificateChainResponse.Intermediates[0].PEM;
				}
				else
				{
					Logger.Warn($"Unexpected error downloading certificate {certId} for order {orderId}: {certificateChainResponse.Errors.FirstOrDefault()?.message}");
				}
			}

			return new CAConnectorCertificate
			{
				CARequestID = caRequestId,
				Certificate = certificate,
				Status = status,
				ProductID = orderResponse.product.name_id,
				SubmissionDate = orderResponse.date_created,
				RevocationDate = GetRevocationDate(orderResponse),
				ResolutionDate = orderResponse.certificate.valid_from
			};
		}

		/// <summary>
		/// Attempts to reach the CA over the network.
		/// </summary>
		public override void Ping()
		{
			try
			{
				CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(ConfigProvider);

				Logger.Debug("Attempting to ping DigiCert API.");
				ListDomainsResponse response = client.ListDomains(new ListDomainsRequest());

				if (response.Errors.Count > 0)
				{
					throw new Exception($"Error attempting to ping DigiCert: {string.Join("\n", response.Errors)}");
				}

				Logger.Debug("Successfully pinged DigiCert API.");
			}
			catch (Exception e)
			{
				Logger.Error($"There was an error contacting DigiCert: {e.Message}.");
				throw new Exception($"Error attempting to ping DigiCert: {e.Message}.", e);
			}
		}

		/// <summary>
		/// Revokes a certificate by its serial number.
		/// </summary>
		/// <param name="caRequestID">The CA request ID (presently the serial number).</param>
		/// <param name="hexSerialNumber">The hex-encoded serial number.</param>
		/// <param name="revocationReason">The revocation reason.</param>
		public override int Revoke(string caRequestID, string hexSerialNumber, uint revocationReason)
		{
			int orderId = Int32.Parse(caRequestID.Substring(0, caRequestID.IndexOf('-')));
			string certId = caRequestID.Substring(caRequestID.IndexOf('-') + 1);
			CertCentralClient client = CertCentralClientUtilities.BuildCertCentralClient(ConfigProvider);
			ViewCertificateOrderResponse orderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));
			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR || orderResponse.status.ToLower() != "issued")
			{
				string errorMessage = String.Format("Request {0} was not found in CertCentral database or is not valid", orderId);
				Logger.Info(errorMessage);
				throw new COMException(errorMessage, HRESULTs.PROP_NOT_FOUND);
			}
			string req = "";
			RequestSummary request_temp = orderResponse.requests.FirstOrDefault(x => x.status == "approved");
			if (request_temp != null && !String.IsNullOrEmpty(request_temp.comments) && request_temp.comments.Contains("CERTIFICATE_REQUESTOR:"))
			{
				req = request_temp.comments.Replace("CERTIFICATE_REQUESTOR:", "").Trim();
			}
			Logger.Trace("Making request to Revoke");
			RevokeCertificateResponse revokeResponse = client.RevokeCertificate(new RevokeCertificateByOrderRequest(orderResponse.id) { comments = Conversions.RevokeReasonToString(revocationReason) });

			ViewCertificateOrderResponse secondOrderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderId));
			RequestSummary requestSummary = secondOrderResponse.requests.FirstOrDefault(x => x.type.Equals("revoke") && x.status.Equals("pending"));
			UpdateRequestStatusResponse updateRequest = new UpdateRequestStatusResponse();
			if (requestSummary != null)
			{
				updateRequest = client.UpdateRequestStatus(new UpdateRequestStatusRequest(requestSummary.id) { Status = "approved" });
			}
			else
			{
				Logger.Warn($"No pending revocation found for Order {orderId}. Order may not have been revoked successfully");
			}
			CAConnectorCertificate revokedCert = GetSingleRecord(caRequestID);
			return revokedCert.Status;
		}

		/// <summary>
		/// Synchronizes the CA with Keyfactor Command.
		/// </summary>
		/// <param name="certificateDataReader">Provides information about the gateway's certificates.</param>
		/// <param name="blockingBuffer">Buffer into which certificates are placed from the CA.</param>
		/// <param name="certificateAuthoritySyncInfo">Information about the last CA sync.</param>
		/// <param name="cancelToken">The cancellation token.</param>
		[Obsolete]
		public override void Synchronize(ICertificateDataReader certificateDataReader, BlockingCollection<CertificateRecord> blockingBuffer, CertificateAuthoritySyncInfo certificateAuthoritySyncInfo, CancellationToken cancelToken, string reservedUnused)
		{
		}

		/// <summary>
		/// Synchronizes the gateway with the external CA.
		/// </summary>
		/// <param name="certificateDataReader">Provides information about the gateway's certificates.</param>
		/// <param name="blockingBuffer">Buffer into which certificates are placed from the CA.</param>
		/// <param name="certificateAuthoritySyncInfo">Information about the last CA sync.</param>
		/// <param name="cancelToken">The cancellation token.</param>
		public override void Synchronize(ICertificateDataReader certificateDataReader, BlockingCollection<CAConnectorCertificate> blockingBuffer, CertificateAuthoritySyncInfo certificateAuthoritySyncInfo, CancellationToken cancelToken)
		{
			bool fullSync = certificateAuthoritySyncInfo.DoFullSync;
			DateTime? lastSync = certificateAuthoritySyncInfo.OverallLastSync;
			lastSync = lastSync.HasValue ? lastSync.Value.AddHours(-7) : DateTime.MinValue;  // We are doing this for a current issue with Digicert treating the time zone as mountain time. -7 to accomodate daylight saving
			DateTime? utcDate = DateTime.UtcNow.AddDays(1); // We are adding a day to catch any change digicert makes without us noticing?
			string lastSyncFormat = FormatSyncDates(lastSync);
			string todaySyncFormat = FormatSyncDates(utcDate);

			List<CAConnectorCertificate> certs = new List<CAConnectorCertificate>();
			List<StatusOrder> certsToSync = new List<StatusOrder>();
			Logger.Debug("Attempting to create a Cert Central Client");

			CertCentralClient digiClient = CertCentralClientUtilities.BuildCertCentralClient(ConfigProvider);

			List<string> skippedOrders = new List<string>();

			if (fullSync)
			{
				ListCertificateOrdersResponse orderResponse = digiClient.ListAllCertificateOrders();
				if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					Error error = orderResponse.Errors[0];
					Logger.Error("Error in listing all certificate orders");
					throw new Exception($"DigiCert CertCentral Web Service returned {error.code} - {error.message} to retreive all rows");
				}
				else
				{
					foreach (Order certDetails in orderResponse.orders)
					{
						cancelToken.ThrowIfCancellationRequested();

						StatusOrder fullCert = new StatusOrder
						{
							order_id = certDetails.id,
							certificate_id = certDetails.certificate.id,
							status = certDetails.status
						};
						certsToSync.Add(fullCert);
						if (certDetails.has_duplicates)
						{
							certsToSync.AddRange(GetDuplicates(digiClient, certDetails.id));
						}
					}
				}
			}
			else
			{
				StatusChangesResponse statusResponse = digiClient.StatusChanges(new StatusChangesRequest(lastSyncFormat, todaySyncFormat));
				if (statusResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					Error error = statusResponse.Errors[0];
					Logger.Error("Error in grabbing certificates for partial sync");
					throw new Exception($"DigiCert CertCentral Web Service returned {error.code} - {error.message} to retreive all rows");
				}
				if (statusResponse.orders?.Count > 0)
				{
					int orderCount = statusResponse.orders.Count;
					certsToSync = statusResponse.orders;
					for (int i = 0; i < orderCount; i++)
					{
						cancelToken.ThrowIfCancellationRequested();

						List<StatusOrder> dupeCerts = GetDuplicates(digiClient, statusResponse.orders[i].order_id);
						if (dupeCerts?.Count > 0)
						{
							certsToSync.AddRange(dupeCerts);
						}
					}
				}
			}

			if (certsToSync?.Count > 0)
			{
				foreach (StatusOrder order in certsToSync)
				{
					cancelToken.ThrowIfCancellationRequested();

					string caRequestId = order.order_id + "-" + order.certificate_id;
					if (order.status.Equals("rejected", StringComparison.OrdinalIgnoreCase))
					{
						skippedOrders.Add(order.certificate_id.ToString());
						continue;
					}
					if (order.status.Equals("issued", StringComparison.OrdinalIgnoreCase) || order.status.Equals("revoked", StringComparison.OrdinalIgnoreCase) || order.status.Equals("approved", StringComparison.OrdinalIgnoreCase))
					{
						CAConnectorCertificate certResponse = GetSingleRecord(caRequestId);

						string certificate = certResponse.Certificate;
						string noHeaders;
						try
						{
							noHeaders = ConfigurationUtils.OnlyBase64CertContent(certificate);
						}
						catch (Exception)
						{
							skippedOrders.Add(order.certificate_id.ToString());
							Logger.Warn($"An error occurred attempting to sync order '{order.certificate_id}'. This order will be skipped.");
							continue;
						}

						CAConnectorCertificate newCert = new CAConnectorCertificate
						{
							CARequestID = caRequestId,
							Certificate = noHeaders,
							Status = certResponse.Status,
							SubmissionDate = certResponse.SubmissionDate,
							ProductID = certResponse.ProductID,
							RevocationDate = certResponse.RevocationDate,
							ResolutionDate = certResponse.ResolutionDate
						};

						CAConnectorCertificate certToAdd = certificateDataReader.GetCertificateRecord(caRequestId, string.Empty);
						if (certToAdd != null)
						{
							certToAdd.SubmissionDate = newCert.SubmissionDate;
							certToAdd.Status = newCert.Status;
							certToAdd.Certificate = newCert.Certificate;
							certToAdd.ResolutionDate = newCert.ResolutionDate;
							certToAdd.RevocationDate = newCert.RevocationDate;
							certToAdd.RevocationReason = newCert.RevocationReason;
							certToAdd.ProductID = newCert.ProductID;
						}
						else
						{
							certToAdd = newCert;
						}
						certs.Add(certToAdd);
					}
				}
			}

			if (cancelToken.IsCancellationRequested)
			{
				Logger.Info("DigiCert sync cancelled.");

				// Throwing here for consistent behavior on cancellation.
				cancelToken.ThrowIfCancellationRequested();
			}

			if (skippedOrders?.Count > 0)
			{
				Logger.Info($"Sync skipped the following orders: {string.Join(",", skippedOrders.ToArray())}");
			}

			foreach (CAConnectorCertificate record in certs)
			{
				blockingBuffer.Add(record, cancelToken);
			}
		}

		/// <summary>
		/// Validates that the CA connection info is correct.
		/// </summary>
		/// <param name="connectionInfo">The information used to connect to the CA.</param>
		public override void ValidateCAConnectionInfo(Dictionary<string, object> connectionInfo)
		{
			Logger.Trace("Entering 'ValidateCAConnectionInfo' method.");
			List<string> errors = new List<string>();

			Logger.Trace("Checking the API Admin Key.");
			string apiKey = connectionInfo.ContainsKey(DigiCertConstants.Config.APIKEY) ? (string)connectionInfo[DigiCertConstants.Config.APIKEY] : string.Empty;
			if (string.IsNullOrWhiteSpace(apiKey))
			{
				errors.Add("The API Admin Key is required.");
			}

			CertCentralClient digiClient = new CertCentralClient(apiKey);
			ListDomainsResponse domains = digiClient.ListDomains(new ListDomainsRequest());
			Logger.Debug("Domain Status: " + domains.Status);
			if (domains.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				Logger.Error($"Error from CertCentral client: {domains.Errors[0].message}");
				errors.Add("Error grabbing DigiCert domains");
			}
			Logger.Trace("Leaving 'ValidateCAConnectionInfo' method.");

			// We cannot proceed if there are any errors.
			if (errors.Any())
			{
				ThrowValidationException(errors);
			}
		}

		/// <summary>
		/// Validates that the product information for the CA is correct.
		/// </summary>
		/// <param name="productInfo">The product information.</param>
		public override void ValidateProductInfo(EnrollmentProductInfo productInfo, Dictionary<string, object> connectionInfo)
		{
			// Set up.
			string productId = productInfo.ProductID;
			string apiKey = (string)connectionInfo[DigiCertConstants.Config.APIKEY];
			CertCentralClient client = new CertCentralClient(apiKey);

			// Get the available types and check that it's one of them.
			// We're doing this because to get the list of valid product IDs in a comment, the user must have at least one correct product/template mapping.
			// We therefore need to have some way of telling them what the valid product IDs are to begin with.
			CertificateTypesResponse productIdResponse = client.GetAllCertificateTypes();
			if (productIdResponse.Status != CertCentralBaseResponse.StatusType.SUCCESS)
			{
				throw new Exception($"The product types could not be retrieved from the server. The following errors occurred: {string.Join(" ", productIdResponse.Errors.Select(x => x.message))}");
			}

			// Get product and check if it exists.
			var product = productIdResponse.Products.FirstOrDefault(x => x.NameId.Equals(productId, StringComparison.InvariantCultureIgnoreCase));
			if (product == null)
			{
				throw new Exception($"The product ID '{productId}' does not exist. The following product IDs are valid: {string.Join(", ", productIdResponse.Products.Select(x => x.NameId))}");
			}

			// Get product ID details.
			CertificateTypeDetailsResponse details = client.GetCertificateTypeDetails(product.NameId);
			if (details.Errors.Any())
			{
				throw new Exception($"Validation of '{productId}' failed for the following reasons: {string.Join(" ", details.Errors.Select(x => x.message))}.");
			}
		}

		#endregion ICAConnector Methods

		#region ICAConnectorConfigInfoProvider Methods

		/// <summary>
		/// Returns the default CA connector section of the config file.
		/// </summary>
		public Dictionary<string, object> GetDefaultCAConnectorConfig()
		{
			return new Dictionary<string, object>()
			{
				{ DigiCertConstants.Config.APIKEY, "" }
			};
		}

		/// <summary>
		/// Gets the default comment on the default product type.
		/// </summary>
		/// <returns></returns>
		public string GetProductIDComment()
		{
			if (ConfigProvider == null)
			{
				throw new NotImplementedException();
			}

			try
			{
				string authAPIKey = (string)ConfigProvider.CAConnectionData[DigiCertConstants.Config.APIKEY];
				CertCentralClient client = new CertCentralClient(authAPIKey);

				// Get product types.
				CertificateTypesResponse productTypesResponse = client.GetAllCertificateTypes();

				// If we couldn't get the types, return an empty comment.
				if (productTypesResponse.Status != CertCentralBaseResponse.StatusType.SUCCESS)
				{
					return string.Empty;
				}

				// Return comment.
				return "Available DigiCert product types are: " + string.Join(", ", productTypesResponse.Products.Select(x => x.Name));
			}
			catch
			{
				// Swallow exceptions and return an empty string.
				return string.Empty;
			}
		}

		/// <summary>
		/// Gets annotations for the CA connector properties.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, PropertyConfigInfo> GetCAConnectorAnnotations()
		{
			return new Dictionary<string, PropertyConfigInfo>()
			{
				[DigiCertConstants.Config.APIKEY] = new PropertyConfigInfo()
				{
					Comments = "API Key for connecting to DigiCert",
					Hidden = true,
					DefaultValue = ""
				}
			};
		}

		/// <summary>
		/// Gets annotations for the template mapping parameters.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, PropertyConfigInfo> GetTemplateParameterAnnotations()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets default template map parameters for DigiCert product types.
		/// </summary>
		/// <returns></returns>
		public Dictionary<string, string> GetDefaultTemplateParametersConfig()
		{
			throw new NotImplementedException();
		}

		public int GetCertificateStatusFromCA(ViewCertificateOrderResponse oCertificateOrderResponse)
		{
			switch (oCertificateOrderResponse.status)
			{
				case "issued":
					return (int)RequestDisposition.ISSUED;

				case "reissue_pending":
				case "pending": // Pending from DigiCert means it will be issued after validation
					return (int)RequestDisposition.EXTERNAL_VALIDATION;

				case "denied":
					return (int)RequestDisposition.DENIED;

				case "revoked":
					return (int)RequestDisposition.REVOKED;

				case "needs_approval": // This indicates that the request has to be approved through DigiCert, which is a misconfiguration
					Logger.Warn($"Order {oCertificateOrderResponse.id} needs to be approved in the DigiCert portal prior to issuance");
					return (int)RequestDisposition.EXTERNAL_VALIDATION;

				default:
					Logger.Warn($"Order {oCertificateOrderResponse.id} has unexpected status {oCertificateOrderResponse.status}");
					return (int)RequestDisposition.UNKNOWN;
			}
		}

		#endregion ICAConnectorConfigInfoProvider Methods

		#region Helpers

		/// <summary>
		/// Get the revocation date tied to a certificate order.
		/// </summary>
		/// <param name="order">The order we seek the revocation date for.</param>
		/// <returns></returns>
		private DateTime? GetRevocationDate(ViewCertificateOrderResponse order)
		{
			if (order.Status != CertCentralBaseResponse.StatusType.SUCCESS)
			{
				Logger.Warn($"Could not retrieve the revocation date for order '{order.id}'. This may cause problems syncing with Command.");
				return null;
			}

			RequestSummary revokeRequest = order.requests.FirstOrDefault(x => x.type.Equals("revoke", StringComparison.OrdinalIgnoreCase) &&
				"approved".Equals(x.status, StringComparison.OrdinalIgnoreCase));
			if (revokeRequest == null)
			{
				if ("revoked".Equals(order.status, StringComparison.OrdinalIgnoreCase))
				{
					Logger.Warn($"Order '{order.id}' is revoked, but lacks a revoke request and revocation date. This may cause problems syncing with Command.");
				}

				return null;
			}

			return revokeRequest.date;
		}

		/// <summary>
		/// Throws an exception with the concatenated errors.
		/// </summary>
		/// <param name="errors">The errors we want to see in the exception.</param>
		private void ThrowValidationException(List<string> errors)
		{
			throw new Exception(string.Join("\n", errors));
		}

		/// <summary>
		/// Enrolls for a certificate.
		/// </summary>
		/// <param name="client">The client that makes requests to DigiCert.</param>
		/// <param name="request">The request to order a certificate.</param>
		/// <param name="template">The template corresponding to the product type.</param>
		/// <param name="commonName">The common name.</param>
		/// <returns></returns>
		private EnrollmentResult EnrollForCertificate(CertCentralClient client, OrderRequest request, string commonName)
		{
			Logger.Trace("Attempting to enroll for a certificate.");
			return ExtractEnrollmentResult(client, client.OrderCertificate(request, true), commonName);
		}

		/// <summary>
		/// Renews a certificate.
		/// </summary>
		/// <param name="client">The client used to contact DigiCert.</param>
		/// <param name="request">The <see cref="OrderRequest"/>.</param>
		/// <param name="enrollmentProductInfo">Information about the DigiCert product this certificate uses.</param>
		/// <returns></returns>
		private EnrollmentResult Renew(CertCentralClient client, OrderRequest request, EnrollmentProductInfo enrollmentProductInfo, string commonName)
		{
			CheckProductExistence(enrollmentProductInfo.ProductID);

			Logger.Trace("Attempting to renew certificate.");
			return ExtractEnrollmentResult(client, client.OrderCertificate(request, true), commonName);
		}

		/// <summary>
		/// Renews a certificate.
		/// </summary>
		/// <param name="client">The client used to contact DigiCert.</param>
		/// <param name="request">The <see cref="OrderRequest"/>.</param>
		/// <param name="enrollmentProductInfo">Information about the DigiCert product this certificate uses.</param>
		/// <returns></returns>
		private EnrollmentResult Reissue(CertCentralClient client, EnrollmentProductInfo enrollmentProductInfo, ICertificateDataReader certificateDataReader, string commonName, string csr, List<string> dnsNames, string signatureHash)
		{
			CheckProductExistence(enrollmentProductInfo.ProductID);

			// Get the old cert so we can properly construct the request.
			string priorCertSnString = enrollmentProductInfo.ProductParameters[CAProxy.AnyGateway.Constants.Attribute.PRIOR_CERT_SN];
			Logger.Trace($"Attempting to retrieve the certificate with serial number {priorCertSnString}.");
			byte[] priorCertSn = DataConversion.HexToBytes(priorCertSnString);
			CAConnectorCertificate anyGatewayCertificate = certificateDataReader.GetCertificateRecord(priorCertSn);
			if (anyGatewayCertificate == null)
			{
				throw new Exception($"No certificate with serial number '{priorCertSnString}' could be found.");
			}

			// Get order ID
			Logger.Trace("Attempting to parse the order ID from the AnyGateway certificate.");
			uint orderId = 0;
			try
			{
				orderId = uint.Parse(anyGatewayCertificate.CARequestID.Split('-').First());
			}
			catch (Exception e)
			{
				throw new Exception($"There was an error parsing the order ID from the certificate: {e.Message}", e);
			}

			// Reissue certificate.
			ReissueRequest reissueRequest = new ReissueRequest(orderId)
			{
				Certificate = new CertificateReissueRequest
				{
					CommonName = commonName,
					CSR = csr,
					DnsNames = dnsNames,
					SignatureHash = signatureHash
				},
				// Setting SkipApproval to true to allow certificate id to return a value. See DigiCert documentation on Reissue API call for more info.
				SkipApproval = true
			};

			Logger.Trace("Attempting to reissue certificate.");
			return ExtractEnrollmentResult(client, client.ReissueCertificate(reissueRequest, true), commonName);
		}

		/// <summary>
		/// Gets the enrollment result from an <see cref="OrderResponse"/> object.
		/// </summary>
		private EnrollmentResult ExtractEnrollmentResult(CertCentralClient client, OrderResponse orderResponse, string commonName)
		{
			int status = (int)RequestDisposition.UNKNOWN;
			string statusMessage = null;
			string certificate = null;
			string caRequestID = null;

			if (orderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				Logger.Error($"Error from CertCentral client: {orderResponse.Errors.First().message}");

				status = (int)RequestDisposition.FAILED;
				statusMessage = orderResponse.Errors[0].message;
			}
			else if (orderResponse.Status == CertCentralBaseResponse.StatusType.SUCCESS)
			{
				uint orderID = (uint)orderResponse.OrderId;
				ViewCertificateOrderResponse certificateOrderResponse = client.ViewCertificateOrder(new ViewCertificateOrderRequest(orderID));
				if (certificateOrderResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
				{
					string errorMessage = $"Order {orderID} was not found for rejection in CertCentral database";
					Logger.Info(errorMessage);
					throw new UnsuccessfulRequestException(errorMessage, unchecked((uint)HRESULTs.BAD_REQUEST_STATUS));
				}

				status = GetCertificateStatusFromCA(certificateOrderResponse);

				// Get cert from response
				if (orderResponse.CertificateChain != null)
				{
					Logger.Trace($"Certificate for order {orderResponse.OrderId} was immediately issued");
					string certPem = orderResponse.CertificateChain.SingleOrDefault(c => c.SubjectCommonName.Equals(commonName, StringComparison.OrdinalIgnoreCase))?.PEM;
					if (string.IsNullOrEmpty(certPem))
					{
						Logger.Warn($"Order {orderResponse.OrderId} was for Common Name '{commonName}', but no certificate with that Common Name was returned");
					}

					certificate = certPem;
					caRequestID = orderResponse.OrderId.ToString() + "-" + orderResponse.CertificateId;
				}
				else if (orderResponse.CertificateId.HasValue)
				{
					Logger.Trace($"Certificate for order {orderResponse.OrderId} is being processed by DigiCert. Most likely a domain/organization requires further validation");
					if (!string.IsNullOrEmpty(orderResponse.DCVRandomValue))
					{
						Logger.Debug($"Saving DCV token for order {orderResponse.OrderId}");
						DCVTokens[orderResponse.OrderId] = orderResponse.DCVRandomValue;
					}

					caRequestID = orderResponse.OrderId.ToString() + "-" + orderResponse.CertificateId;
				}
				else // We should really only get here if there is a misconfiguration (e.g. set up for approval in DigiCert)
				{
					Logger.Warn($"Order {orderResponse.OrderId} did not return a CertificateId. Manual intervention may be required");
					if (orderResponse.Requests.Any(x => x.Status == DigiCertConstants.Status.PENDING))
					{
						Logger.Trace($"Attempting to approve order '{orderResponse.OrderId}'.");

						// Attempt to update the request status.
						int requestId = int.Parse(orderResponse.Requests.FirstOrDefault(x => x.Status == DigiCertConstants.Status.PENDING).Id);
						UpdateRequestStatusRequest updateStatusRequest = new UpdateRequestStatusRequest(requestId, DigiCertConstants.Status.APPROVED);
						UpdateRequestStatusResponse updateStatusResponse = client.UpdateRequestStatus(updateStatusRequest);

						if (updateStatusResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
						{
							string errors = string.Join(" ", updateStatusResponse.Errors.Select(x => x.message));
							Logger.Error($"The order '{orderResponse.OrderId}' could not be approved: '{errors}");

							caRequestID = orderResponse.OrderId.ToString();
							if (updateStatusResponse.Errors.Any(x => x.code == "access_denied|invalid_approver"))
							{
								status = (int)RequestDisposition.EXTERNAL_VALIDATION;
								statusMessage = errors;
							}
							else
							{
								status = (int)RequestDisposition.FAILED;
								statusMessage = $"Approval of order '{orderResponse.OrderId}' failed. Check the gateway logs for more details.";
							}
						}
						else // If the request was successful, we attempt to retrieve the certificate.
						{
							ViewCertificateOrderResponse order = client.ViewCertificateOrder(new ViewCertificateOrderRequest((uint)orderResponse.OrderId));

							// We don't worry about failures here, since the sync will update the cert if we can't get it right now for some reason.
							if (order.Status != CertCentralBaseResponse.StatusType.ERROR)
							{
								caRequestID = $"{order.id}-{order.certificate.id}";
								try
								{
									CAConnectorCertificate connCert = GetSingleRecord($"{order.id}-{order.certificate.id}");
									certificate = connCert.Certificate;
									status = connCert.Status;
									statusMessage = $"Post-submission approval of order {order.id} returned success";
								}
								catch (Exception getRecordEx)
								{
									Logger.Warn($"Unable to retrieve certificate {order.certificate.id} for order {order.id}: {getRecordEx.Message}");
									status = (int)RequestDisposition.UNKNOWN;
									statusMessage = $"Post-submission approval of order {order.id} was successful, but pickup failed";
								}
							}
						}
					}
					else
					{
						Logger.Warn("The request disposition is for this enrollment could not be determined.");
						status = (int)RequestDisposition.UNKNOWN;
						statusMessage = "The request disposition could not be determined.";
					}
				}
			}
			return new EnrollmentResult
			{
				CARequestID = caRequestID,
				Certificate = certificate,
				Status = status,
				StatusMessage = statusMessage
			};
		}

		private void CheckProductExistence(string productId)
		{
			// Check that the product type is still valid.
			Logger.Trace($"Checking that the product '{productId}' exists.");
			CABaseCertType productType = CertCentralCertType.GetAllTypes(ConfigProvider).FirstOrDefault(x => x.ProductCode.Equals(productId, StringComparison.InvariantCultureIgnoreCase));
			if (productType == null)
			{
				throw new Exception($"The product type '{productId}' does not exist.");
			}
		}

		private int? GetPendingRequestId(List<Requests> requests)
		{
			string id = requests.FirstOrDefault(x => x.Status == DigiCertConstants.Status.PENDING).Id;
			return !string.IsNullOrEmpty(id) ? int.Parse(id) : (int?)null;
		}

		private string FormatSyncDates(DateTime? syncTime)
		{
			string date = syncTime.Value.Year + "-" + syncTime.Value.Month + "-" + syncTime.Value.Day;
			string time = syncTime.Value.TimeOfDay.Hours + ":" + syncTime.Value.TimeOfDay.Minutes + ":" + syncTime.Value.TimeOfDay.Seconds;
			return date + "+" + time;
		}

		private List<StatusOrder> GetDuplicates(CertCentralClient digiClient, int orderId)
		{
			Logger.Trace($"Getting Duplicates for order {orderId}");
			List<StatusOrder> dupeCerts = new List<StatusOrder>();
			ListDuplicatesResponse duplicateResponse = digiClient.ListDuplicates(new ListDuplicatesRequest(orderId));
			if (duplicateResponse.Status == CertCentralBaseResponse.StatusType.ERROR)
			{
				Error error = duplicateResponse.Errors[0];
				Logger.Error($"Error in retrieving duplicates for order {orderId}");
				throw new Exception($"DigiCert CertCentral Web Service returned {error.code} - {error.message} to retreive all rows");
			}
			if (duplicateResponse.certificates?.Count > 0)
			{
				foreach (CertificateOrder dupeCert in duplicateResponse.certificates)
				{
					StatusOrder dupeStatusOrder = new StatusOrder
					{
						order_id = orderId,
						certificate_id = dupeCert.id,
						status = dupeCert.status
					};
					dupeCerts.Add(dupeStatusOrder);
				}
			}

			return dupeCerts;
		}

		#endregion Helpers
	}
}