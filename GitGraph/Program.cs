using System;
using GitGraph.Input;
using GitGraph.Optimisation;
using GitGraph.Output;

namespace GitGraph
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var dir = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
			var repo = new RepositoryImporter(new Git(dir)).GetRepository();
			var repo2 = GraphOptimiser.GetOptimised(repo.Refs);
			DotFormatter.ToDigraph(repo2.Refs, Console.Out);
		}
	}
}
