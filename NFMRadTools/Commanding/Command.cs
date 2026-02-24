using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFMRadTools.Commanding
{
    public class Command
    {
        private MethodInfo _method;
        private ParameterInfo[] _params;
        public required string Name { get; init; }
        public string Description { get; init; }
        public required MethodInfo Method 
        { 
            get => _method;
            init
            {
                _method = value;
                if(_method is not null)
                {
                    _params = _method.GetParameters();
                }
            }
        }
        public required bool VerifyCarLoaded { get; init; }
        public bool HasArgs
        {
            get
            {
                if (_params is null) return false;
                return _params.Length > 0;
            }
        }
        public Command()
        {

        }

        public void Execute(string args)
        {
            if(args is null || !HasArgs)
            {
                Method.Invoke(null, Array.Empty<object>());
                return;
            }
            int argsLen = _params.Length;
            ArgumentEnumerator argEnumerator = new ArgumentEnumerator(args);
            int index = 0;
            object[] arrArgs = new object[argsLen];
            foreach(ParameterInfo pi in _params)
            {
                if(!argEnumerator.MoveNext())
                {
                    if(pi.IsOptional && pi.HasDefaultValue)
                    {
                        arrArgs[index] = pi.DefaultValue;
                        index++;
                        continue;
                    }
                    Logger.Error($"Missing argument for [{TypeNames.GetTypeName(pi.ParameterType)} {pi.Name}].");
                    return;
                }
                if (!ValueParser.TryParse(argEnumerator.Current.ToString(), pi.ParameterType, out object obj))
                {
                    Logger.Error($"Failed to parse value for [{TypeNames.GetTypeName(pi.ParameterType)} {pi.Name}].");
                    return;
                }
                arrArgs[index] = obj;
                index++;
            }
            Method.Invoke(null, arrArgs);
        }

        public override string ToString()
        {
            ParameterInfo[] parameters = Method.GetParameters();
            if (parameters.Length <= 0) return Name;
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            sb.Append(" [");
            int i = 0;
            foreach (ParameterInfo param in parameters)
            {
                sb.Append(TypeNames.GetTypeName(param.ParameterType));
                sb.Append(" ");
                sb.Append(param.Name);
                i++;
                if(i < parameters.Length) sb.Append(", ");
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
