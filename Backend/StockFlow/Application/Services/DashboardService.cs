using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;
using Domain.Entities.Enum;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.Dashboarddtos.Dashboarddtos;

namespace Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IAppDbContext _dbContext;

        public DashboardService(IAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync()
        {
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var previousMonthStart = currentMonthStart.AddMonths(-1);

            // -------------------------------------------------------------
            // 1. Total Products Count & Growth Percentage
            // -------------------------------------------------------------
            var totalProducts = await _dbContext.Products.CountAsync();

            var currentMonthProductsCount = await _dbContext.Products
                .CountAsync(p => p.CreatedAt >= currentMonthStart);

            var previousMonthProductsCount = await _dbContext.Products
                .CountAsync(p => p.CreatedAt >= previousMonthStart && p.CreatedAt < currentMonthStart);

            var productsGrowthPercentage = previousMonthProductsCount > 0
                ? (double)((currentMonthProductsCount - previousMonthProductsCount) / (double)previousMonthProductsCount * 100)
                : (currentMonthProductsCount > 0 ? 100.0 : 0.0);

            // -------------------------------------------------------------
            // 2. Low Stock Count & Alerts
            // -------------------------------------------------------------
            var lowStockItems = await _dbContext.Products
                .Where(p => p.IsActive && p.InventoryItems.Sum(i => i.QuantityOnHand) <= p.ReorderLevel)
                .Select(p => new LowStockAlertDto
                {
                    ProductId = p.Id,
                    ProductName = p.Name,
                    QuantityOnHand = p.InventoryItems.Sum(i => i.QuantityOnHand),
                    ReorderLevel = p.ReorderLevel
                })
                .ToListAsync();

            // -------------------------------------------------------------
            // 3. Orders, Revenue & Growth Calculations (FILTERED BY COMPLETED)
            // -------------------------------------------------------------
            // Base query for valid/completed sales orders only
            var completedSalesOrders = _dbContext.SalesOrders
                .Where(so => so.Status == OrderStatus.Completed);

            var totalOrders = await completedSalesOrders.CountAsync();

            // Orders placed in current month vs. previous month
            var currentMonthOrdersCount = await completedSalesOrders
                .CountAsync(so => so.OrderDate >= currentMonthStart);

            var previousMonthOrdersCount = await completedSalesOrders
                .CountAsync(so => so.OrderDate >= previousMonthStart && so.OrderDate < currentMonthStart);

            var ordersGrowthPercentage = previousMonthOrdersCount > 0
                ? (double)((currentMonthOrdersCount - previousMonthOrdersCount) / (double)previousMonthOrdersCount * 100)
                : (currentMonthOrdersCount > 0 ? 100.0 : 0.0);

            // Revenue calculations (Excludes Cancelled and Pending)
            var currentMonthSales = await completedSalesOrders
                .Where(so => so.OrderDate >= currentMonthStart)
                .SumAsync(so => (decimal?)so.TotalAmount) ?? 0m;

            var previousMonthSales = await completedSalesOrders
                .Where(so => so.OrderDate >= previousMonthStart && so.OrderDate < currentMonthStart)
                .SumAsync(so => (decimal?)so.TotalAmount) ?? 0m;

            var totalRevenue = await completedSalesOrders
                .SumAsync(so => (decimal?)so.TotalAmount) ?? 0m;

            var totalCost = await _dbContext.PurchaseOrders
                .Where(po => po.Status == OrderStatus.Completed) // Apply same logic if POs have statuses
                .SumAsync(po => (decimal?)po.TotalAmount) ?? 0m;

            var grossProfit = totalRevenue - totalCost;

            var revenueGrowth = previousMonthSales > 0
                ? (double)((currentMonthSales - previousMonthSales) / previousMonthSales * 100)
                : (currentMonthSales > 0 ? 100.0 : 0.0);

            // -------------------------------------------------------------
            // 4. Monthly Sales (Last 6 Months - FILTERED BY COMPLETED)
            // -------------------------------------------------------------
            var sixMonthsAgo = currentMonthStart.AddMonths(-5);
            var rawMonthlyData = await completedSalesOrders
                .Where(so => so.OrderDate >= sixMonthsAgo)
                .GroupBy(so => new { so.OrderDate.Year, so.OrderDate.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Total = g.Sum(so => so.TotalAmount)
                })
                .ToListAsync();

            var maxMonthlyTotal = rawMonthlyData.Max(m => (decimal?)m.Total) ?? 1m;

            var monthlySales = Enumerable.Range(0, 6).Select(i =>
            {
                var monthDate = currentMonthStart.AddMonths(-5 + i);
                var match = rawMonthlyData.FirstOrDefault(m => m.Year == monthDate.Year && m.Month == monthDate.Month);
                var total = match?.Total ?? 0m;
                var heightPct = maxMonthlyTotal > 0 ? (int)Math.Round((total / maxMonthlyTotal) * 100) : 0;

                return new MonthlySalesDto
                {
                    Month = monthDate.ToString("MMM"),
                    TotalSales = total,
                    HeightPercentage = Math.Max(heightPct, 10)
                };
            }).ToList();

            // -------------------------------------------------------------
            // 5. Recent Activity
            // -------------------------------------------------------------
            var recentSalesOrders = await _dbContext.SalesOrders
                .OrderByDescending(so => so.OrderDate)
                .Take(3)
                .Select(so => new RecentActivityDto
                {
                    Title = $"Sales order {so.Status.ToString().ToLower()}",
                    Subtitle = $"SO-{so.Id}",
                    CreatedAt = so.OrderDate
                }).ToListAsync();

            var recentPurchaseOrders = await _dbContext.PurchaseOrders
                .OrderByDescending(po => po.OrderDate)
                .Take(3)
                .Select(po => new RecentActivityDto
                {
                    Title = "New purchase order received",
                    Subtitle = $"PO-{po.Id}",
                    CreatedAt = po.OrderDate
                }).ToListAsync();

            var recentActivities = recentSalesOrders.Concat(recentPurchaseOrders)
                .OrderByDescending(a => a.CreatedAt)
                .Take(5)
                .ToList();

            // -------------------------------------------------------------
            // 6. Category Distribution
            // -------------------------------------------------------------
            var categoryCounts = await _dbContext.Categories
                .Select(c => new
                {
                    CategoryName = c.Name,
                    ProductCount = c.Products.Count
                })
                .ToListAsync();

            var totalCategorizedProducts = categoryCounts.Sum(c => c.ProductCount);
            var categoryDistribution = categoryCounts.Select(c => new CategoryDistributionDto
            {
                CategoryName = c.CategoryName,
                Percentage = totalCategorizedProducts > 0
                    ? Math.Round(((double)c.ProductCount / totalCategorizedProducts) * 100, 1)
                    : 0
            }).ToList();

            // -------------------------------------------------------------
            // Return DTO
            // -------------------------------------------------------------
            return new DashboardSummaryDto
            {
                TotalProducts = totalProducts,
                ProductsGrowthPercentage = Math.Round(productsGrowthPercentage, 1),
                LowStockItemsCount = lowStockItems.Count,
                TotalOrders = totalOrders,
                OrdersGrowthPercentage = Math.Round(ordersGrowthPercentage, 1),
                TotalRevenue = totalRevenue,
                RevenueGrowthPercentage = Math.Round(revenueGrowth, 1),
                MonthlySales = monthlySales,
                LowStockAlerts = lowStockItems.Take(4).ToList(),
                RecentActivities = recentActivities,
                CategoryDistribution = categoryDistribution,
                TotalCost = totalCost
            };
        }
    }
}