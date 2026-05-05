using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ims_inv.Events;
using Microsoft.Extensions.DependencyInjection;
using System;
using ims_inv.Observers;

namespace ims_inv.Interceptors
{
    public class DomainEventsInterceptor : SaveChangesInterceptor
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventsInterceptor(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override async ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context == null) return result;

            var entitiesWithEvents = eventData.Context.ChangeTracker.Entries<IHasDomainEvents>()
                .Select(e => e.Entity)
                .Where(e => e.DomainEvents.Any())
                .ToList();

            foreach (var entity in entitiesWithEvents)
            {
                var events = entity.DomainEvents.ToList();
                entity.ClearDomainEvents();

                foreach (var domainEvent in events)
                {
                    await DispatchEvent(domainEvent);
                }
            }

            return await base.SavedChangesAsync(eventData, result, cancellationToken);
        }

        private async Task DispatchEvent(IDomainEvent domainEvent)
        {
            // Simple dispatcher logic: find all handlers for the event type
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handlers = _serviceProvider.GetServices(handlerType);

            foreach (var handler in handlers)
            {
                var method = handlerType.GetMethod("Handle");
                if (method != null)
                {
                    await (Task)method.Invoke(handler, new object[] { domainEvent });
                }
            }
        }
    }
}
