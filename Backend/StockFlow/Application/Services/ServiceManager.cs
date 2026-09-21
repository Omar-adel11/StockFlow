using System;
using Application.Interfaces;
using Application.Interfaces.AuthInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Services
{
    public class ServiceManager : IServiceManager
    {
        private readonly Lazy<IAuthenticationService> _authService;
        private readonly Lazy<ICategoryService> _categoryService;
        private readonly Lazy<IPurchaseOrderService> _purchaseOrderService;
        private readonly Lazy<ISalesOrderService> _salesOrderService;
        private readonly Lazy<IProductService> _productService;
        private readonly Lazy<IWarehouseService> _warehouseService;
        private readonly Lazy<ISupplierService> _supplierService;
        private readonly Lazy<ICustomerService> _customerService;
        private readonly Lazy<IStockMovementService> _stockMovementService;
        private readonly Lazy<IInventoryService> _inventoryService;

        public ServiceManager(IServiceProvider serviceProvider)
        {
            _authService = new Lazy<IAuthenticationService>(() => serviceProvider.GetRequiredService<IAuthenticationService>());
            _categoryService = new Lazy<ICategoryService>(() => serviceProvider.GetRequiredService<ICategoryService>());
            _purchaseOrderService = new Lazy<IPurchaseOrderService>(() => serviceProvider.GetRequiredService<IPurchaseOrderService>());
            _salesOrderService = new Lazy<ISalesOrderService>(() => serviceProvider.GetRequiredService<ISalesOrderService>());
            _productService = new Lazy<IProductService>(() => serviceProvider.GetRequiredService<IProductService>());
            _warehouseService = new Lazy<IWarehouseService>(() => serviceProvider.GetRequiredService<IWarehouseService>());
            _supplierService = new Lazy<ISupplierService>(() => serviceProvider.GetRequiredService<ISupplierService>());
            _customerService = new Lazy<ICustomerService>(() => serviceProvider.GetRequiredService<ICustomerService>());
            _stockMovementService = new Lazy<IStockMovementService>(() => serviceProvider.GetRequiredService<IStockMovementService>());
            _inventoryService = new Lazy<IInventoryService>(() => serviceProvider.GetRequiredService<IInventoryService>());
        }

        public IAuthenticationService AuthService => _authService.Value;
        public ICategoryService CategoryService => _categoryService.Value;
        public IPurchaseOrderService PurchaseOrderService => _purchaseOrderService.Value;
        public ISalesOrderService SalesOrderService => _salesOrderService.Value;
        public IProductService ProductService => _productService.Value;
        public IWarehouseService WarehouseService => _warehouseService.Value;
        public ISupplierService SupplierService => _supplierService.Value;
        public ICustomerService CustomerService => _customerService.Value;
        public IStockMovementService StockMovementService => _stockMovementService.Value;
        public IInventoryService InventoryService => _inventoryService.Value;

    }
}