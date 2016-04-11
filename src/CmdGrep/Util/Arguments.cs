using System;
using System.Collections.Specialized;
using System.Text.RegularExpressions;

namespace CommandLine.Utility
{
    /// <summary>
    /// Arguments class
    /// </summary>
    public class Arguments
    {
        private static readonly Regex _spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled); // Matches (-,/ or --) and a possible enclosed value (=,:)
        private static readonly Regex _remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled); // Matches possible enclosing characters (",')

        private readonly HybridDictionary _parameters = new HybridDictionary();

        public object this[string param]
        {
            get
            {
                return (_parameters[param]);
            }
        }

        /// <summary>
        /// Provides a class that parses argument string values into a case-sensitive dictionary of key value pairs.
        /// Valid parameters forms: {-,/,--}param{ ,=,:}((",')value(",'))
        /// Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
        /// </summary>
        /// <param name="args">Array of argument string values</param>
        public Arguments(string[] args)
        {
            var parameter = default(object);
            foreach (var arg in args)
            {
                var parts = _spliter.Split(arg, 3);
                switch (parts.Length)
                {
                    case 1:
                        if (parameter != null)
                        {
                            if (!_parameters.Contains(parameter))
                            {
                                // Found a value that belongs to the previous argument parameter key
                                var value = _remover.Replace(parts[0], "$1");
                                _parameters[parameter] = value;
                            }
                            parameter = null;
                        }
                        // else Error: no parameter key to set, skipping this invalid argument parameter form syntax
                        break;
                    case 2:
                        // Adds previously existing parameter key as an argument switch parameter
                        AddSwitchParam(parameter);
                        // Found a new parameter key
                        parameter = parts[1];
                        break;
                    case 3:
                        // Adds previously existing parameter key as an argument switch parameter
                        AddSwitchParam(parameter);
                        // Found a new parameter key with an enclosed value
                        parameter = parts[1];
                        if (!_parameters.Contains(parameter))
                        {
                            var value = _remover.Replace(parts[2], "$1");
                            _parameters[parameter] = value;
                        }
                        parameter = null;
                        break;
                }
            }
            // Adds previously existing parameter key as an argument switch parameter
            AddSwitchParam(parameter);
        }

        /// <summary>
        ///  Checks that a parameter is not null and not added to the collection.
        ///  When true, adds the key and sets its value to true.
        /// </summary>
        /// <param name="key">The key of the entry to add</param>
        private void AddSwitchParam(object key)
        {
            if (key != null && !_parameters.Contains(key))
                _parameters[key] = true;
        }
    }
}
