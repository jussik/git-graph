﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class GraphOptimiserTests
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

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id), Is.EqualTo(root.Id));
		}

	    [Test]
	    public void TestIdentityChainNoGraft()
	    {
		    RepositoryBuilder builder = new RepositoryBuilder();
		    Commit root = builder.AddCommit();
		    Commit head = builder.AddCommit(root);
		    builder.AddBranch("master", head);
		    Repository repo = builder.BuildRepository();

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit));
		    Assert.That(grafts.Any(), Is.EqualTo(false));
		}

	    [Test]
	    public void TestSingleCommitChainNoGraft()
	    {
		    RepositoryBuilder builder = new RepositoryBuilder();
		    Commit head = builder.AddCommit();
		    builder.AddBranch("master", head);
		    Repository repo = builder.BuildRepository();

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit));
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

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(2));
		    Assert.That(grafts.GetValueOrDefault(a2.Id), Is.EqualTo(root.Id));
		    Assert.That(grafts.GetValueOrDefault(b2.Id), Is.EqualTo(root.Id));
		}

	    [Test]
	    public void TestSplitChainGrafts()
	    {
		    RepositoryBuilder builder = new RepositoryBuilder();
		    Commit root = builder.AddCommit();
		    Commit a1 = builder.AddCommit(root);
		    Commit a2 = builder.AddCommit(a1);
		    Commit b1 = builder.AddCommit(root);
		    Commit b2 = builder.AddCommit(b1);
		    builder.AddBranch("a", a2);
		    builder.AddBranch("b", b2);
		    Repository repo = builder.BuildRepository();

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(2));
		    Assert.That(grafts.GetValueOrDefault(a2.Id), Is.EqualTo(root.Id));
		    Assert.That(grafts.GetValueOrDefault(b2.Id), Is.EqualTo(root.Id));
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

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(1));
		    Assert.That(grafts.GetValueOrDefault(head.Id), Is.EqualTo(postMerge.Id));
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

		    var grafts = GraphOptimiser.GetChainGrafts(repo.Refs.All.Select(r => r.Commit))
			    .ToDictionary(g => g.Ids[0], g => g.Ids[1]);
		    Assert.That(grafts.Count, Is.EqualTo(2));
		    Assert.That(grafts.GetValueOrDefault(head.Id), Is.EqualTo(tagged.Id));
		    Assert.That(grafts.GetValueOrDefault(tagged.Id), Is.EqualTo(root.Id));
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

			var grafts = GraphOptimiser.GetLoopGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(1));
			Assert.That(grafts.GetValueOrDefault(head.Id), Is.EqualTo(root.Id));
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

		    var grafts = GraphOptimiser.GetLoopGrafts(repo.Refs.All.Select(r => r.Commit))
				.ToDictionary(g => g.Ids[0], g => g.Ids[1]);
			Assert.That(grafts.Count, Is.EqualTo(1));
		    Assert.That(grafts.GetValueOrDefault(head.Id), Is.EqualTo(root.Id));
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

		    var grafts = new[] {new GraphOptimiser.Graft(head.Id, root.Id)};
		    var newRepo = GraphOptimiser.GetGraftedCommits(repo.Commits, grafts);

		    Commit newHead = newRepo.GetValueOrDefault(head.Id);
		    Assert.That(newHead, Is.EqualTo(head));
			Assert.That(newHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestSimplePrune()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
			Commit toPrune = builder.AddCommit(root);
			Commit head = builder.AddCommit(root);
			builder.AddBranch("master", head);
			Repository repo = builder.BuildRepository();

			Dictionary<BigInteger, Commit> map = repo.Commits.ToDictionary(c => c.Id);

			Assert.That(map.Count, Is.EqualTo(3));
			GraphOptimiser.PruneCommits(map, new[] {head});
			Assert.That(map.Count, Is.EqualTo(2));

			Assert.That(map, Contains.Key(root.Id));
			Assert.That(map, Contains.Key(head.Id));
			Assert.That(map, Does.Not.ContainKey(toPrune.Id));
		}

		[Test]
	    public void TestCollapseMergeLoopWithChains()
	    {
			RepositoryBuilder builder = new RepositoryBuilder();
		    Commit root = builder.AddCommit();
			Commit preBranch = builder.AddCommit(root);
		    Commit branch1 = builder.AddCommit(preBranch);
			Commit chain1 = builder.AddCommit(branch1);
		    Commit branch2 = builder.AddCommit(preBranch);
			Commit chain2 = builder.AddCommit(branch2);
		    Commit merge = builder.AddCommit(chain1, chain2);
		    Commit head = builder.AddCommit(merge);
		    builder.AddBranch("master", head);
		    Repository repo = builder.BuildRepository();

			Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedHead = optimised.Refs.All[0].Commit;
		    Assert.That(optimisedHead, Is.EqualTo(head));
		    Assert.That(optimisedHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestCollapseMergeDoubleLoop()
	    {
			RepositoryBuilder builder = new RepositoryBuilder();
		    Commit root = builder.AddCommit();
		    Commit preBranch = builder.AddCommit(root);
		    Commit branch1 = builder.AddCommit(preBranch);
		    Commit branch2 = builder.AddCommit(preBranch);
		    Commit merge1 = builder.AddCommit(branch1, branch2);
		    Commit branch3 = builder.AddCommit(branch2);
		    Commit merge2 = builder.AddCommit(merge1, branch3);
		    Commit head = builder.AddCommit(merge2);
		    builder.AddBranch("master", head);
		    Repository repo = builder.BuildRepository();

			Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedHead = optimised.Refs.All[0].Commit;
		    Assert.That(optimisedHead, Is.EqualTo(head));
		    Assert.That(optimisedHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestCollapseSplitHeads()
	    {
			RepositoryBuilder builder = new RepositoryBuilder();
		    Commit root = builder.AddCommit();
		    Commit middle = builder.AddCommit(root);
		    Commit master = builder.AddCommit(middle);
		    Commit feature = builder.AddCommit(middle);
		    builder.AddBranch("master", master);
		    builder.AddBranch("feature", feature);
		    Repository repo = builder.BuildRepository();

		    Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedMaster = optimised.Refs.ByName("master").Commit;
		    Assert.That(optimisedMaster, Is.EqualTo(master));
		    Assert.That(optimisedMaster.Parent, Is.EqualTo(middle));

		    Commit optimisedFeature = optimised.Refs.ByName("feature").Commit;
		    Assert.That(optimisedFeature, Is.EqualTo(feature));
		    Assert.That(optimisedFeature.Parent, Is.EqualTo(middle));
		}

	    [Test]
	    public void TestCollapseSplitHeadsWithChains()
		{
			RepositoryBuilder builder = new RepositoryBuilder();
			Commit root = builder.AddCommit();
		    Commit preBranch = builder.AddCommit(root);
		    Commit masterMid = builder.AddCommit(preBranch);
		    Commit featureMid = builder.AddCommit(preBranch);
		    Commit master = builder.AddCommit(masterMid);
		    Commit feature = builder.AddCommit(featureMid);
		    builder.AddBranch("master", master);
		    builder.AddBranch("feature", feature);
		    Repository repo = builder.BuildRepository();

			Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedMaster = optimised.Refs.ByName("master").Commit;
		    Assert.That(optimisedMaster, Is.EqualTo(master));
		    Assert.That(optimisedMaster.Parent, Is.EqualTo(preBranch));

		    Commit optimisedFeature = optimised.Refs.ByName("feature").Commit;
		    Assert.That(optimisedFeature, Is.EqualTo(feature));
		    Assert.That(optimisedFeature.Parent, Is.EqualTo(preBranch));
	    }

		[Test]
	    public void TestUnmerged()
	    {
		    Repository repo = new RepositoryImporter(new MockGit()).GetRepository();
		    Assert.That(
			    GraphOptimiser.GetUnmergedRefs(repo.Refs).Select(r => r.Name),
			    Is.EquivalentTo(new[] { "master", "other-branch" }));
	    }
	}
}
