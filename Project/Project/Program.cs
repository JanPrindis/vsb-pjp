﻿using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace Project
{
    public static class Program 
    {
        public static void Main(string[] args) 
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            string[] tests = { "test1.in", "test2.in", "test3.in", "testErr.in", "testMy.in" };
            //string[] tests = { "testMy.in" };

            foreach (var file in tests)
            {
                string inputFile = File.ReadAllText($"InputFiles/{file}");
                
                Console.WriteLine("########################################");
                Console.WriteLine($"Currently processing file: {file}");
            
                AntlrInputStream input = new AntlrInputStream(inputFile);
                ProjectGrammarLexer lexer = new ProjectGrammarLexer(input);
            
                CommonTokenStream tokens = new CommonTokenStream(lexer);
                ProjectGrammarParser parser = new ProjectGrammarParser(tokens);
                parser.AddErrorListener(new VerboseListener());
                IParseTree tree = parser.prog();

                if (parser.NumberOfSyntaxErrors > 0)
                {
                    Console.WriteLine("error count: {0}", parser.NumberOfSyntaxErrors);
                    continue;
                }

                var visitor = new TreeVisitor();
                visitor.Visit(tree);

                if (Errors.NumberOfErrors > 0)
                {
                    Errors.PrintAndClearErrors();
                    continue;
                }
                
                //visitor.DumpToFile(file + "-output.txt");

                VirtualMachine vm = new VirtualMachine(visitor.GetCodeString());
                vm.Run();
            }
        }
    }
}