using System;
using Grammar;
using System.Collections.Generic;
using System.Linq;

namespace Lab3
{
	public class GrammarOps
	{
		public GrammarOps(IGrammar g)
		{
			this.g = g;
			compute_empty();
			compute_first();
			compute_follow();
		}

		public ISet<Nonterminal> EmptyNonterminals { get; } = new HashSet<Nonterminal>();
		private void compute_empty()
		{
			foreach (var gRule in this.g.Rules)
			{
				if (gRule.RHS.Count == 0) EmptyNonterminals.Add(gRule.LHS);
			}

			int count = 0;
			do
			{
				count = EmptyNonterminals.Count;
				foreach (var gRule in this.g.Rules)
				{
					if (!gRule.RHS.All(x => x is Nonterminal && EmptyNonterminals.Contains(x))) continue;
					EmptyNonterminals.Add(gRule.LHS);
				}
			} while (count != EmptyNonterminals.Count);
		}

		public IGrammar g { get; }

		private bool[,] table_first { get; set; }

		private bool[,] table_follow { get; set; }
		
		private void compute_first()
		{
			var terminals = g.Terminals.OrderBy(x => x.Name);
			var nonterminals = g.Nonterminals.OrderBy(x => x.Name);
			
			int rows = g.Nonterminals.Count;
			int cols = rows + g.Terminals.Count;

			Terminal empty = new Terminal("{e}");

			bool[,] table = new bool[rows, cols];
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					table[i, j] = false;
				}
			}

			foreach (var gRule in this.g.Rules)
			{
				int currentRow = Array.FindIndex<Nonterminal>(nonterminals.ToArray(), obj => obj.Name == gRule.LHS.Name);
				foreach (var symbol in gRule.RHS)
				{
					if (symbol is Terminal)
					{
						int index = Array.FindIndex<Terminal>(terminals.ToArray(), obj => obj.Name == symbol.Name);
						int currentCol = rows + index;

						table[currentRow, currentCol] = true;
						break;
					}
					
					if (symbol is Nonterminal)
					{
						int currentCol = Array.FindIndex<Nonterminal>(nonterminals.ToArray(), obj => obj.Name == symbol.Name);
						
						table[currentRow, currentCol] = true;
						
						if (!EmptyNonterminals.Contains(symbol)) break;
					}
					else throw new GrammarException("Unexpected Type", -1); // Idk line number
				}
			}

			// Copy rows
			for (int currentRow = 0; currentRow < rows; currentRow++)
			{
				for (int copyRow = 0; copyRow < rows; copyRow++)
				{
					// If row needs to be copied
					if (table[currentRow, copyRow])
					{
						for (int col = 0; col < cols; col++)
						{
							table[currentRow, col] |= table[copyRow, col];
						} 
					}
				}
			}
			
			// Add to Nonterminals
			for (int row = 0; row < rows; row++)
			{
				for (int i = 0; i < nonterminals.Count(); i++)
				{
					if (table[row, i + rows])
						nonterminals.ToArray()[row].first.Add(terminals.ToArray()[i]);
				}
			}
			
			// Add to rules
			foreach (var gRule in g.Rules)
			{
				foreach (var symbol in gRule.RHS)
				{
					if (symbol is Terminal)
					{
						if (!gRule.First.Contains(symbol))
							gRule.First.Add(symbol);
						else
							g.IsLL1 = false;
						break;
					}
					
					if (symbol is Nonterminal)
					{
						int currentRow = Array.FindIndex<Nonterminal>(nonterminals.ToArray(), obj => obj.Name == symbol.Name);

						for (int i = 0; i < g.Terminals.Count; i++)
						{
							if (table[currentRow, i + rows])
							{
								if(!gRule.First.Contains(terminals.ToArray()[i]))
									gRule.First.Add(terminals.ToArray()[i]);
								else
									g.IsLL1 = false;
							}
						}
						
						if (!EmptyNonterminals.Contains(symbol)) break;
					}
					
					if (symbol == gRule.RHS.Last() && EmptyNonterminals.Contains(symbol))
					{	
						if(!gRule.First.Contains(empty))
							gRule.First.Add(empty);
					} 
				}
				
				if (gRule.First.Count == 0)
				{
					gRule.First.Add(empty);
				}
			}

			table_first = table;
			
			//Debug Print
			// for (int i = 0; i < rows; i++)
			// {
			// 	for (int j = 0; j < cols; j++)
			// 	{
			// 		Console.Write(table[i, j] ? "* " : "_ ");
			// 	}
			// 	Console.Write("\n");
			// }
		}
		
		private void compute_follow()
		{
			var terminals = g.Terminals.OrderBy(x => x.Name);
			var nonterminals = g.Nonterminals.OrderBy(x => x.Name);
			
			int rows = g.Nonterminals.Count;
			int cols = rows + g.Terminals.Count + 1;

			Terminal endOfStack = new Terminal("$");

			bool[,] table = new bool[rows, cols];
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					table[i, j] = false;
				}
			}
			
			// Set starting epsilon
			int startingIndex = Array.FindIndex(nonterminals.ToArray(), x => x == g.StartingNonterminal);

			table[startingIndex, cols - 1] = true;

			foreach (var gRule in g.Rules)
			{
				var sIndex = 0;
				
				foreach (var symbol in gRule.RHS)
				{
					if (symbol is Nonterminal)
					{
						int rowIndex = Array.FindIndex(nonterminals.ToArray(), x => x == symbol);

						if (sIndex < gRule.RHS.Count - 1)
						{
							var alpha = gRule.RHS[sIndex + 1];
							if (alpha is Terminal)
							{
								// alpha is Follow
								int colIndex = Array.FindIndex(terminals.ToArray(), x => x == alpha);
								table[rowIndex, colIndex + rows] = true;
							}
							if (alpha is Nonterminal)
							{
								// First of alpha is Follow
								int rowIndexFirst = Array.FindIndex(nonterminals.ToArray(), x => x == alpha);
								for (int i = 0; i < terminals.Count(); i++)
								{
									table[rowIndex, i + rows] |= table_first[rowIndex, i + rows];
								}
							}
						}
						else
						{
							// no alpha, symbol is last
							int colIndex = Array.FindIndex(nonterminals.ToArray(), x => x == gRule.LHS);
							table[rowIndex, colIndex] = true;
						}
					}

					sIndex++;
				}
			}
			
			// Copy rows
			for (int currentRow = 0; currentRow < rows; currentRow++)
			{
				for (int copyRow = 0; copyRow < rows; copyRow++)
				{
					// If row needs to be copied
					if (table[currentRow, copyRow])
					{
						for (int col = 0; col < cols; col++)
						{
							table[currentRow, col] |= table[copyRow, col];
						} 
					}
				}
			}

			// Add to Nonterminals
			for (int row = 0; row < rows; row++)
			{
				for (int i = 0; i < terminals.Count() + 1; i++)
				{
					if (table[row, i + rows])
					{
						if (i == terminals.Count())
							nonterminals.ToArray()[row].follow.Add(endOfStack);
						else
							nonterminals.ToArray()[row].follow.Add(terminals.ToArray()[i]);
					}
				}
			}

			table_follow = table;

			// Debug Print
			// for (int i = 0; i < rows; i++)
			// {
			// 	for (int j = 0; j < cols; j++)
			// 	{
			// 		Console.Write(table[i, j] ? "* " : "_ ");
			// 	}
			// 	Console.Write("\n");
			// }
		}
	}
}
