using System.Collections.Generic;
using Antlr4.Runtime;

namespace Project
{
    public class SymbolTable
    {
        Dictionary<string, (Type Type, object Value)> memory = new Dictionary<string, (Type Type, object Value)>();

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
                        memory.Add(name, (type, false));
                        break;
                    }

                    case Type.Int:
                    {
                        memory.Add(name, (type, 0));
                        break;
                    }

                    case Type.Float:
                    {
                        memory.Add(name, (type, 0f));
                        break;
                    }

                    default:
                    {
                        memory.Add(name, (type, ""));
                        break;
                    }
                }
            }
        }

        public (Type Type, object Value) this[IToken variable]
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
                    return (Type.Error, 0);
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