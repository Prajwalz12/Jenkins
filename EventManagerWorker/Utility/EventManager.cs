using System;
using System.Collections.Generic;
using Domain.Models;

namespace Utility
{
    public static class EventManager
    {
        class Event
        {
            public int EventId { get; set; }
            public string EventName { get; set; }
            public string EventCode { get; set; }
        }
        private static readonly Dictionary<string, string> EventOfferTypeDictionary = new Dictionary<string, string>()
        {
            { "WalletCreation", "Activity"},
            { "WalletLoad", "Activity"},
            { "Spend", "Payment"},
            { "VPACreation", "Activity"},
            //{ "BillPayment", "Hybrid"},
            { "ClearBounceEMI", "Hybrid"},
            { "Signup", "Activity"},
            { "CDFEMI", "Payment"},
            { "DOISSUE", "Activity"},
            { "DICOMPLETED", "Activity"},
            { "EMIREPAYMENT", "Activity"},
            { "GenericActivity", ""},
            { "EMandateCreation", "Activity" },
            { "UPILite", "Activity" }
        };
        private static readonly Dictionary<string, Event> EventCodeAndNameDictionary = new Dictionary<string, Event>()
        {
            {"WalletCreation", new Event() {EventId = 1,  EventCode = "WalletCreation", EventName = "Wallet creation" } },
            {"WalletLoad", new Event() {EventId = 2,  EventCode = "WalletLoad", EventName = "Wallet Load" } },
            {"Spend", new Event() {EventId = 3,  EventCode = "Spend", EventName = "Spend" } },
            {"VPACreation", new Event() {EventId = 4,  EventCode = "VPACreation", EventName = "VPA creation" } },
            {"ClearBounceEMI", new Event() {EventId = 5,  EventCode = "ClearBounceEMI", EventName = "Clear bounce EMI" } },
            {"KYCCompletion", new Event() {EventId = 6,  EventCode = "KYCCompletion", EventName = "KYC Completion" } },
            {"Signup", new Event() {EventId = 7,  EventCode = "Signup", EventName = "Sign up" } },
            {"CustomRewarding", new Event() {EventId = 8,  EventCode = "CustomRewarding", EventName = "Custom Rewarding" } },
            {"CDFinancing", new Event() {EventId = 9,  EventCode = "CDFinancing", EventName = "CD Financing" } },
            {"MRNCreation", new Event() {EventId = 10,  EventCode = "MRNCreation", EventName = "MRN Creation" } },
            {"DOISSUE", new Event() {EventId = 11,  EventCode = "DOISSUE", EventName = "DO Issue" } },
            {"DICOMPLETED", new Event() {EventId = 12,  EventCode = "DICOMPLETED", EventName = "DI Completed" } },
            {"EMIREPAYMENT", new Event() {EventId = 13,  EventCode = "EMIREPAYMENT", EventName = "EMI Repayment" } },
            {"GenericActivity", new Event() {EventId = 14,  EventCode = "GenericActivity", EventName = "Generic Activity" } },
            {"Any", new Event() {EventId = 15,  EventCode = "Any", EventName = "Any" } },
            {"EMandateCreation", new Event() {EventId = 16,  EventCode = "EMandateCreation", EventName = "E Mandate Creation" } },
            {"UPILite", new Event() {EventId = 17,  EventCode = "UPILite", EventName = "UPI Lite" } },
        };
        public static string GetOfferTypeByEventCode(this string eventCode)
        {
            var offerType = String.Empty;
            if (EventOfferTypeDictionary.ContainsKey(eventCode))
            {
                offerType = EventOfferTypeDictionary[eventCode];
            }
            return offerType;
        }
        public static string GetEventCodeByEventName(this string eventName)
        {
            var eventCode = String.Empty;
            if (EventCodeAndNameDictionary.ContainsKey(eventName))
            {
                eventCode = EventCodeAndNameDictionary[eventName].EventCode;
            }
            return eventCode;
        }
    }

}
