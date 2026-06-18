using Pet.Jira.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Events
{
    public interface IEventDataSource
    {
        Task<IReadOnlyList<Event>> GetEventsAsync(
            DateOnly from,
            DateOnly to,
            CancellationToken cancellationToken);
    }
}
