export function validateSupplierForm(formData) {
  const errors = [];

  if (!formData.name || !formData.name.trim()) {
    errors.push("Supplier name is required.");
  }

  if (formData.contactEmail && formData.contactEmail.trim()) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.contactEmail.trim())) {
      errors.push("Please enter a valid contact email address.");
    }
  }

  return {
    isValid: errors.length === 0,
    errors
  };
}

export function buildSupplierPayload(formData) {
  return {
    name: formData.name.trim(),
    contactEmail: formData.contactEmail ? formData.contactEmail.trim() : null,
    contactPhone: formData.contactPhone ? formData.contactPhone.trim() : null,
    address: formData.address ? formData.address.trim() : null,
    isActive: Boolean(formData.isActive)
  };
}