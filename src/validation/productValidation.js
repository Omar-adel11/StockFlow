export function validateProductForm(formData) {
  const errors = [];
  const sku = formData.sku || formData.itemSKU;
  const price = formData.unitSellingPrice ?? formData.unitPrice;

  if (!sku || !sku.trim()) {
    errors.push("SKU is required.");
  }

  if (!formData.name || !formData.name.trim()) {
    errors.push("Product name is required.");
  }

  if (!formData.categoryId) {
    errors.push("Category selection is required.");
  }

  if (price === "" || price === null || price === undefined || isNaN(price)) {
    errors.push("Unit Price must be a valid number.");
  } else if (parseFloat(price) < 0) {
    errors.push("Unit Price cannot be negative.");
  }

  if (formData.reorderLevel !== "" && formData.reorderLevel !== null && formData.reorderLevel !== undefined) {
    if (isNaN(formData.reorderLevel) || parseInt(formData.reorderLevel, 10) < 0) {
      errors.push("Reorder level must be a non-negative integer.");
    }
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

export function buildProductPayload(formData) {
  const sku = (formData.sku || formData.itemSKU || '').trim();
  const price = formData.unitSellingPrice ?? formData.unitPrice;

  return {
    ItemSKU: sku,
    name: formData.name.trim(),
    categoryId: parseInt(formData.categoryId, 10),
    preferredSupplierId: formData.preferredSupplierId ? parseInt(formData.preferredSupplierId, 10) : null,
    UnitSellingPrice: parseFloat(price),
    reorderLevel: formData.reorderLevel ? parseInt(formData.reorderLevel, 10) : 0,
    isActive: Boolean(formData.isActive)
  };
}