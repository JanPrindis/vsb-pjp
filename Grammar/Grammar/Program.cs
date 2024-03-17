using Grammar;
using System;
using System.IO;

namespace Lab3
{

	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				StreamReader r = new StreamReader(new FileStream("G1.TXT", FileMode.Open));
				//StreamReader r = new StreamReader(new FileStream("C:/Users/janpr/PJP/Grammar/Grammar/G2.TXT", FileMode.Open));

				GrammarReader inp = new GrammarReader(r);
				var grammar = inp.Read();
				grammar.dump();

				GrammarOps gr = new GrammarOps(grammar);

				// First step, computes nonterminals that can be rewritten as empty word
				foreach (Nonterminal nt in gr.EmptyNonterminals)
				{
					Console.Write(nt.Name + " ");
				}
				Console.WriteLine();

				// Print Fisrt
				foreach (var rule in gr.g.Rules)
				{
					Console.Write("first[" + rule.LHS.Name + ":");
					foreach (var symbol in rule.RHS)
						Console.Write(symbol.Name);

					if (rule.RHS.Count == 0)
						Console.Write("{e}");
					
					Console.Write("] = ");
					
					foreach (var first in rule.First)
						Console.Write(first.Name + " ");
					
					Console.Write("\n");
				}
				
				// Print Follow
				foreach (var nonterminal in gr.g.Nonterminals)
				{
					Console.Write("follow[" + nonterminal.Name + "] = ");
					foreach (var follow in nonterminal.follow)
						Console.Write(follow.Name + " ");
					Console.Write("\n");
				}
				
				// Print if LL1
				Console.WriteLine(gr.g.IsLL1 ? "Grammar is LL1" : "Grammar is not LL1"); 
			}
			catch (GrammarException e)
			{
				Console.WriteLine($"{e.LineNumber}: Error -  {e.Message}");
			}
			catch (IOException e)
			{
				Console.WriteLine(e);
			}
		}
	}
}