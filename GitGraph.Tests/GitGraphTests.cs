using System.Linq;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class GitGraphTests
    {
	    [Test]
	    public void TestCommitCount()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetRepository().Commits;
			Assert.That(commits.Count, Is.EqualTo(7));
		}

	    [Test]
	    public void TestChildCommits()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetRepository().Commits;

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
			var commits = new GraphProcessor(new MockGit()).GetRepository().CommitsById;
			Assert.That(commits[7].Branches, Is.EqualTo(new[] {"master"}));
			Assert.That(commits[6].Branches, Is.EqualTo(new[] {"other-branch"}));
		}

	    [Test]
	    public void TestTags()
	    {
		    var commits = new GraphProcessor(new MockGit()).GetRepository().CommitsById;
		    Assert.That(commits[5].Tags, Is.EqualTo(new[] {"merged"}));
		    Assert.That(commits[1].Tags, Is.EqualTo(new[] {"initial"}));
	    }

		[Test]
	    public void TestCommitDepth()
	    {
		    int GetMaxDepth(Commit commit) => 1 + commit.ChildCommits
				.Select(GetMaxDepth)
				.DefaultIfEmpty()
				.Max();

		    var commits = new GraphProcessor(new MockGit()).GetRepository().CommitsById;
		    Assert.That(GetMaxDepth(commits[1]), Is.EqualTo(5));
	    }
	}
}

