using System;
using System.IO;
using System.Text;
using Antlr4.Runtime.Tree;

namespace Project
{
    public class TreeVisitor : ProjectGrammarBaseVisitor<Type>
    {
        private SymbolTable _symbols = new SymbolTable();
        private StringBuilder _instructions = new StringBuilder();
        private int _lastLabel = -1;

        public void DumpToFile(string filename)
        {
            File.WriteAllText(filename, _instructions.ToString().Trim());
        }

        public string GetCodeString()
        {
            return _instructions.ToString();
        }
        
        public override Type VisitExpression(ProjectGrammarParser.ExpressionContext context)
        {
            var type = Visit(context.expr());
            _instructions.AppendLine(Instruction.Pop);
            
            return Type.Error;
        }

        public override Type VisitDeclaration(ProjectGrammarParser.DeclarationContext context)
        {
            var type = Visit(context.variableType());
            foreach (var identifier in context.ID())
            {
                _symbols.Add(identifier.Symbol, type);
                switch (type)
                {
                    case Type.Boolean:
                        _instructions.AppendLine(Instruction.Push(type, "false"));
                        break;
                    case Type.Int:
                        _instructions.AppendLine(Instruction.Push(type, 0));
                        break;
                    case Type.Float:
                        _instructions.AppendLine(Instruction.Push(type, 0.0f));
                        break;
                    case Type.String:
                        _instructions.AppendLine(Instruction.Push(type, "\"\""));
                        break;
                }

                _instructions.AppendLine(Instruction.Save(identifier.ToString()));
            }
            

            return Type.Error;
        }

        public override Type VisitBlockOfStatements(ProjectGrammarParser.BlockOfStatementsContext context)
        {
            return Visit(context.block());
        }

        public override Type VisitUnaryMinus(ProjectGrammarParser.UnaryMinusContext context)
        {
            var right = Visit(context.expr());
            
            switch (right)
            {
                case Type.Float:
                {
                    _instructions.AppendLine(Instruction.Uminus);
                    return Type.Float;
                }
                case Type.Int:
                {
                    _instructions.AppendLine(Instruction.Uminus);
                    return Type.Int;
                }
                default:
                {
                    Errors.ReportError(context.start, $"Operator '-' expected int or float, got {right}.");
                    return Type.Error;
                }
            }
        }

        public override Type VisitNot(ProjectGrammarParser.NotContext context)
        {
            var type = Visit(context.expr());
            
            if (type == Type.Boolean)
            {
                _instructions.AppendLine(Instruction.Not);
                return Type.Boolean;
            }
            
            Errors.ReportError(context.start, $"Operator '!' expected boolean, got {type}.");
            return Type.Error;
        }

        public override Type VisitMulDivMod(ProjectGrammarParser.MulDivModContext context)
        {
            var left = Visit(context.expr()[0]);
            int afterLeftIndex = _instructions.Length;
            var right = Visit(context.expr()[1]);

            if (left == Type.Error || right == Type.Error) return Type.Error;
            
            if (left == Type.Boolean || right == Type.Boolean)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type bool.");
                return Type.Error;
            }
            if (left == Type.String || right == Type.String)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type String.");
                return Type.Error;
            }

            if (left == Type.Float)
            {
                if (right == Type.Float || right == Type.Int)
                {
                    if (right == Type.Int)
                        _instructions.AppendLine(Instruction.ItoF);

                    switch (context.op.Type)
                    {
                        case ProjectGrammarParser.MUL:
                        {
                            _instructions.AppendLine(Instruction.Mul(Type.Float));
                            return Type.Float;
                        }
                        case ProjectGrammarParser.DIV:
                        {
                            _instructions.AppendLine(Instruction.Div(Type.Float));
                            return Type.Float;
                        }
                        default:
                            return Type.Error;
                    }
                }
            }

