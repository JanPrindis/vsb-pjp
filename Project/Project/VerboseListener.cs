using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime;

namespace Project
{
    public class VerboseListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, int line, int charPositionInLine, string msg,
            RecognitionException e)
        {
            IList<string> stack = ((Parser)recognizer).GetRuleInvocationStack();
            stack.Reverse();
            
            Console.Error.WriteLine("Rule stack: " + String.Join(", ", stack));
            Console.Error.WriteLine("Line " + line + ":" + charPositionInLine + " at " + offendingSymbol + ": " + msg);
        }
    }
}