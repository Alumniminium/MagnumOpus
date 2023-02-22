namespace MagnumOpus.ECS
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class ComponentAttribute : Attribute { }
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class SaveAttribute : Attribute { }
}