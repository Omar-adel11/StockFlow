using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Entities.Enum;
using Domain.Exceptions.NotFound;
using Microsoft.EntityFrameworkCore;
using static Application.DTOs.PurchaseOrderDtos;

namespace Application.Services
{
    public class PurchaseOrderService(IAppDbContext _context) : IPurchaseOrderService
    {
        private readonly IAppDbContext _context = _context;

        private static readonly Expression<Func<PurchaseOrder, PurchaseResponse>> ToPurchaseResponse =
            po => new PurchaseResponse(
                po.Id,
                po.PONumber,
                po.SupplierId,
                po.Supplier != null ? po.Supplier.Name : string.Empty,
                po.WarehouseId,
                po.Warehouse != null ? po.Warehouse.Name : string.Empty,
                po.Status.ToString(),
                po.OrderDate,
                po.TotalAmount,
                po.Items.Select(poi => new PurchaseItemResponse(
                    poi.ProductId,
                    poi.Product != null ? poi.Product.Name : string.Empty,
                    poi.Product != null ? poi.Product.SKU : string.Empty,
                    poi.Quantity,
                    poi.UnitCost,
                    poi.Quantity * poi.UnitCost
                )).ToList()
            );

        private DbSet<PurchaseOrder> PurchaseOrders => _context.PurchaseOrders;
        private DbSet<InventoryItem> InventoryItems => _context.InventoryItems;

        public async Task<IReadOnlyCollection<PurchaseResponse>> GetAllOrdersAsync(int count = 20)
        {
            return await PurchaseOrders
                .AsNoTracking()
                .OrderByDescending(po => po.OrderDate)
                .Take(count)
                .Select(ToPurchaseResponse)
                .ToListAsync();
        }

        public async Task<PurchaseResponse?> GetOrderByIdAsync(int id)
        {
            var result = await PurchaseOrders
                .AsNoTracking()
                .Where(po => po.Id == id)
                .Select(ToPurchaseResponse)
                .FirstOrDefaultAsync();
            if(result == null)
            {
                throw new OrderNotFoundException();
            }
            return result;
        }

        public async Task<PurchaseResponse> CreatePurchaseOrderAsync(PurchaseCreateRequest createRequest, int currentUserId)
        {
            string poNumber = $"PO-{DateTime.UtcNow:yyyyMM}-{Guid.NewGuid().ToString()[..6].ToUpper()}";

            var orderItems = createRequest.Items.Select(i => new PurchaseOrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.QuantityOrdered,
                UnitCost = i.AgreedUnitPrice
            }).ToList();

            decimal totalAmount = orderItems.Sum(i => i.Quantity * i.UnitCost);

            var purchaseOrder = new PurchaseOrder
            {
                PONumber = poNumber,
                SupplierId = createRequest.SupplierId,
                WarehouseId = createRequest.WarehouseId,
                CreatedByUserId = currentUserId,
                Status = OrderStatus.Pending,
                OrderDate = DateTime.UtcNow,
                TotalAmount = totalAmount,
                Items = orderItems
            };

            PurchaseOrders.Add(purchaseOrder);
            await _context.SaveChangesAsync(CancellationToken.None);

            var createdOrder = await GetOrderByIdAsync(purchaseOrder.Id);
            return createdOrder ?? throw new InvalidOperationException("Failed to retrieve created purchase order.");
        }

        public async Task<bool> ReceivePurchaseOrderAsync(int id)
        {
            var order = await PurchaseOrders
                .Include(po => po.Items)
                .FirstOrDefaultAsync(po => po.Id == id);

            if (order == null || order.Status != OrderStatus.Pending)
            {
                throw new OrderNotFoundException();
            }

            // Fetch target warehouse stock entries for the items in this PO
            var productIds = order.Items.Select(i => i.ProductId).ToList();
            var existingInventory = await InventoryItems
                .Where(i => i.WarehouseId == order.WarehouseId && productIds.Contains(i.ProductId))
                .ToDictionaryAsync(i => i.ProductId);

            foreach (var item in order.Items)
            {
                if (existingInventory.TryGetValue(item.ProductId, out var stockItem))
                {
                    stockItem.QuantityOnHand += item.Quantity;
                }
                else
                {
                    var newStockItem = new InventoryItem
                    {
                        WarehouseId = order.WarehouseId,
                        ProductId = item.ProductId,
                        QuantityOnHand = item.Quantity
                    };
                    InventoryItems.Add(newStockItem);
                }
            }

            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync(CancellationToken.None);

            return true;
        }

        public async Task<bool> CancelPurchaseOrderAsync(int id)
        {
            var order = await PurchaseOrders.FirstOrDefaultAsync(po => po.Id == id);

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