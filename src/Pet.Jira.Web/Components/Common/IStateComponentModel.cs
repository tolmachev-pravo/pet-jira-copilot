namespace Pet.Jira.Web.Components
{
    public interface IStateComponentModel
    {
        ComponentModelState State { get; }
        void StateTo(ComponentModelState state);
    }
}
