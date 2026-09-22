export function validateWarehouseForm(formData) {
  const errors = [];

  if (!formData.name || !formData.name.trim()) {
    errors.push("Warehouse name is required.");
  }

  if (!formData.location || !formData.location.trim()) {
    errors.push("Warehouse location is required.");
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

export function buildWarehousePayload(formData, isEdit = false) {
  const payload = {
    WarehouseName: formData.name.trim(),
    LocationAddress: formData.location.trim()
  };

  // Only include isActive when updating/editing an existing warehouse
  if (isEdit) {
    payload.isActive = Boolean(formData.isActive);
  }

  return payload;
}