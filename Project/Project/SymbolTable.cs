using System.Collections.Generic;
using Antlr4.Runtime;

namespace Project
{
    public class SymbolTable
    {
        Dictionary<string, Type> memory = new Dictionary<string, Type>();

        public void Add(IToken variable, Type type)
        {
            var name = variable.Text.Trim();

            if (memory.ContainsKey(name))
            {
                Errors.ReportError(variable, $"Variable {name} was already declared.");
            }
            else
            {
                switch (type)
                {
                    case Type.Boolean:
                    {
                        memory.Add(name, type);
                        break;
                    }

                    case Type.Int:
                    {
                        memory.Add(name, type);
                        break;
                    }

                    case Type.Float:
                    {
                        memory.Add(name, type);
                        break;
                    }

                    default:
                    {
                        memory.Add(name, type);
                        break;
                    }
                }
            }
        }

        public Type this[IToken variable]
        {
            get
            {
                var name = variable.Text.Trim();
                if (memory.TryGetValue(name, out var item))
                {
                    return item;
                }
                else
                {
                    Errors.ReportError(variable, $"Variable {name} was not declared.");
                    return Type.Error;
                }
            }

            set
            {
                var name = variable.Text.Trim();
                memory[name] = value;
            }
        }
    }
}