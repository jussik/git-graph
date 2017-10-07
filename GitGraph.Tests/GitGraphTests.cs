using System;
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
		    var commits = new RepositoryImporter(new MockGit()).GetRepository().CommitsById;
			Assert.That(commits.Values.Count, Is.EqualTo(7));
		}

	    [Test]
	    public void TestParentCommits()
	    {
		    var repo = new RepositoryImporter(new MockGit()).GetRepository();

		    var c1 = repo.FindCommit("356a192b");
		    var c2 = repo.FindCommit("da4b9237");
		    var c3 = repo.FindCommit("77de68da");
		    var c4 = repo.FindCommit("1b645389");
		    var c5 = repo.FindCommit("ac3478d6");

			Assert.That(
			    c1.Parent,
			    Is.Null);
		    Assert.That(
			    c1.MergeParent,
			    Is.Null);

			Assert.That(
			    c5.Parent,
			    Is.EqualTo(c4));
		    Assert.That(
			    c5.MergeParent,
			    Is.EqualTo(c3));

		    Assert.That(
			    c2.Parent,
			    Is.EqualTo(c1));
		    Assert.That(
			    c2.MergeParent,
			    Is.Null);
		}

		[Test]
		public void TestBranches()
		{
			var repo = new RepositoryImporter(new MockGit()).GetRepository();

			var master = repo.Refs.FirstOrDefault(r => r.Name == "master");
			Assert.That(master, Is.Not.Null);
			Assert.That(master.Type, Is.EqualTo(Ref.RefType.Branch));
			Assert.That(master.Commit, Is.EqualTo(repo.FindCommit("902ba3cd")));

			var other = repo.Refs.FirstOrDefault(r => r.Name == "other-branch");
			Assert.That(other, Is.Not.Null);
			Assert.That(other.Type, Is.EqualTo(Ref.RefType.Branch));
			Assert.That(other.Commit, Is.EqualTo(repo.FindCommit("c1dfd96e")));
		}

	    [Test]
	    public void TestTags()
		{
			var repo = new RepositoryImporter(new MockGit()).GetRepository();

			var merged = repo.Refs.FirstOrDefault(r => r.Name == "merged");
			Assert.That(merged, Is.Not.Null);
			Assert.That(merged.Type, Is.EqualTo(Ref.RefType.Tag));
			Assert.That(merged.Commit, Is.EqualTo(repo.FindCommit("ac3478d6")));

			var initial = repo.Refs.FirstOrDefault(r => r.Name == "initial");
			Assert.That(initial, Is.Not.Null);
			Assert.That(initial.Type, Is.EqualTo(Ref.RefType.Tag));
			Assert.That(initial.Commit, Is.EqualTo(repo.FindCommit("356a192b")));
	    }

		[Test]
	    public void TestCommitDepth()
		{
			int GetDepth(Commit commit) => commit != null ? 1 + Math.Max(GetDepth(commit.Parent), GetDepth(commit.MergeParent)) : 0;

		    var repo = new RepositoryImporter(new MockGit()).GetRepository();
		    Assert.That(repo.Refs.Max(r => GetDepth(r.Commit)), Is.EqualTo(5));
	    }
	}
}

