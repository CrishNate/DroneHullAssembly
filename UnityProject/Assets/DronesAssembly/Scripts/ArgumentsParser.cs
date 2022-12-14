using System;
using System.Collections.Generic;
using UnityEngine;

namespace DroneHullAssembly.Tools
{
    public class ArgumentParser : IEquatable<string>
    {
        private string m_Arg;
        private string m_Description;
        private Action<string> m_Action;

        public ArgumentParser(string arg, string desc, Action<string> action)
        {
            m_Arg = arg;
            m_Description = desc;
            m_Action = action;
        }

        public bool Equals(string other)
        {
            return m_Arg == other;
        }

        internal void Execute(string value)
        {
            m_Action(value);
        }
    }
    
    public class ArgumentsParser
    {
        private List<ArgumentParser> _argumentParsers = new List<ArgumentParser>();

        public void Add(string arg, string desc, Action<string> action)
        {
            _argumentParsers.Add(new ArgumentParser(arg, desc, action));
        }

        public void Parse(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var argumentParser = _argumentParsers.Find(x => x.Equals(args[i]));
                if (argumentParser == null) 
                    continue;

                argumentParser.Execute(i + 1 < args.Length ? args[i + 1] : string.Empty);
            }
        }
    }
}