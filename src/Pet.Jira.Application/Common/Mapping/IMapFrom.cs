using AutoMapper;

namespace Pet.Jira.Application.Common.Mapping
{
	public interface IMapFrom<T>
	{
		void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
	}
}
