using System;
using System.Collections.Generic;
using Antlr4.Runtime;

namespace Project
{
    public class Errors
    {
        private static readonly List<string> ErrorsList = new List<string>();

        public static void ReportError(IToken token, string message)
        {
            ErrorsList.Add($"{token.Line}:{token.Column} - {message}");
        }
        
        public static int NumberOfErrors => ErrorsList.Count;

        public static void PrintAndClearErrors()
        {
            foreach (var error in ErrorsList)
            {
                Console.WriteLine(error);
            }
            ErrorsList.Clear();
        }
    }
}