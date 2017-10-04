using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GitGraph.Tests
{
	public class MockGit : IGit
	{
		public IEnumerable<string> GetCommits()
		{
			return Split(@"
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
5 main-branch
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

	[TestFixture]
    public class GitGraphTests
    {
	    [Test]
	    public void TestCommitCount()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetCommits();
			Assert.That(commits.Count, Is.EqualTo(6));
		}

	    [Test]
	    public void TestChildCommits()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetCommits();

		    Assert.That(
			    commits[1].ChildCommits.Select(c => (int) c.Id),
			    Is.EquivalentTo(new[] {2}));

		    Assert.That(
			    commits[3].ChildCommits.Select(c => (int) c.Id),
			    Is.EquivalentTo(new[] {5, 6}));

		    Assert.That(
			    commits[6].ChildCommits,
			    Is.Empty);
		}

		[Test]
		public void TestBranches()
		{
			var commits = new GraphProcessor(new MockGit()).GetCommits();
			Assert.That(commits[5].Branches, Is.EqualTo(new[] {"main-branch"}));
			Assert.That(commits[6].Branches, Is.EqualTo(new[] {"other-branch"}));
		}

	    [Test]
	    public void TestTags()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetCommits();
		    Assert.That(commits[5].Tags, Is.EqualTo(new[] {"merged"}));
		    Assert.That(commits[1].Tags, Is.EqualTo(new[] {"initial"}));
	    }

		[Test]
	    public void TestCommitDepth()
	    {
		    int GetMaxDepth(Commit commit) => 1 + commit.ChildCommits
				.Select(GetMaxDepth)
				.DefaultIfEmpty(1)
				.Max();

		    var commits = new GraphProcessor(new MockGit()).GetCommits();
		    Assert.That(GetMaxDepth(commits[1]), Is.EqualTo(5));
	    }
	}
}

