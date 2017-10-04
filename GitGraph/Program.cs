namespace GitGraph
{
	public static class Program
	{
		public static void Main()
		{
			var processor = new GraphProcessor(new Git());
			var commits = processor.GetCommits();
		}
	}
}
