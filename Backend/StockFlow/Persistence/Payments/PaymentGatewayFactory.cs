using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Interfaces;

namespace Persistence.Payments
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentGatewayFactory(IEnumerable<IPaymentGateway> gateways)
        {
            _gateways = gateways;
        }

        public IPaymentGateway GetProvider(string providerName)
        {
            var gateway = _gateways.FirstOrDefault(g =>
                g.ProviderName.Equals(providerName, StringComparison.OrdinalIgnoreCase));

            return gateway ?? throw new InvalidOperationException($"Payment gateway '{providerName}' is not supported.");
        }
    }
}
