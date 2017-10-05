using System;
using System.Threading.Tasks;

namespace GitGraph
{
	public static class Program
	{
		public static async Task Main(string[] args)
		{
			var dir = args.Length > 0 ? args[0] : Environment.CurrentDirectory;
			var repo = new GraphProcessor(new Git(dir)).GetRepository();
			await DotFormatter.ToDigraphAsync(repo, Console.Out, true);
		}
	}
}
