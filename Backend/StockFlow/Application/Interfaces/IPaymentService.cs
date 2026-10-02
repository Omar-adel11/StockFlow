using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Application.DTOs.Payment.Paymentdtos;

namespace Application.Interfaces
{
    public interface IPaymentService
    {
        Task<CheckoutSessionResponse> CreateCheckoutSessionAsync(int businessId, CreateCheckoutSessionRequest request, CancellationToken ct = default);
        Task ProcessPaymentCallbackAsync(ProcessPaymentCallbackRequest request, CancellationToken ct = default);
    }
}
