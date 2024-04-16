using System;

namespace Project
{
    public static class Instruction
    {
        public static string Add(Type type)
        {
            if (type == Type.Int)
                return "add I";
            if (type == Type.Float)
                return "add F";
            return "add";
        }

        public static string Sub(Type type)
        {
            if (type == Type.Int)
                return "sub I";
            if (type == Type.Float)
                return "sub F";
            return "sub";
        }

        public static string Mul(Type type)
        {
            if (type == Type.Int)
                return "mul I";
            if (type == Type.Float)
                return "mul F";
            return "mul";
        }

        public static string Div(Type type)
        {
            if (type == Type.Int)
                return "div I";
            if (type == Type.Float)
                return "div F";
            return "div";
        }
        public static string Mod => "mod";
        public static string Uminus => "uminus";
        public static string Concat => "concat";
        public static string And => "and";
        public static string Or => "or";
        public static string Gt => "gt";
        public static string Lt => "lt";
        public static string Eq => "eq";
        public static string Not => "not";
        public static string ItoF => "itof";
        public static string Push(Type type, object value)
        {
            switch (type)
            {
                case Type.Boolean:
                    return $"push B {value}";
                
                case Type.Float:
                    return $"push F {((float)value == 0.0f ? "0.0" : value.ToString())}";
                
                case Type.Int:
                    return $"push I {value}";
                
                case Type.String:
                    return $"push S {value}";
                
                default:
                    throw new Exception();
            }
        }
        public static string Pop => "pop";
        public static string Load(string id) => $"load {id}";
        public static string Save(string id) => $"save {id}";
        public static string Label(int n) => $"label {n}";
        public static string Jmp(int n) => $"jmp {n}";
        public static string Fjmp(int n) => $"fjmp {n}";
        public static string Print(int n) => $"print {n}";
        public static string Read(Type type)
        {
            switch (type)
            {
                case Type.Boolean:
                    return $"read B";
                
                case Type.Float:
                    return $"read F";
                
                case Type.Int:
                    return $"read I";
                
                case Type.String:
                    return $"read S";
                
                default:
                    throw new Exception();
            }
        }
    }
}