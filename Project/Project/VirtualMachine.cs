using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Project
{
    public class VirtualMachine
    {
        private Stack<object> stack = new Stack<object>();
        private List<String[]> code = new List<String[]>();
        private Dictionary<string, object> memory = new Dictionary<string, object>();

        public VirtualMachine(string inputCode)
        {
            var lines = inputCode.Split("\n\r".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var line in lines)
            {
                var parts = Regex.Matches(line, @"[\""].+?[\""]|[^ ]+")
                    .Cast<Match>()
                    .Select(m => m.Value.Trim('"'))
                    .ToArray();
                
                code.Add(parts);
            }
            
        }

        public void Run()
        {
            for (var index = 0; index < code.Count; index++)
            {
                var instruction = code[index];
                
                if (instruction[0].StartsWith("push"))
                {
                    var type = instruction[1];
                    var value = instruction[2];

                    switch (type)
                    {
                        case "I":
                            stack.Push(int.Parse(value));
                            break;
                        case "F":
                            stack.Push(float.Parse(value));
                            break;
                        case "S":
                            stack.Push(value.Replace("\"", ""));
                            break;
                        case "B":
                            stack.Push(bool.Parse(value));
                            break;
                    }
                }
                else if (instruction[0].StartsWith("pop"))
                {
                    stack.Pop();
                }
                else if (instruction[0].StartsWith("load"))
                {
                    var value = memory[instruction[1]];
                    stack.Push(value);
                }
                else if (instruction[0].StartsWith("save"))
                {
                    var id = instruction[1];
                    var value = stack.Pop();
                    memory[id] = value;
                }
                else if (instruction[0].StartsWith("jmp") || instruction[0].StartsWith("fjmp"))
                {
                    var n = int.Parse(instruction[1]);

                    for (int searchedIndex = 0; searchedIndex < code.Count; searchedIndex++)
                    {
                        var searchedInstruction = code[searchedIndex];
                        if (searchedInstruction[0].StartsWith("label"))
                        {
                            if (int.Parse(searchedInstruction[1]) == n)
                            {
                                if (instruction[0].StartsWith("jmp"))
                                {
                                    index = searchedIndex;
                                    break;
                                }
                                
                                var lastBool = (bool)stack.Pop();
                                if (!lastBool && instruction[0].StartsWith("fjmp"))
                                {
                                    index = searchedIndex;
                                    break;
                                }
                                stack.Push(lastBool);
                            }
                        }
                    }
                }
                else if (instruction[0].StartsWith("print"))
                {
                    var n = int.Parse(instruction[1]);
                    var toPrint = new List<object>();
                    
                    for (var i = 0; i < n; i++)
                        toPrint.Add(stack.Pop());

                    foreach (object item in toPrint.ToArray().Reverse())
                    {
                        Console.Write(item);
                    }
                    Console.WriteLine();
                }
                else if (instruction[0].StartsWith("read"))
                {
                    var type = instruction[1];
                    switch (type)
                    {
                        case "I":
                            Console.WriteLine("Input int:");
                            stack.Push(int.Parse(Console.ReadLine()));
                            break;
                        case "F":
                            Console.WriteLine("Input float:");
                            stack.Push(float.Parse(Console.ReadLine()));
                            break;
                        case "S":
                            Console.WriteLine("Input String:");
                            stack.Push(Console.ReadLine());
                            break;
                        case "B":
                            Console.WriteLine("Input Boolean:");
                            stack.Push(bool.Parse(Console.ReadLine()));
                            break;
                    }
                }
                else if (instruction[0].StartsWith("not"))
                {
                    var value = (bool)stack.Pop();
                    stack.Push(!value);
                }
                else if (instruction[0].StartsWith("uminus"))
                {
                    var value = stack.Pop();

                    if (value is int)
                    {
                        stack.Push((int)value * -1);
                    }
                    else
                    {
                        stack.Push((float)value * -1.0f);
                    }
                }
                else if (instruction[0].StartsWith("itof"))
                {
                    var value = (int)stack.Pop();
                    stack.Push((float)value);
                }
                else if (instruction[0].StartsWith("label")) continue;
                else
                {
                    var right = stack.Pop();
                    var left = stack.Pop();
                    switch (instruction[0])
                    {
                        case "add" when left is int && right is int:
                            stack.Push((int)left + (int)right);
                            break;
                        case "add": 
                            stack.Push((float)left + (float)right);
                            break;
                        
                        case "sub" when left is int && right is int:
                            stack.Push((int)left - (int)right);
                            break;
                        case "sub": 
                            stack.Push((float)left - (float)right);
                            break;
                        
                        case "mul" when left is int && right is int:
                            stack.Push((int)left * (int)right);
                            break;
                        case "mul": 
                            stack.Push((float)left * (float)right);
                            break;
                        
                        case "div" when left is int && right is int:
                            stack.Push((int)left / (int)right);
                            break;
                        case "div": 
                            
                            stack.Push((float)left / (float)right);
                            break;
                        case "mod" when left is int && right is int:
                            stack.Push((int)left % (int)right);
                            break;
                        
                        case "concat":
                            stack.Push(string.Concat((string)left, (string)right));
                            break;
                        
                        case "and":
                            stack.Push((bool)left & (bool)right);
                            break;
                        
                        case "or":
                            stack.Push((bool)left | (bool)right);
                            break;
                        
                        case "lt" when left is int && right is int:
                            stack.Push((int)left < (int)right);
                            break;
                        case "lt":
                            stack.Push((float)left < (float)right);
                            break;
                        
                        case "gt" when left is int && right is int:
                            stack.Push((int)left > (int)right);
                            break;
                        case "gt": 
                            stack.Push((float)left > (float)right);
                            break;
                        
                        case "eq" when left is int && right is int:
                            stack.Push((int)left == (int)right);
                            break;
                        case "eq" when left is float && right is float:
                            stack.Push((float)left == (float)right);
                            break;
                        case "eq" when left is string && right is string:
                            stack.Push((string)left == (string)right);
                            break;
                        case "eq" when left is bool && right is bool:
                            stack.Push((bool)left == (bool)right);
                            break;
                    }
                }
            } 
        }
    }
}