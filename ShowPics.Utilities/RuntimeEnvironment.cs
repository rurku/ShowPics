using System;
using System.Collections.Generic;
using System.Text;

namespace ShowPics.Utilities
{
    public class RuntimeEnvironment : IRuntimeEnvironment
    {
        public RuntimeEnvironment(string environment)
        {
            Name = environment;
        }

        public string Name { get; }
    }

    public interface IRuntimeEnvironment
    {
        string Name { get; }
    }
}
