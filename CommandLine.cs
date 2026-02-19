using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NinjaTraderLauncher
{
    public class CommandLineParseError
    {
        public CommandLineParseError(string message, CommandLineArgument? argument = null)
        {
            Message = message;
            Argument = argument;
        }

        public string Message { internal set; get; }

        public bool HasSourceArgument { get { return Argument != null; } }

        public CommandLineArgument? Argument { internal set; get; }
    }

    public class CommandLineArgument
    {
        public CommandLineArgument(string name, string description, bool valueRequired=false)
        {
            Name = name;
            Description = description;

            Character = '\0';
            UsesCharacter = false;

            Value = string.Empty;
            UsesValue = valueRequired;
            ValueRequired = valueRequired;

            ArgumentFound = false;
        }
        public CommandLineArgument(CommandLineArgument? argument)
        {
            if (argument == null) throw new ArgumentNullException(nameof(argument), "");

            argument.CopyTo(this);
        }

        public void CopyTo(CommandLineArgument other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other), "");

            other.Character = Character;
            other.UsesCharacter = UsesCharacter;

            other.Name = Name;
            other.Description = Description;

            other.Value = Value;
            other.UsesValue = UsesValue;
            other.ValueRequired = ValueRequired;

            other.ArgumentFound = ArgumentFound;
        }

        public string Name { set; get; } = string.Empty;
        public string Description { set; get; } = string.Empty;

        public char Character { set; get; } = '\0';
        public bool UsesCharacter { set; get; } = false;

        public string Value { internal set; get; } = string.Empty;
        public bool UsesValue { set; get; } = false;
        public bool ValueRequired { set; get; } = false;

        public bool ArgumentFound { internal set; get; } = false;

        public bool ArgMatch(string arg, List<string> prefixes)
        {
            if (arg == null) throw new ArgumentNullException(nameof(arg),"");
            if (prefixes == null) throw new ArgumentNullException(nameof(prefixes),"");
            if (prefixes.Count == 0) throw new ArgumentOutOfRangeException(nameof(prefixes),"");

            foreach (string prefix in prefixes)
            {
                if (arg == prefix + Name) return true;
                if(UsesCharacter && arg == prefix + Character) return true;
            }

            return false;
        }
    }

    public class CommandLine
    {
        Dictionary<string,CommandLineArgument> _arguments;
        List<CommandLineParseError> _errors;
        List<string> _bareValues;
        List<string> _prefixes;

        public Dictionary<string,CommandLineArgument> Arguments { get { return _arguments; } }
        public List<CommandLineParseError> Errors { get { return _errors; } }
        public List<string> BareValues { get { return _bareValues; } }
        public List<string> Prefixes { get { return _prefixes; } }

        public CommandLine()
        {
            _prefixes = new List<string>();
            _arguments = new Dictionary<string,CommandLineArgument>();
            _errors = new List<CommandLineParseError>();
            _bareValues = new List<string>();
        }

        public bool HasError { get { return _errors.Count > 0; } }

        public string AllErrors()
        {
            string errors = "";
            bool firstError = true;
            foreach (CommandLineParseError error in _errors)
            {
                if (firstError)
                {
                    errors += error.Message;
                    firstError = false;
                }
                else
                {
                    errors += Environment.NewLine + error.Message;
                }
            }
            
            return errors;
        }        

        public void Reset() { _errors.Clear(); _bareValues.Clear(); }

        public void Clear() { _prefixes.Clear(); _arguments.Clear(); Reset(); }
        
        public void AddPrefix(string prefix)
        {
            _prefixes.Add(prefix);
        }
        public void AddPrefixes(List<string> prefixes)
        {
            foreach (string prefix in prefixes) { AddPrefix(prefix); }
        }

        public void AddPrefixes(string[] prefixes)
        {
            foreach (string prefix in prefixes) { AddPrefix(prefix); }
        }

        public void AddArgument(CommandLineArgument argument)
        {
            if (_arguments.ContainsKey(argument.Name))
                _arguments[argument.Name] = new CommandLineArgument(argument);
            else
                _arguments.Add(argument.Name, new CommandLineArgument(argument));
        }

        public void AddArguments(List<CommandLineArgument> arguments)
        {
            foreach (CommandLineArgument argument in arguments) { AddArgument(argument); }
        }

        public void AddArguments(CommandLineArgument[] arguments)
        {
            foreach (CommandLineArgument argument in arguments) { AddArgument(argument); }
        }


        public bool Parse(string[] args)
        {
            _errors.Clear();
            if (_arguments.Count == 0) { return true; }
            if(_prefixes.Count == 0)
            {
                _errors.Add(new CommandLineParseError("CommandLine has no Prefixes set."));
                return false;
            }

            for (int i = 0; i < args.Length; i++)
            {
                ParseArg(args, ref i);
            }

            return !HasError;
        }

        private bool StartsWithPrefix(string arg)
        {
            foreach (string prefix in _prefixes)
            {
                if (arg.StartsWith(prefix)) return true;
            }
            return false;
        }

        private bool ParseArg(string[] args, ref int i)
        {
            if (i >= args.Length || i < 0) throw new ArgumentOutOfRangeException(nameof(i), "");

            if (StartsWithPrefix(args[i]))
            {
                string[] argValues = args[i].Split('=');
                string argName = argValues[0];
                string argValue = string.Empty;

                bool argumentFound = false;
                foreach (string argumentName in _arguments.Keys)
                {
                    CommandLineArgument argument = _arguments[argumentName];

                    if (argument.ArgMatch(argName, _prefixes))
                    {
                        argumentFound = true;
                        argument.ArgumentFound = true;

                        if (argValues.Length == 2)
                        {
                            if (!argument.UsesValue)
                            {
                                _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" has a value, but does not use a value.", argument));
                                return false;
                            }
                            argValue = argValues[1];
                        }
                        else
                        {
                            if (!argument.UsesValue) return true;

                            ++i;
                            if (i >= args.Length)
                            {
                                _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                                return false;
                            }

                            if (args[i] == "=") { ++i; }

                            if (i >= args.Length)
                            {
                                _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                                return false;
                            }

                            if (StartsWithPrefix(args[i]))
                            {
                                if (!argument.ValueRequired)
                                {
                                    --i;
                                    return true;
                                }

                                _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                                return false;
                            }

                            argValue = args[i];
                        }

                        if (string.IsNullOrEmpty(argValue))
                        {
                            _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                            return false;
                        }

                        argValue = argValue.TrimStart();
                        argValue = argValue.TrimEnd();

                        if (string.IsNullOrEmpty(argValue))
                        {
                            _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                            return false;
                        }

                        if (argValue.StartsWith('"') && argValue.EndsWith('"'))
                        {
                            argValue = argValue.TrimStart('"');
                            argValue = argValue.TrimEnd('"');
                        }

                        if (string.IsNullOrEmpty(argValue))
                        {
                            _errors.Add(new CommandLineParseError($"CommandLine argument \"{argument.Name}\" requires a value.", argument));
                            return false;
                        }

                        argument.Value = argValue;
                    }
                }
                
                if(!argumentFound)
                {
                    _errors.Add(new CommandLineParseError($"CommandLine argument \"{argName}\" unknown."));
                    return false;
                }
            }
            else
            {
                _bareValues.Add(args[i]);
            }

            return true;
        }


    }
}
