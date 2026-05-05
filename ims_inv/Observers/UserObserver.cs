using System.Threading.Tasks;
using ims_inv.Events;
using ims_inv.Services;
using ims_inv.Models;

namespace ims_inv.Observers
{
    public interface IDomainEventHandler<in T> where T : IDomainEvent
    {
        Task Handle(T domainEvent);
    }

    public class UserObserver : IDomainEventHandler<UserRegisteredEvent>
    {
        private readonly IEmailService _emailService;

        public UserObserver(IEmailService emailService)
        {
            _emailService = emailService;
        }

        public async Task Handle(UserRegisteredEvent domainEvent)
        {
            var user = domainEvent.User;
            var model = new WelcomeEmailViewModel
            {
                Name = user.Name,
                LoginUrl = "" // In a real app, you would inject a URL generator or configuration
            };

            await _emailService.SendTemplateEmailAsync(
                user.Email,
                "Welcome to Our Application!",
                "Views/Emails/WelcomeEmail.cshtml",
                model
            );
        }
    }
}
