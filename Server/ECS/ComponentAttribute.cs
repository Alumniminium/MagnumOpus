namespace MagnumOpus.ECS
{
    [AttributeUsage(AttributeTargets.Struct)]
    public class ComponentAttribute(bool saveEnabled = false) : Attribute
    {
        public bool SaveEnabled { get; } = saveEnabled;
    }
}