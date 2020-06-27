using System;

namespace MutiFramework
{
    public abstract class Framework : IDisposable
    {

        public Environment env { get; set; }
        public abstract string name { get; }

        public abstract int priority { get; }


        public abstract void Startup();
        public abstract void Dispose();
    }


}

