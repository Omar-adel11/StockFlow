using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class CustomerDtos
    {
        public record AddressSaveRequest(
            string Street,
            string City,
            string State,
            string ZipCode,
            string Country,
            bool IsDefault
        );  
        public record AddressResponse(
            int Id,
            string Street,
            string City,
            string State,
            string ZipCode,
            string Country,
            bool IsDefault
        );

        public record CustomerResponse(
            int Id,
            string Name,
            string Email,
            string Phone,
            IReadOnlyCollection<AddressResponse> Addresses
        );

        public record AddressCreateRequest(
            string Street,
            string City,
            string State,
            string ZipCode,
            string Country,
            bool IsDefault
        );

        public record CustomerCreateRequest(
            string Name,
            string Email,
            string Phone,
            List<AddressCreateRequest> Addresses
        );

        public record AddressUpdateRequest(
            int? Id, // Nullable: null/0 for new addresses, populated for existing ones
            string Street,
            string City,
            string State,
            string ZipCode,
            string Country,
            bool IsDefault
        );

        public record CustomerUpdateRequest(
            string Name,
            string Email,
            string Phone,
            List<AddressUpdateRequest> Addresses
        );
    }
}
