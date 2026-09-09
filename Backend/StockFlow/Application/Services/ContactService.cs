using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class ContactService : IContactService
    {
        
        private readonly IContactRepository _contactRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactService> _logger;

        public ContactService(
            IContactRepository contactRepository,
            IEmailService emailService,
            ILogger<ContactService> logger)
        {
            _contactRepository = contactRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ContactResponseDto> SubmitInquiryAsync(ContactRequestDto request)
        {
            var entity = new ContactMessage
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim(),
                Subject = request.Subject.Trim(),
                Message = request.Message.Trim(),
                SubmittedAtUtc = DateTime.UtcNow
            };

            // 1. Persist first. Storing the inquiry is the actual requirement -
            // this must succeed for the response to say "success".
            await _contactRepository.AddAsync(entity);

            // 2. Email is a bonus notification, not the requirement itself.
            // If Gmail/SMTP has a hiccup, the user's message is still safely
            // stored, so we don't want that to turn into a failed response -
            // we just log it and move on.
            try
            {
                await _emailService.SendContactConfirmationAsync(
                    entity.Email, entity.Name, entity.Subject, entity.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Contact message {Id} was saved but confirmation email failed to send.", entity.Id);
            }

            return new ContactResponseDto(true, "Your message has been sent successfully.");
        }
    }
    }
