using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces.AuthInterfaces;

namespace Application.Interfaces
{
    public interface IServiceManager
    {
        IAuthenticationService AuthService { get; }
        ICategoryService CategoryService { get; }
        ISupplierService SupplierService { get; }
        IWarehouseService WarehouseService { get; }
        IProductService ProductService { get; }
        ICustomerService CustomerService { get; }
        IInventoryService InventoryService { get; }
        IPurchaseOrderService PurchaseOrderService { get; }
        ISalesOrderService SalesOrderService { get; }
        IStockMovementService StockMovementService { get; }

    }
}
