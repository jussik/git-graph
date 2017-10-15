using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GitGraph.Optimisation;
using NUnit.Framework;

namespace GitGraph.Tests
{
	public class GrafterTests
	{
		[Test]
		public void TestChainGraft()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit middle = builder.AddCommit(root);
			Commit head = builder.AddCommit(middle);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id)?.Commits[1], Is.EqualTo(root));
		}

		[Test]
		public void TestIdentityChainNoGraft()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit head = builder.AddCommit(root);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Any(), Is.EqualTo(false));
		}

		[Test]
		public void TestSingleCommitChainNoGraft()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit head = builder.AddCommit();
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Any(), Is.EqualTo(false));
		}

		[Test]
		public void TestParallelChainGrafts()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit a1 = builder.AddCommit(root);
			Commit a2 = builder.AddCommit(a1);
			Commit b1 = builder.AddCommit(root);
			Commit b2 = builder.AddCommit(b1);
			Commit head = builder.AddCommit(a2, b2);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(2));
			Assert.That(grafts.GetValueOrDefault(a2.Id)?.Parent, Is.EqualTo(root));
			Assert.That(grafts.GetValueOrDefault(b2.Id)?.Parent, Is.EqualTo(root));
		}

		[Test]
		public void TestSplitChainGrafts()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit preSplit = builder.AddCommit(root);
			Commit a1 = builder.AddCommit(preSplit);
			Commit a2 = builder.AddCommit(a1);
			Commit b1 = builder.AddCommit(preSplit);
			Commit b2 = builder.AddCommit(b1);
			builder.AddBranch("a", a2);
			builder.AddBranch("b", b2);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(2));
			Assert.That(grafts.GetValueOrDefault(a2.Id)?.Parent, Is.EqualTo(root));
			Assert.That(grafts.GetValueOrDefault(b2.Id)?.Parent, Is.EqualTo(root));
		}

		[Test]
		public void TestPostMergeChainGrafts()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit branch1 = builder.AddCommit(root);
			Commit branch2 = builder.AddCommit(root);
			Commit postMerge = builder.AddCommit(branch1, branch2);
			Commit middle = builder.AddCommit(postMerge);
			Commit head = builder.AddCommit(middle);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id)?.Parent, Is.EqualTo(postMerge));
		}

		[Test]
		public void TestChainGraftWhitelist()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit preTag = builder.AddCommit(root);
			Commit tagged = builder.AddCommit(preTag);
			Commit postTag = builder.AddCommit(tagged);
			Commit head = builder.AddCommit(postTag);
			builder.AddBranch("master", head);
			builder.AddTag("tag", tagged);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftChains().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(2));
			Assert.That(grafts.GetValueOrDefault(head.Id)?.Parent, Is.EqualTo(tagged));
			Assert.That(grafts.GetValueOrDefault(tagged.Id)?.Parent, Is.EqualTo(root));
		}

		[Test]
		public void TestLoopGraft()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit branch1 = builder.AddCommit(root);
			Commit branch2 = builder.AddCommit(root);
			Commit head = builder.AddCommit(branch1, branch2);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftLoops().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id)?.Parent, Is.EqualTo(root));
		}

		[Test]
		public void TestMultiLoopGraft()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit branch1 = builder.AddCommit(root);
			Commit branch2 = builder.AddCommit(root);
			Commit branch3 = builder.AddCommit(branch2);
			Commit merge = builder.AddCommit(branch1, branch2);
			Commit head = builder.AddCommit(merge, branch3);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Graft> grafts = new Grafter(repo.Refs.All.Select(r => r.Commit))
				.GraftLoops().Grafts;
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id)?.Parent, Is.EqualTo(root));
		}

		[Test]
		public void ApplyGraftTestParent()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit middle = builder.AddCommit(root);
			Commit head = builder.AddCommit(middle);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			var grafter = new Grafter(repo.Refs.All.Select(r => r.Commit));
			grafter.Grafts.Add(head.Id, new Graft(head, root));
			var newRepo = grafter.GetCommitMap(repo.Commits);

			Commit newHead = newRepo.GetValueOrDefault(head.Id);
			Assert.That(newHead, Is.EqualTo(head));
			Assert.That(newHead.Parent, Is.EqualTo(root));
		}
	}
}