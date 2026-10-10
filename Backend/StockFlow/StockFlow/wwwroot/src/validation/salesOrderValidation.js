/**
 * Validates the sales order form and line items.
 *
 * @param {Object} formData
 * @returns {{ isValid: boolean, errors: string[] }}
 */
export function validateSalesOrder(formData) {
  const errors = [];

  if (!formData.customerId) {
    errors.push('Customer selection is required.');
  }

  if (!formData.warehouseId) {
    errors.push('Fulfilling Warehouse selection is required.');
  }

  if (!Array.isArray(formData.items) || formData.items.length === 0) {
    errors.push('At least one line item must be added to the sales order.');
  } else {
    formData.items.forEach((item, index) => {
      const itemNum = index + 1;
      if (!item.productId) {
        errors.push(`Item #${itemNum}: Please select a product.`);
      }

      const qty = parseInt(item.quantitySold, 10);
      if (isNaN(qty) || qty < 1) {
        errors.push(`Item #${itemNum}: Quantity sold must be at least 1.`);
      }

      const price = parseFloat(item.billedUnitPrice);
      if (isNaN(price) || price < 0.01) {
        errors.push(`Item #${itemNum}: Billed unit price must be at least 0.01.`);
      }
    });
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Maps state form data to the backend SalesCreateRequest DTO payload.
 *
 * @param {Object} formData
 * @returns {Object} SalesCreateRequest payload
 */
export function buildSalesCreatePayload(formData) {
  return {
    customerId: parseInt(formData.customerId, 10),
    warehouseId: parseInt(formData.warehouseId, 10),
    items: formData.items.map(item => ({
      productId: parseInt(item.productId, 10),
      quantitySold: parseInt(item.quantitySold, 10),
      billedUnitPrice: parseFloat(item.billedUnitPrice)
    }))
  };
}