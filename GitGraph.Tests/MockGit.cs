using System;
using System.Collections.Generic;

namespace GitGraph.Tests
{
	public class MockGit : IGit
	{
		public IEnumerable<string> GetCommits()
		{
			return Split(@"
902ba3cda1883801594b6e1b452790cc53948fda ac3478d69a3c81fa62e60f5c3696165a4e5e6ac4
c1dfd96eea8cc2b62785275bca38ac261256e278 77de68daecd823babbb58edb1c8e14d7106e83bb
ac3478d69a3c81fa62e60f5c3696165a4e5e6ac4 1b6453892473a467d07372d45eb05abc2031647a 77de68daecd823babbb58edb1c8e14d7106e83bb
1b6453892473a467d07372d45eb05abc2031647a da4b9237bacccdf19c0760cab7aec4a8359010b0
77de68daecd823babbb58edb1c8e14d7106e83bb da4b9237bacccdf19c0760cab7aec4a8359010b0
da4b9237bacccdf19c0760cab7aec4a8359010b0 356a192b7913b04c54574d18c28d46e6395428ab
356a192b7913b04c54574d18c28d46e6395428ab
");
		}

		public IEnumerable<string> GetBranches()
		{
			return Split(@"
902ba3cda1883801594b6e1b452790cc53948fda master
c1dfd96eea8cc2b62785275bca38ac261256e278 other-branch
");
		}

		public IEnumerable<string> GetTags()
		{
			return Split(@"
ac3478d69a3c81fa62e60f5c3696165a4e5e6ac4 merged
356a192b7913b04c54574d18c28d46e6395428ab initial
");
		}

		private static readonly char[] Newline = {'\n', '\r'};
		private static IEnumerable<string> Split(string str) =>
			str.Trim().Split(Newline, StringSplitOptions.RemoveEmptyEntries);
	}
}