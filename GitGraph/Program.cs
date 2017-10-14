using System;
using GitGraph.Input;
using GitGraph.Output;

namespace GitGraph
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var dir = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
			var repo = new RepositoryImporter(new Git(dir)).GetRepository();
			DotFormatter.ToDigraph(repo.Refs, Console.Out);
		}
	}
}
