namespace MagnumOpus.ECS
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class ComponentAttribute : Attribute
    {
        public bool SaveEnabled { get; }
        public ComponentAttribute(bool saveEnabled = false) => SaveEnabled = saveEnabled;
    }
}