using MediatR;
using Pet.Jira.Domain.Models.Events;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Pet.Jira.Application.Events.Queries
{
    public class GetEvents
    {
        public class Query : IRequest<IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>>>
        {
            public DateOnly From { get; set; }
            public DateOnly To { get; set; }
        }

        public class QueryHandler
            : IRequestHandler<Query, IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>>>
        {
            private readonly IEventAggregator _aggregator;

            public QueryHandler(IEventAggregator aggregator)
            {
                _aggregator = aggregator;
            }

            public Task<IReadOnlyDictionary<DateOnly, IReadOnlyList<Event>>> Handle(
                Query request, CancellationToken cancellationToken)
            {
                return _aggregator.GetEventsAsync(request.From, request.To, cancellationToken);
            }
        }
    }
}
