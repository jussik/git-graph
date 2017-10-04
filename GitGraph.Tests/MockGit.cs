using System;
using System.Collections.Generic;

namespace GitGraph.Tests
{
	public class MockGit : IGit
	{
		public IEnumerable<string> GetCommits()
		{
			return Split(@"
7 5
6 3
5 4 3
4 2
3 2
2 1
1
");
		}

		public IEnumerable<string> GetBranches()
		{
			return Split(@"
7 master
6 other-branch
");
		}

		public IEnumerable<string> GetTags()
		{
			return Split(@"
5 merged
1 initial
");
		}

		private static readonly char[] Newline = {'\n', '\r'};
		private static IEnumerable<string> Split(string str) =>
			str.Trim().Split(Newline, StringSplitOptions.RemoveEmptyEntries);
	}
}