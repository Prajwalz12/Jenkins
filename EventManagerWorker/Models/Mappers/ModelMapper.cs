using Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Models.Mappers
{
    public static class ModelMapper
    {
        public static CustomerModel.CustomerVersion Map(CustomerModel.Customer customer)
        {
            var model = customer;
            return new CustomerModel.CustomerVersion()
            {
                MobileNumber = model.MobileNumber,
                Type = model.Type,
                TypeId = model.TypeId,
                SignUpDate = model.SignUpDate,
                Wallet = model.Wallet,
                WalletBalance = model.WalletBalance,
                KYC = model.KYC,
                Flags = model.Flags,
                Install = model.Install,
                Segments = model.Segments,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public static TransactionModel.Transaction Map(TransactionModel.Transaction transactionRequest)
        {
            var model = transactionRequest;

            return new TransactionModel.Transaction()
            {
                TransactionId = model.TransactionId,
                MobileNumber = model.MobileNumber,
                LOB = model.LOB,
                EventId = model.EventId,
                ChannelCode = model.ChannelCode,
                ProductCode = model.ProductCode,
                CustomerDetail = GetCustomerDetail(),
                Wallet = model.Wallet,
                Campaign = model.Campaign,
                UTM = model.UTM,
                TransactionDetail = model.TransactionDetail
            };

            Common.TransactionModel.CustomerDetail GetCustomerDetail() => new Common.TransactionModel.CustomerDetail { LoyaltyId = string.Empty, CustomerVersionId = string.Empty };
        }

        public static Common.TransactionModel.CustomerDetail MapCustomerDetail(CustomerModel.Customer customer)
        {
            var model = customer;
            return new Common.TransactionModel.CustomerDetail()
            {
                LoyaltyId = customer.LoyaltyId,
                CustomerVersionId = customer.CustomerVersionId
            };
        }

        #region Customer



        public static EventManagerApi.CustomerRequest ToCustomerRequestModel(this CustomerModel.Customer customer)
        {
            var customerrequest = new EventManagerApi.CustomerRequest()
            {
                CustomerId = customer.LoyaltyId,
                MobileNumber = customer.MobileNumber,
                Type = customer.Type,
                // SignUpDate = customer.SignUpDate.ToString(),
                UpiId = String.IsNullOrEmpty(customer.UPIId) ? string.Empty : customer.UPIId,
                WalletBalance = customer.WalletBalance.ToString()
            };
            if (customer.Flags != null)
            {
                customerrequest.Flags = customer.Flags.ToFlagRequestModel();
            }

            if (customer.KYC != null)
            {
                customerrequest.Kyc = customer.KYC.ToKYCModel();
            }
            if (customer.Install != null)
            {
                customerrequest.InstallMedium = customer.Install.ToInstallModel();
            }
            return customerrequest;
        }

        public static EventManagerApi.FlagRequest ToFlagRequestModel(this Models.Common.CustomerModel.Flag flag)
        {
            return new EventManagerApi.FlagRequest()
            {
                Delinquency = flag.GlobalDeliquient,
                Dormant = flag.Dormant,
                Wallet = flag.Wallet
            };
        }
        //public static CustomerModel.FlagRequest GetDefaultFlagReqyestModel()
        //{
        //    return new CustomerModel.FlagRequest()
        //    {
        //        Delinquency = false,
        //        Dormant = false,
        //        Wallet = false

        //    };
        //}
        public static EventManagerApi.KYCRequest ToKYCModel(this Models.Common.CustomerModel.KYC kyc)
        {
            return new EventManagerApi.KYCRequest()
            {
                Status = kyc.Status.ToString(),
                Date = kyc.CompletedDateTime
            };
        }
        public static EventManagerApi.InstallMedium ToInstallModel(this Models.Common.CustomerModel.Install install)
        {
            return new EventManagerApi.InstallMedium()
            {
                Source = install.Source,
                Channel = install.Channel
            };
        }

        #endregion


    }
}