using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.SalesOrderDtos;

namespace Application.Services
{
    public class SaleOrderService(IAppDbContext _context) : ISalesOrderService
    {
        private readonly IAppDbContext _context = _context;

        private DbSet<SalesOrder> SalesOrders => _context.SalesOrders;
        private DbSet<InventoryItem> InventoryItems => _context.InventoryItems;

        private static readonly Expression<Func<SalesOrder, SalesResponse>> ToSalesResponse =
            so => new SalesResponse(
                so.Id,
                so.InvoiceNumber,
                so.CustomerId,
                so.Customer != null ? so.Customer.Name : string.Empty,
                so.WarehouseId,
                so.Warehouse != null ? so.Warehouse.Name : string.Empty,
                so.Status.ToString(),
                so.OrderDate,
                so.TotalAmount,
                so.Items.Select(soi => new SalesItemResponse(
                    soi.ProductId,
                    soi.Product != null ? soi.Product.Name : string.Empty,
                    soi.Product != null ? soi.Product.SKU : string.Empty,
                    soi.Quantity,
                    soi.UnitPrice,
                    soi.Quantity * soi.UnitPrice
                )).ToList()
            );

        public async Task<IReadOnlyCollection<SalesResponse>> GetAllOrdersAsync()
        {
            return await SalesOrders
                .AsNoTracking()
                .OrderByDescending(so => so.OrderDate)
                .Select(ToSalesResponse)
                .ToListAsync();
        }

        public async Task<SalesResponse?> GetOrderByIdAsync(int id)
        {
            var result = await SalesOrders
                .AsNoTracking()
                .Where(so => so.Id == id)
                .Select(ToSalesResponse)
                .FirstOrDefaultAsync();
            if(result  == null)
            {
                throw new OrderNotFoundException();
            }
            return result;
        }

        public async Task<SalesResponse> CreateSalesOrderAsync(SalesCreateRequest createRequest, int currentUserId)
        {
            string invoiceNumber = $"INV-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var orderItems = createRequest.Items.Select(i => new SalesOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.QuantitySold,
                UnitPrice = i.BilledUnitPrice
            }).ToList();

            decimal totalAmount = orderItems.Sum(i => i.Quantity * i.UnitPrice);

            var salesOrder = new SalesOrder
            {
                InvoiceNumber = invoiceNumber,
                CustomerId = createRequest.CustomerId,
                WarehouseId = createRequest.WarehouseId,
                CreatedByUserId = currentUserId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Items = orderItems
            };

            SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync(CancellationToken.None);

            var createdOrder = await GetOrderByIdAsync(salesOrder.Id);
            return createdOrder ?? throw new InvalidOperationException("Failed to retrieve created sales order.");
        }

        public async Task<bool> FulfillSalesOrderAsync(int id)
        {
            var order = await SalesOrders
                .Include(so => so.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                throw new OrderNotFoundException();
            }

            var productIds = order.Items.Select(i => i.ProductId).ToList();

            var existingInventory = await InventoryItems
                .Where(i => i.WarehouseId == order.WarehouseId && productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId);

            // 1. Check if sufficient stock is available for ALL items before deducting
            foreach (var item in order.Items)
            {
                if (!existingInventory.TryGetValue(item.ProductId, out var stockItem) || stockItem.QuantityOnHand < item.Quantity)
                {
                    throw new InvalidOperationException($"sufficient stock for item {item.Product.Name} with id{item.ProductId}");
                }
            }

            // 2. Deduct inventory
            foreach (var item in order.Items)
            {
                existingInventory[item.ProductId].QuantityOnHand -= item.Quantity;
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync(CancellationToken.None);

            return true;
        }

        public async Task<bool> CancelSalesOrderAsync(int id)
        {
            var order = await SalesOrders.FirstOrDefaultAsync(so => so.Id == id);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                throw new OrderNotFoundException();
            }

            order.Status = OrderStatus.Cancelled;
            await _context.SaveChangesAsync(CancellationToken.None);

            return true;
        }
    }
}
