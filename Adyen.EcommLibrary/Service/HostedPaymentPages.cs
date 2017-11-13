﻿using Adyen.EcommLibrary.Constants;
using Adyen.EcommLibrary.Constants.HPPConstants;
using Adyen.EcommLibrary.Model.Hpp;
using Adyen.EcommLibrary.Util;
using System;
using System.Collections.Generic;

namespace Adyen.EcommLibrary.Service
{
    public class HostedPaymentPages : AbstractService
    {
        public HostedPaymentPages(Client client)
            : base(client)
        {
        }

        public string DirectoryLookup(Dictionary<string, string> postParameters)
        {
            var endpoint = ClientConstants.HppTest + "/directory.shtml";
            var clientInterface = this.Client.HttpClient;
            var config = this.Client.Config;
            return clientInterface.Post(endpoint, postParameters, config);
        }

        public Dictionary<string, string> GetPostParametersFromDlRequest(DirectoryLookupRequest request)
        {
            var config = this.Client.Config;

            var postParameters = new Dictionary<string, string>();
            postParameters.Add(Fields.CurrencyCode, request.CurrencyCode);

            if (!string.IsNullOrEmpty(request.MerchantAccount))
            {
                postParameters.Add(Fields.MerchantAccount, request.MerchantAccount);
            }
            else
            {
                postParameters.Add(Fields.MerchantAccount, config.MerchantAccount);
            }
            postParameters.Add(Fields.PaymentAmount, request.PaymentAmount);

            if (!string.IsNullOrEmpty(request.SkinCode))
            {
                postParameters.Add(Fields.SkinCode, request.SkinCode);
            }
            else
            {
                postParameters.Add(Fields.SkinCode, config.SkinCode);
            }

            postParameters.Add(Fields.MerchantReference, request.MerchantReference);
            postParameters.Add(Fields.SessionValidity, request.SessionValidity);
            postParameters.Add(Fields.CountryCode, request.CountryCode);

            var hmacValidator = new HmacValidator();

            var dataToSign = hmacValidator.BuildSigningString(postParameters);

            string hmacKey;
            if (!string.IsNullOrEmpty(request.HmacKey))
            {
                hmacKey = request.HmacKey;
            }
            else
            {
                hmacKey = config.HmacKey;
            }

            var merchantSig = hmacValidator.CalculateHmac(dataToSign, hmacKey);
            postParameters.Add(Fields.MerchantSig, merchantSig);

            return postParameters;
        }

        public List<PaymentMethod> GetPaymentMethods(DirectoryLookupRequest request)
        {
            try
            {
                var postParameters = GetPostParametersFromDlRequest(request);
                var jsonResult = DirectoryLookup(postParameters);
                var directoryLookupResult = Util.JsonOperation.Deserealize<DirectoryLookupResult>(jsonResult);

                return directoryLookupResult.PaymentMethods;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }
    }
}