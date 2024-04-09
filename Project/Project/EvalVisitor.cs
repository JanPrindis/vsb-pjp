using System;
using System.Linq;
using Antlr4.Runtime.Tree;

namespace Project
{
    public class EvalVisitor : ProjectGrammarBaseVisitor<(Type Type, object Value)>
    {
        private SymbolTable symbols = new SymbolTable();

        private float ToFloat(object value)
        {
            if (value is int i) return (float)i;
            return (float)value;
        }

        public override (Type Type, object Value) VisitExpression(ProjectGrammarParser.ExpressionContext context)
        {
            var value = Visit(context.expr());
            if (value.Type == Type.Error)
                Errors.PrintAndClearErrors();
            // else 
            //     Console.WriteLine(value.Value);

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitDeclaration(ProjectGrammarParser.DeclarationContext context)
        {
            var type = Visit(context.variableType());
            foreach (var identifier in context.ID())
            {
                symbols.Add(identifier.Symbol, type.Type);
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitBlockOfStatements(ProjectGrammarParser.BlockOfStatementsContext context)
        {
            return Visit(context.block());
        }

        public override (Type Type, object Value) VisitUnaryMinus(ProjectGrammarParser.UnaryMinusContext context)
        {
            var right = Visit(context.expr());
            
            switch (right.Type)
            {
                case Type.Float:
                    return (Type.Float, -1 * ToFloat(right.Value));
                case Type.Int:
                    return (Type.Int, -1 * (int)right.Value);
                default:
                {
                    Errors.ReportError(context.start, $"Operator '-' expected int or float, got {right.Type}.");
                    return (Type.Error, 0);
                }
            }
        }

        public override (Type Type, object Value) VisitNot(ProjectGrammarParser.NotContext context)
        {
            var right = Visit(context.expr());
            
            if (right.Type == Type.Boolean) return (Type.Boolean, !(bool)right.Value);
            
            Errors.ReportError(context.start, $"Operator '!' expected boolean, got {right.Type}.");
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitMulDivMod(ProjectGrammarParser.MulDivModContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            
            if (left.Type == Type.Boolean || right.Type == Type.Boolean)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type bool.");
                return (Type.Error, 0);
            }
            if (left.Type == Type.String || right.Type == Type.String)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type String.");
                return (Type.Error, 0);
            }

            if (left.Type == Type.Float)
            {
                if (right.Type == Type.Float || right.Type == Type.Int)
                {

                    switch (context.op.Type)
                    {
                        case ProjectGrammarParser.MUL:
                            return (Type.Float, ToFloat(left.Value) * ToFloat(right.Value));
                        case ProjectGrammarParser.DIV:
                            return (Type.Float, ToFloat(left.Value) / ToFloat(right.Value));
                        default:
                            return (Type.Error, 0);
                    }
                }
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {right.Type}.");
                return (Type.Error, 0);
            }

            if (right.Type == Type.Int)
            {
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.MUL:
                        return (Type.Int, (int)left.Value * (int)right.Value);
                    case ProjectGrammarParser.DIV:
                        return (Type.Int, (int)left.Value / (int)right.Value);
                    case ProjectGrammarParser.MOD:
                        return (Type.Int, (int)left.Value % (int)right.Value);
                }
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected int, got {right.Type}.");
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitAddSubConcat(ProjectGrammarParser.AddSubConcatContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            
            if (left.Type == Type.Boolean || right.Type == Type.Boolean)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' does not support type bool.");
                return (Type.Error, 0);
            }

            if (left.Type == Type.String)
            {
                if (context.op.Type == ProjectGrammarParser.CONCAT)
                {
                    if (right.Type == Type.String) return (Type.String, left.Value.ToString() + right.Value);
                    
                    Errors.ReportError(context.start, $"Operator '.' expected String, got {right.Type}.");
                    return (Type.Error, 0);
                }
                
                Errors.ReportError(context.start, $"Operator {context.op.Text} does not support type String.");
                return (Type.Error, 0);
            }

            if (left.Type == Type.Float)
            {
                if (right.Type == Type.Float || right.Type == Type.Int)
                {
                    switch (context.op.Type)
                    {
                        case ProjectGrammarParser.ADD:
                            return (Type.Float, ToFloat(left.Value) + ToFloat(right.Value));
                        case ProjectGrammarParser.SUB:
                            return (Type.Float, ToFloat(left.Value) - ToFloat(right.Value));
                        default:
                            return (Type.Error, 0);
                    }
                }

                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {right.Type}.");
                return (Type.Error, 0);
            }

            if (right.Type == Type.Int)
            {
                switch (context.op.Type)
                {
                    case ProjectGrammarParser.ADD:
                        return (Type.Int, (int)left.Value + (int)right.Value);
                    case ProjectGrammarParser.SUB:
                        return (Type.Int, (int)left.Value - (int)right.Value);
                    default:
                        return (Type.Error, 0);
                }
            }
            
            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected int, got {right.Type}.");
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitEqNeq(ProjectGrammarParser.EqNeqContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            if (left.Type == Type.Boolean && right.Type == Type.Boolean)
            {
                return context.op.Type == ProjectGrammarParser.EQ ? (Type.Boolean, (bool)left.Value == (bool)right.Value) : (Type.Boolean, (bool)left.Value != (bool)right.Value);
            }

            if (left.Type == Type.String && right.Type == Type.String)
            {
                return context.op.Type == ProjectGrammarParser.EQ ? (Type.Boolean, (string)left.Value == (string)right.Value) : (Type.Boolean, (string)left.Value != (string)right.Value);
            }

            if (left.Type == Type.Float && right.Type == Type.Float)
            {
                return context.op.Type == ProjectGrammarParser.EQ ? (Type.Boolean, (float)left.Value == (float)right.Value) : (Type.Boolean, (float)left.Value != (float)right.Value);
            }

            if (left.Type == Type.Float && right.Type == Type.Int)
            {
                return context.op.Type == ProjectGrammarParser.EQ ? (Type.Boolean, (float)left.Value == ToFloat(right.Value)) : (Type.Boolean, (float)left.Value != ToFloat(right.Value));
            }

            if (left.Type == Type.Int && right.Type == Type.Int)
            {
                return context.op.Type == ProjectGrammarParser.EQ ? (Type.Boolean, (int)left.Value == (int)right.Value) : (Type.Boolean, (int)left.Value != (int)right.Value);
            }
            
            if (left.Type == Type.Float && right.Type != Type.Int && right.Type != Type.Float)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {left.Type} and {right.Type}.");
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected {left.Type}, got {right.Type}.");

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitLtGt(ProjectGrammarParser.LtGtContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);

            if (left.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);

            if (left.Type == Type.Float && right.Type == Type.Float)
            {
                return context.op.Type == ProjectGrammarParser.LT ? (Type.Boolean, (float)left.Value < (float)right.Value) : (Type.Boolean, (float)left.Value > (float)right.Value);
            }

            if (left.Type == Type.Float && right.Type == Type.Int)
            {
                return context.op.Type == ProjectGrammarParser.LT ? (Type.Boolean, (float)left.Value < ToFloat(right.Value)) : (Type.Boolean, (float)left.Value > ToFloat(right.Value));
            }

            if (left.Type == Type.Int && right.Type == Type.Int)
            {
                return context.op.Type == ProjectGrammarParser.LT ? (Type.Boolean, (int)left.Value < (int)right.Value) : (Type.Boolean, (int)left.Value > (int)right.Value);
            }
            
            if (left.Type == Type.Float && right.Type != Type.Int && right.Type != Type.Float)
            {
                Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected float or int, got {left.Type} and {right.Type}.");
            }

            Errors.ReportError(context.start, $"Operator '{context.op.Text}' expected {left.Type}, got {right.Type}.");

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitAnd(ProjectGrammarParser.AndContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            
            if (left.Type == Type.Boolean && right.Type == Type.Boolean) return (Type.Boolean, (bool)left.Value & (bool)right.Value);
            
            Errors.ReportError(context.start, $"And operator '&&' expected booleans, got {left.Type} and {right.Type}.");
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitOr(ProjectGrammarParser.OrContext context)
        {
            var left = Visit(context.expr()[0]);
            var right = Visit(context.expr()[1]);
            
            if (left.Type == Type.Boolean && right.Type == Type.Boolean) return (Type.Boolean, (bool)left.Value | (bool)right.Value);
            
            Errors.ReportError(context.start, $"Or operator '||' expected booleans, got {left.Type} and {right.Type}.");
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitParentheses(ProjectGrammarParser.ParenthesesContext context)
        {
            return Visit(context.expr());
        }

        public override (Type Type, object Value) VisitAssignment(ProjectGrammarParser.AssignmentContext context)
        {
            var right = Visit(context.expr());
            var variable = symbols[context.ID().Symbol];

            // Bad
            if (variable.Type == Type.Error || right.Type == Type.Error) return (Type.Error, 0);
            
            // Int
            if (variable.Type == Type.Int && right.Type == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is float.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Int && right.Type == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is bool.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Int && right.Type == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is int, but the assigned value is String.");
                return (Type.Error, 0);
            }
            
            // Bool
            if (variable.Type == Type.String && right.Type == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is float.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.String && right.Type == Type.Int)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is int.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.String && right.Type == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is String, but the assigned value is bool.");
                return (Type.Error, 0);
            }
            
            // Bool
            if (variable.Type == Type.Boolean && right.Type == Type.Float)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is float.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Boolean && right.Type == Type.Int)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is int.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Boolean && right.Type == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is bool, but the assigned value is String.");
                return (Type.Error, 0);
            }
            
            // Float
            if (variable.Type == Type.Float && right.Type == Type.Boolean)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is float, but the assigned value is bool.");
                return (Type.Error, 0);
            }
            if (variable.Type == Type.Float && right.Type == Type.String)
            {
                Errors.ReportError(context.ID().Symbol, $"Variable '{context.ID().GetText()}' type is float, but the assigned value is String.");
                return (Type.Error, 0);
            }

            // Good
            if (variable.Type == Type.Float && right.Type == Type.Int)
            {
                var value = (Type.Float, ToFloat(right.Value));
                symbols[context.ID().Symbol] = value;
                return value;
            }

            symbols[context.ID().Symbol] = right;
            return right;
        }

        public override (Type Type, object Value) VisitBlock(ProjectGrammarParser.BlockContext context)
        {
            foreach (var statement in context.stat())
            {
                Visit(statement);
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitInt(ProjectGrammarParser.IntContext context)
        {
            return (Type.Int, int.Parse(context.INT().GetText()));
        }

        public override (Type Type, object Value) VisitFloat(ProjectGrammarParser.FloatContext context)
        {
            return (Type.Float, float.Parse(context.FLOAT().GetText()));
        }

        public override (Type Type, object Value) VisitString(ProjectGrammarParser.StringContext context)
        {
            return (Type.String, context.STRING().GetText().Trim('"'));
        }

        public override (Type Type, object Value) VisitBool(ProjectGrammarParser.BoolContext context)
        {
            return (Type.Boolean, bool.Parse(context.BOOL().GetText()));
        }

        public override (Type Type, object Value) VisitId(ProjectGrammarParser.IdContext context)
        {
            return symbols[context.ID().Symbol];
        }

        public override (Type Type, object Value) VisitWhileStat(ProjectGrammarParser.WhileStatContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            while ((bool)expr.Value)
            {
                Visit(context.stat());
                expr = Visit(context.expr());
            } 
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitWhileBlock(ProjectGrammarParser.WhileBlockContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            while ((bool)expr.Value)
            {
                Visit(context.block());
                expr = Visit(context.expr());
            } 
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitDoWhileStat(ProjectGrammarParser.DoWhileStatContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            do
            {
                Visit(context.stat());
                
                expr = Visit(context.expr());
            } while ((bool)expr.Value);

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitDoWhileBlock(ProjectGrammarParser.DoWhileBlockContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }
            
            do
            {
                Visit(context.block());
                
                expr = Visit(context.expr());
            } while ((bool)expr.Value);
            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitProg(ProjectGrammarParser.ProgContext context)
        {
            foreach (var statement in context.stat())
            {
                var value = Visit(statement);
                if (value.Type != Type.Error) Console.WriteLine(value.Value);
                else
                    Errors.PrintAndClearErrors();
            }

            return (Type.Error, 0);
        }
        
        public override (Type Type, object Value) VisitVariableType(ProjectGrammarParser.VariableTypeContext context)
        {
            switch (context.type.Text.Trim())
            {
                case "int": return (Type.Int, 0);
                case "float": return (Type.Float, (float)0);
                case "bool": return (Type.Boolean, false);
                case "string": return (Type.String, "");
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitRead(ProjectGrammarParser.ReadContext context)
        {
            foreach (var id in context.ID())
            {
                var variable = symbols[id.Symbol];
                if (variable.Type == Type.Error)
                {
                    Errors.ReportError(context.start, $"Could not read variable {variable}. Variable not found.");
                }
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitWrite(ProjectGrammarParser.WriteContext context)
        {
            foreach (var expr in context.expr())
            {
                var value = Visit(expr);
                if (value.Type != Type.Error)
                    Console.Write(value.Value);
            }
            Console.WriteLine();

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitIfStat(ProjectGrammarParser.IfStatContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            if ((bool)expr.Value)
            {
                return Visit(context.stat());
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitIfStatElseStat(ProjectGrammarParser.IfStatElseStatContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            if ((bool)expr.Value)
            {
                return Visit(context.stat()[0]);
            }

            return Visit(context.stat()[1]);
        }

        public override (Type Type, object Value) VisitIfStatElseBlock(ProjectGrammarParser.IfStatElseBlockContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }
            
            if ((bool)expr.Value)
            {
                return Visit(context.stat());
            }

            return Visit(context.block());
        }

        public override (Type Type, object Value) VisitIfBlock(ProjectGrammarParser.IfBlockContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            if ((bool)expr.Value)
            {
                return Visit(context.block());
            }

            return (Type.Error, 0);
        }

        public override (Type Type, object Value) VisitIfBlockElseStat(ProjectGrammarParser.IfBlockElseStatContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }
            
            if ((bool)expr.Value)
            {
                return Visit(context.block());
            }

            return Visit(context.stat());
        }

        public override (Type Type, object Value) VisitIfBlockElseBlock(ProjectGrammarParser.IfBlockElseBlockContext context)
        {
            var expr = Visit(context.expr());
            if (expr.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr().start, $"Condition expression expected boolean, got {expr.Type}.");
                return (Type.Error, 0);
            }

            if ((bool)expr.Value)
            {
                return Visit(context.block()[0]);
            }

            return Visit(context.block()[1]);
        }

        public override (Type Type, object Value) VisitTernaryCondCond(ProjectGrammarParser.TernaryCondCondContext context)
        {
            var exprCond = Visit(context.expr()[0]);
            var exprTrue = Visit(context.expr()[1]);
            var exprFalse = Visit(context.expr()[2]);
        
            if (exprCond.Type != Type.Boolean)
            {
                Errors.ReportError(context.expr()[0].start, $"Ternary operator expected boolean, got {exprCond.Type}.");
                return (Type.Error, 0);
            }

            if ((exprTrue.Type == Type.Int && exprFalse.Type == Type.Float) ||
                (exprTrue.Type == Type.Float && exprFalse.Type == Type.Int))
            {
                var value =  (bool)exprCond.Value ? ToFloat(exprTrue.Value) : ToFloat(exprFalse.Value);
                return (Type.Int, value);
            }
            
            if (exprTrue.Type != exprFalse.Type)
            {
                Errors.ReportError(context.expr()[0].start, $"Return types must match, got {exprTrue.Type} and {exprFalse.Type}.");
                return (Type.Error, 0);
            }

            return (bool)exprCond.Value ? exprTrue : exprFalse;
        }
    }
}