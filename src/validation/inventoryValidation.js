/**
 * Validates stock adjustment form inputs before posting to backend.
 */
export function validateStockAdjustment(formData) {
  const errors = [];

  if (!formData.productId) {
    errors.push('Please select a product.');
  }

  if (!formData.warehouseId) {
    errors.push('Please select a warehouse.');
  }

  const rawQty = formData.quantityChanged ?? formData.quantityChange;
  if (rawQty === '' || rawQty === null || rawQty === undefined || isNaN(rawQty)) {
    errors.push('Quantity change must be a valid integer.');
  } else if (parseInt(rawQty, 10) === 0) {
    errors.push('Quantity change cannot be 0.');
  }

  if (formData.reason === '' || formData.reason === null || formData.reason === undefined || isNaN(formData.reason)) {
    errors.push('Please select a valid movement reason.');
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

/**
 * Constructs the payload JSON object expected by the C# backend API endpoint.
 */
export function buildAdjustmentPayload(formData) {
  const rawQty = formData.quantityChanged ?? formData.quantityChange;

  return {
    productId: parseInt(formData.productId, 10),
    warehouseId: parseInt(formData.warehouseId, 10),
    quantityChanged: parseInt(rawQty, 10),
    reason: parseInt(formData.reason, 10),
    referenceId: formData.referenceId ? parseInt(formData.referenceId, 10) : 0
  };
}