            if (left == Type.Int && right == Type.Float)
            {
                _instructions.Insert(afterLeftIndex, Instruction.ItoF + "\n\r");
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.MUL:
                    {
                        _instructions.AppendLine(Instruction.Mul(Type.Float));
                        return Type.Float;
                    }
                    case ProjectGrammarParser.DIV:
                    {
                        _instructions.AppendLine(Instruction.Div(Type.Float));
                        return Type.Float;
                    }
                    default:
                        return Type.Error;
                }
            }

            if (left == Type.Int && right == Type.Int)
            {
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.MUL:
                    {
                        _instructions.AppendLine(Instruction.Mul(Type.Int));
                        return Type.Int;
                    }
                    case ProjectGrammarParser.DIV:
                    {
                        _instructions.AppendLine(Instruction.Div(Type.Int));
                        return Type.Int;
                    }
                    case ProjectGrammarParser.MOD:
                    {
                        _instructions.AppendLine(Instruction.Mod);
                        return Type.Int;
                    }
                }
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected int, got {right}.");
            return Type.Error;
        }

        public override Type VisitAddSubConcat(ProjectGrammarParser.AddSubConcatContext context)
        {
            var left = Visit(context.expr()[0]);
            int afterLeftIndex = _instructions.Length;
            var right = Visit(context.expr()[1]);

            if (left == Type.Error || right == Type.Error) return Type.Error;
            
            if (left == Type.Boolean || right == Type.Boolean)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type bool.");
                return Type.Error;
            }

            if (left == Type.String)
            {
                if (context.op.Type == ProjectGrammarParser.CONCAT)
                {
                    if (right == Type.String)
                    {
                        _instructions.AppendLine(Instruction.Concat);
                        return Type.String;
                    }
                    
                    Errors.ReportError(context.start, $"Operator '.' expected String, got {right}.");
                    return Type.Error;
                }
                
                Errors.ReportError(context.start, $"Operator {context.op.Text} does not support type String.");
                return Type.Error;
            }

            if (left == Type.Float)
            {
                if (right == Type.Float || right == Type.Int)
                {
                    if (right == Type.Int)
                        _instructions.AppendLine(Instruction.ItoF);

                    switch (context.op.Type)
                    {
                        case ProjectGrammarParser.ADD:
                        {
                            _instructions.AppendLine(Instruction.Add(Type.Float));
                            return Type.Float;
                        }
                        case ProjectGrammarParser.SUB:
                        {
                            _instructions.AppendLine(Instruction.Sub(Type.Float));
                            return Type.Float;
                        }
                        default:
                            return Type.Error;
                    }
                }

                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {right}.");
                return Type.Error;
            }

            if (left == Type.Int && right == Type.Float)
            {
                _instructions.Insert(afterLeftIndex, Instruction.ItoF + "\n\r");
                
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.ADD:
                    {
                        _instructions.AppendLine(Instruction.Add(Type.Float));
                        return Type.Float;
                    }
                    case ProjectGrammarParser.SUB:
                    {
                        _instructions.AppendLine(Instruction.Sub(Type.Float));
                        return Type.Float;
                    }
                    default:
                        return Type.Error;
                }   
            }

            if (left == Type.Int && right == Type.Int)
            {
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.ADD:
                    {
                        _instructions.AppendLine(Instruction.Add(Type.Int));
                        return Type.Int;
                    }
                    case ProjectGrammarParser.SUB:
                    {
                        _instructions.AppendLine(Instruction.Sub(Type.Int));
                        return Type.Int;
                    }
                    default:
                        return Type.Error;
                }
            }
            
            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected int, got {right}.");
            return Type.Error;
        }

        public override Type VisitEqNeq(ProjectGrammarParser.EqNeqContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left == Type.Error || right == Type.Error) return Type.Error;
            if (left == Type.Boolean && right == Type.Boolean)
            {
                if (context.op.Type == ProjectGrammarParser.EQ)
                    _instructions.AppendLine(Instruction.Eq);
                else
                {
                    _instructions.AppendLine(Instruction.Eq);
                    _instructions.AppendLine(Instruction.Not);
                }
                
                return Type.Boolean;
            }

            if (left == Type.String && right == Type.String)
            {
                if (context.op.Type == ProjectGrammarParser.EQ)
                    _instructions.AppendLine(Instruction.Eq);
                else
                {
                    _instructions.AppendLine(Instruction.Eq);
                    _instructions.AppendLine(Instruction.Not);
                }
                return Type.Boolean;            
            }

            if (left == Type.Float && right == Type.Float)
            {
                if (context.op.Type == ProjectGrammarParser.EQ)
                    _instructions.AppendLine(Instruction.Eq);
                else
                {
                    _instructions.AppendLine(Instruction.Eq);
                    _instructions.AppendLine(Instruction.Not);
                }
                
                return Type.Boolean;            
            }

            if (left == Type.Float && right == Type.Int || left == Type.Int && right == Type.Float)
            {
                if (right == Type.Int)
                    _instructions.AppendLine(Instruction.ItoF);
                else
                    _instructions.Insert(_instructions.Length - 1, Instruction.ItoF);
                
                if (context.op.Type == ProjectGrammarParser.EQ)
                    _instructions.AppendLine(Instruction.Eq);
                else
                {
                    _instructions.AppendLine(Instruction.Eq);
                    _instructions.AppendLine(Instruction.Not);
                }
                return Type.Boolean;            
            }

            if (left == Type.Int && right == Type.Int)
            {
                if (context.op.Type == ProjectGrammarParser.EQ)
                    _instructions.AppendLine(Instruction.Eq);
                else
                {
                    _instructions.AppendLine(Instruction.Eq);
                    _instructions.AppendLine(Instruction.Not);
                }
                
                return Type.Boolean;            
            }
            
            if (left == Type.Float && right != Type.Int && right != Type.Float)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {left} and {right}.");
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected {left}, got {right}.");

            return Type.Error;
        }

        public override Type VisitLtGt(ProjectGrammarParser.LtGtContext context)
        {
            var left = Visit(context.expr()[0]);
            int afterLeftIndex = _instructions.Length;
            var right = Visit(context.expr()[1]);

            if (left == Type.Error || right == Type.Error) return Type.Error;

            if (left == Type.Float && right == Type.Float)
            {
                _instructions.AppendLine(context.op.Type == ProjectGrammarParser.LT ? Instruction.Lt : Instruction.Gt);

                return Type.Boolean;
            }

            if (left == Type.Float && right == Type.Int)
            {
                _instructions.AppendLine(Instruction.ItoF);
                _instructions.AppendLine(context.op.Type == ProjectGrammarParser.LT ? Instruction.Lt : Instruction.Gt);
                return Type.Boolean;
            }

            if (left == Type.Int && right == Type.Int)
            {
                _instructions.AppendLine(context.op.Type == ProjectGrammarParser.LT ? Instruction.Lt : Instruction.Gt);
                return Type.Boolean;
            }

            if (left == Type.Int && right == Type.Float)
            {
                _instructions.Insert(afterLeftIndex, Instruction.ItoF + "\n\r");
                _instructions.AppendLine(context.op.Type == ProjectGrammarParser.LT ? Instruction.Lt : Instruction.Gt);
                return Type.Boolean;
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {left} and {right}.");

            return Type.Error;
        }

        public override Type VisitAnd(ProjectGrammarParser.AndContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left == Type.Boolean && right == Type.Boolean)
            {
                _instructions.AppendLine(Instruction.And);
                return Type.Boolean;
            }
            
            Errors.ReportError(context.start, $"And operator '&&' expected booleans, got {left} and {right}.");
            return Type.Error;
        }

        public override Type VisitOr(ProjectGrammarParser.OrContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left == Type.Boolean && right == Type.Boolean)
            {
                _instructions.AppendLine(Instruction.Or);
                return Type.Boolean;
            }
            
            Errors.ReportError(context.start, $"Or operator '||' expected booleans, got {left} and {right}.");
            return Type.Error;
        }

        public override Type VisitParentheses(ProjectGrammarParser.ParenthesesContext context)
        {
            return Visit(context.expr());
        }

        public override Type VisitAssignment(ProjectGrammarParser.AssignmentContext context)
        {
            var right = Visit(context.expr());
            var variable = _symbols[context.ID().Symbol];

            // Bad
            if (variable == Type.Error || right == Type.Error) return Type.Error;
            
            // Int
            if (variable == Type.Int && right == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is float.");
                return Type.Error;
            }
            if (variable == Type.Int && right == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is bool.");
                return Type.Error;
            }
            if (variable == Type.Int && right == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is String.");
                return Type.Error;
            }
            
            // Bool
            if (variable == Type.String && right == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is float.");
                return Type.Error;
            }
            if (variable == Type.String && right == Type.Int)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is int.");
                return Type.Error;
            }
            if (variable == Type.String && right == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is bool.");
                return Type.Error;
            }
            
            // Bool
            if (variable == Type.Boolean && right == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is float.");
                return Type.Error;
            }
            if (variable == Type.Boolean && right == Type.Int)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is int.");
                return Type.Error;
            }
            if (variable == Type.Boolean && right == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is String.");
                return Type.Error;
            }
            
            // Float
            if (variable == Type.Float && right == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is float, but the assigned value is bool.");
                return Type.Error;
            }
            if (variable == Type.Float && right == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is float, but the assigned value is String.");
                return Type.Error;
            }

            // Good
            if (variable == Type.Float && right == Type.Int)
            {
                var value = Type.Float;
                _symbols[context.ID().Symbol] = value;

                _instructions.AppendLine(Instruction.ItoF);
                _instructions.AppendLine(Instruction.Save(context.ID().GetText()));
                _instructions.AppendLine(Instruction.Load(context.ID().GetText()));

                return Type.Float;
            }

            _symbols[context.ID().Symbol] = right;
            
            _instructions.AppendLine(Instruction.Save(context.ID().GetText()));
            _instructions.AppendLine(Instruction.Load(context.ID().GetText()));

            return right;
        }

        public override Type VisitBlock(ProjectGrammarParser.BlockContext context)
        {
            foreach (var statement in context.stat())
            {
                Visit(statement);
            }

            return Type.Error;
        }

        public override Type VisitInt(ProjectGrammarParser.IntContext context)
        {
            var value = Convert.ToInt32(context.INT().GetText());
;            _instructions.AppendLine(Instruction.Push(Type.Int, value));
            return Type.Int;
        }

        public override Type VisitFloat(ProjectGrammarParser.FloatContext context)
        {
            var value = (float)Convert.ToDecimal(context.FLOAT().GetText());
            _instructions.AppendLine(Instruction.Push(Type.Float, value));
            return Type.Float;
        }

        public override Type VisitString(ProjectGrammarParser.StringContext context)
        {
            _instructions.AppendLine(Instruction.Push(Type.String, context.STRING().GetText()));
            return Type.String;
        }

        public override Type VisitBool(ProjectGrammarParser.BoolContext context)
        {
            var value = context.BOOL().GetText() == "true" ? "true" : "false";
            _instructions.AppendLine(Instruction.Push(Type.Boolean, value));
            return Type.Boolean;
        }

        public override Type VisitId(ProjectGrammarParser.IdContext context)
        {
            _instructions.AppendLine(Instruction.Load(context.ID().GetText()));
            return _symbols[context.ID().Symbol];
        }

        public override Type VisitWhileStat(ProjectGrammarParser.WhileStatContext context)
        {
            var startingLabel = _lastLabel + 1;
            var endingLabel = _lastLabel + 2;
            _lastLabel = endingLabel;
            
            _instructions.AppendLine(Instruction.Label(startingLabel));
            
            var expr = Visit(context.expr());
            if (expr != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr}.");
                return Type.Error;
            }

            _instructions.AppendLine(Instruction.Fjmp(endingLabel));
            Visit(context.stat());
            _instructions.AppendLine(Instruction.Jmp(startingLabel));
            
            _instructions.AppendLine(Instruction.Label(endingLabel));

            return Type.Error;
        }

        public override Type VisitDoWhileStat(ProjectGrammarParser.DoWhileStatContext context)
        {
            var startingLabel = _lastLabel + 1;
            var endingLabel = _lastLabel + 2;
            _lastLabel = endingLabel;
            
            _instructions.AppendLine(Instruction.Label(startingLabel));
            Visit(context.stat());
         
            var expr = Visit(context.expr());
            if (expr != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr}.");
                return Type.Error;
            }
            _instructions.AppendLine(Instruction.Fjmp(endingLabel));
            _instructions.AppendLine(Instruction.Fjmp(startingLabel));

            _instructions.AppendLine(Instruction.Label(endingLabel));
            return Type.Error;
        }

        public override Type VisitProg(ProjectGrammarParser.ProgContext context)
        {
            foreach (var statement in context.stat())
            {
                var value = Visit(statement);
                //if (value != Type.Error) Console.WriteLine(value);
                // else
                //     Errors.PrintAndClearErrors();
            }

            return Type.Error;
        }
        
        public override Type VisitVariableType(ProjectGrammarParser.VariableTypeContext context)
        {
            switch (context.type.Text.Trim())
            {
                case "int": return Type.Int;
                case "float": return Type.Float;
                case "bool": return Type.Boolean;
                case "string": return Type.String;
            }

            return Type.Error;
        }

        public override Type VisitRead(ProjectGrammarParser.ReadContext context)
        {
            foreach (var id in context.ID())
            {
                var variable = _symbols[id.Symbol];
                if (variable == Type.Error)
                {
                    Errors.ReportError(context.start, $"Could not read variable {variable}. Variable not found.");
                }

                _instructions.AppendLine(Instruction.Read(variable));
                _instructions.AppendLine(Instruction.Save(id.ToString()));
            }

            return Type.Error;
        }

        public override Type VisitWrite(ProjectGrammarParser.WriteContext context)
        {
            foreach (var expr in context.expr())
            {
                var value = Visit(expr);
            }

            _instructions.AppendLine(Instruction.Print(context.expr().Length));
            
            return Type.Error;
        }

        public override Type VisitIfStat(ProjectGrammarParser.IfStatContext context)
        {
            var exitLabel = _lastLabel + 1;
            _lastLabel = exitLabel;
            
            var expr = Visit(context.expr());
            if (expr != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr}.");
                return Type.Error;
            }

            _instructions.AppendLine(Instruction.Fjmp(exitLabel));
            Visit(context.stat());
            _instructions.AppendLine(Instruction.Label(exitLabel));
            
            return Type.Error;
        }

        public override Type VisitIfStatElseStat(ProjectGrammarParser.IfStatElseStatContext context)
        {
            var falseLabel = _lastLabel + 1;
            var exitLabel = _lastLabel + 2;
            _lastLabel = exitLabel;

            var expr = Visit(context.expr());
            if (expr != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr}.");
                return Type.Error;
            }

            _instructions.AppendLine(Instruction.Fjmp(falseLabel));
            
            Visit(context.stat()[0]);
            _instructions.AppendLine(Instruction.Jmp(exitLabel));

            _instructions.AppendLine(Instruction.Label(falseLabel));
            Visit(context.stat()[1]);
            _instructions.AppendLine(Instruction.Label(exitLabel));

            return Type.Error;
        }
        
        public override Type VisitTernaryCondCond(ProjectGrammarParser.TernaryCondCondContext context)
        {
            var exprCond = Visit(context.expr()[0]);
            
            var exprTrue = Visit(context.expr()[1]);
            var exprFalse = Visit(context.expr()[2]);
        
            if (exprCond != Type.Boolean)
            {
                Errors.ReportError(context.expr()[0].start, $"Ternary operator expected boolean, got {exprCond}.");
                return Type.Error;
            }

            if ((exprTrue == Type.Int && exprFalse == Type.Float) ||
                (exprTrue == Type.Float && exprFalse == Type.Int))
            {
                return Type.Int;
            }
            
            if (exprTrue != exprFalse)
            {
                Errors.ReportError(context.expr()[0].start, $"Return types must match, got {exprTrue} and {exprFalse}.");
                return Type.Error;
            }

            return Type.Error;
        }
    }
}