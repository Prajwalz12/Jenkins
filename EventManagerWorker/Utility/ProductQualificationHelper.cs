using System;
using System.Collections.Generic;
using System.Linq;

namespace EventManagerWorker.Utility
{
    public static class ProductQualificationHelper
    {
        public static bool Qualify(List<Domain.Models.TransactionModel.Product> transactionProducts, List<Domain.Models.CampaignModel.Product> campaignProducts)
        {
            if (campaignProducts == null || !campaignProducts.Any())
            {
                return true; // No campaign products, automatically qualifies
            }

            if (transactionProducts == null || !transactionProducts.Any())
            {
                return false; // No transaction products, cannot qualify
            }

            // Map Campaign products to a more comparable format
            var mappedCampaignProducts = campaignProducts
                .Select(cp => new
                {
                    ProductIds = cp.ProductId ?? new List<string>(),
                    Journeys = cp.Journey?.Any() == true ? cp.Journey : null,
                    PurchaseType = cp.PurchaseType
                })
                .ToList();

            // Map Transaction products to a comparable format
            var mappedTransactionProducts = transactionProducts
                .GroupBy(tp => new
                {
                    Id = tp.Id,
                    Journey = string.IsNullOrEmpty(tp.Journey) ? "" : tp.Journey,
                    PurchaseType = tp.PurchaseType
                })
                .Select(g => new
                {
                    Id = g.Key.Id,
                    Journey = g.Key.Journey,
                    PurchaseType = g.Key.PurchaseType,
                    Count = g.Count()
                })
                .ToList();

            // For each campaign product, check if there is a matching transaction product
            foreach (var campaignProduct in mappedCampaignProducts)
            {
                var matchingTransactions = mappedTransactionProducts
                    .Where(tp => campaignProduct.ProductIds.Contains(tp.Id)
                        && (campaignProduct.Journeys == null || campaignProduct.Journeys.Contains(tp.Journey))
                        && campaignProduct.PurchaseType == tp.PurchaseType)
                    .ToList();

                // If no matching transactions for a campaign product, return false
                if (!matchingTransactions.Any())
                {
                    return false;
                }
            }

            return true; // All campaign products have matching transactions
        }
    }
}
