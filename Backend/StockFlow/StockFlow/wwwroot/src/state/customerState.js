let customers = [];

export function setCustomers(newCustomers) {
    customers = newCustomers;
}

export function getCustomers() {
    return customers;
}

export function getCustomerById(id) {
    return customers.find(cust => cust.id === Number(id));
}

export function addCustomerToState(customer) {
    customers.unshift(customer);
}

export function updateCustomerInState(updatedCustomer) {
    const index = customers.findIndex(cust => cust.id === updatedCustomer.id);
    if (index !== -1) {
        customers[index] = updatedCustomer;
    }
}

export function removeCustomerFromState(id) {
    customers = customers.filter(cust => cust.id !== Number(id));
}