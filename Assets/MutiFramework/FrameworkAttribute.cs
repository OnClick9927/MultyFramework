using System;

namespace MutiFramework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class FrameworkAttribute : Attribute
    {
        public Environment env { get; private set; }

        public FrameworkAttribute(Environment env)
        {
            this.env = env;
        }
    }


}

