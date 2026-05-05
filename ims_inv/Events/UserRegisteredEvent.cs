using ims_inv.Models;

namespace ims_inv.Events
{
    public class UserRegisteredEvent : IDomainEvent
    {
        public User User { get; }

        public UserRegisteredEvent(User user)
        {
            User = user;
        }
    }
}
