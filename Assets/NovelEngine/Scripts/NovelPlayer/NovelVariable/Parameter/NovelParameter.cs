using System;
namespace novel
{
    [Serializable]
    public abstract class NovelParameter
    {
    }
    public abstract class CustomParameter : NovelParameter { }

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class NovelParameterAliasAttribute : Attribute
    {
        public string Alias { get; private set; }
        public NovelParameterAliasAttribute(string alias)
        {
            this.Alias = alias;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class OrderAttribute : Attribute
    {
        public int Index { get; }
        public OrderAttribute(int index) => Index = index;
    }

}