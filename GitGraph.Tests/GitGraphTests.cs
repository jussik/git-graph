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
		    var commits = new RepositoryImporter(new MockGit()).GetRepository().CommitsById;

		    Assert.That(
			    commits[1].Parent,
			    Is.Null);
		    Assert.That(
			    commits[1].MergeParent,
			    Is.Null);

			Assert.That(
			    commits[5].Parent,
			    Is.EqualTo(commits[4]));
		    Assert.That(
			    commits[5].MergeParent,
			    Is.EqualTo(commits[3]));

		    Assert.That(
			    commits[2].Parent,
			    Is.EqualTo(commits[1]));
		    Assert.That(
			    commits[2].MergeParent,
			    Is.Null);
		}

		[Test]
		public void TestBranches()
		{
			var repo = new RepositoryImporter(new MockGit()).GetRepository();

			var master = repo.Refs.FirstOrDefault(r => r.Name == "master");
			Assert.That(master, Is.Not.Null);
			Assert.That(master.Type, Is.EqualTo(Ref.RefType.Branch));
			Assert.That((int)master.Commit.Id, Is.EqualTo(7));

			var other = repo.Refs.FirstOrDefault(r => r.Name == "other-branch");
			Assert.That(other, Is.Not.Null);
			Assert.That(other.Type, Is.EqualTo(Ref.RefType.Branch));
			Assert.That((int)other.Commit.Id, Is.EqualTo(6));
		}

	    [Test]
	    public void TestTags()
		{
			var repo = new RepositoryImporter(new MockGit()).GetRepository();

			var merged = repo.Refs.FirstOrDefault(r => r.Name == "merged");
			Assert.That(merged, Is.Not.Null);
			Assert.That(merged.Type, Is.EqualTo(Ref.RefType.Tag));
			Assert.That((int)merged.Commit.Id, Is.EqualTo(5));

			var initial = repo.Refs.FirstOrDefault(r => r.Name == "initial");
			Assert.That(initial, Is.Not.Null);
			Assert.That(initial.Type, Is.EqualTo(Ref.RefType.Tag));
			Assert.That((int)initial.Commit.Id, Is.EqualTo(1));
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

