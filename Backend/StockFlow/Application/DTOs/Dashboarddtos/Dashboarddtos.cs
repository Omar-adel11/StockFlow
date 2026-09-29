using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs.Dashboarddtos
{
    public class Dashboarddtos
    {
        public class DashboardSummaryDto
        {
            public int TotalProducts { get; set; }
            public double ProductsGrowthPercentage { get; set; }

            public int LowStockItemsCount { get; set; }

            public int TotalOrders { get; set; }
            public double OrdersGrowthPercentage { get; set; }

            public decimal TotalRevenue { get; set; }        // Based on UnitSellingPrice
            public decimal TotalCost { get; set; }           // Based on UnitPurchasePrice
            public decimal GrossProfit => TotalRevenue - TotalCost;
            public double RevenueGrowthPercentage { get; set; }

            public List<MonthlySalesDto> MonthlySales { get; set; } = new();
            public List<LowStockAlertDto> LowStockAlerts { get; set; } = new();
            public List<RecentActivityDto> RecentActivities { get; set; } = new();
            public List<CategoryDistributionDto> CategoryDistribution { get; set; } = new();
        }

        public class MonthlySalesDto
        {
            public string Month { get; set; } = string.Empty;
            public decimal TotalSales { get; set; }
            public int HeightPercentage { get; set; } // Helps frontend map CSS bar heights dynamically
        }

        public class LowStockAlertDto
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public int QuantityOnHand { get; set; }
            public int ReorderLevel { get; set; }
        }

        public class RecentActivityDto
        {
            public string Title { get; set; } = string.Empty;
            public string Subtitle { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        public class CategoryDistributionDto
        {
            public string CategoryName { get; set; } = string.Empty;
            public double Percentage { get; set; }
        }
    }
}

