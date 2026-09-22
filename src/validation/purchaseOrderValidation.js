/**
 * Validates the purchase order creation form and its line items.
 *
 * @param {Object} formData
 * @returns {{ isValid: boolean, errors: string[] }}
 */
export function validatePurchaseOrder(formData) {
  const errors = [];

  if (!formData.supplierId) {
    errors.push('Supplier selection is required.');
  }

  if (!formData.warehouseId) {
    errors.push('Destination Warehouse selection is required.');
  }

  if (!Array.isArray(formData.items) || formData.items.length === 0) {
    errors.push('At least one line item must be added to the purchase order.');
  } else {
    formData.items.forEach((item, index) => {
      const itemNum = index + 1;
      if (!item.productId) {
        errors.push(`Item #${itemNum}: Please select a product.`);
      }

      const qty = parseInt(item.quantityOrdered, 10);
      if (isNaN(qty) || qty < 1) {
        errors.push(`Item #${itemNum}: Quantity ordered must be at least 1.`);
      }

      const price = parseFloat(item.agreedUnitPrice);
      if (isNaN(price) || price < 0.01) {
        errors.push(`Item #${itemNum}: Unit cost must be at least 0.01.`);
      }
    });
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Maps input values to the C# PurchaseCreateRequest DTO payload.
 *
 * @param {Object} formData
 * @returns {Object} PurchaseCreateRequest payload
 */
export function buildPurchaseCreatePayload(formData) {
  return {
    supplierId: parseInt(formData.supplierId, 10),
    warehouseId: parseInt(formData.warehouseId, 10),
    items: formData.items.map(item => ({
      productId: parseInt(item.productId, 10),
      quantityOrdered: parseInt(item.quantityOrdered, 10),
      agreedUnitPrice: parseFloat(item.agreedUnitPrice)
    }))
  };
}