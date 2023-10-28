using System;

namespace Pet.Jira.Domain.Entities
{
	public interface IEntity
	{
		Guid Id { get; set; }
	}
}
