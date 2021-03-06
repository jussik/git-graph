﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GitGraph.Optimisation;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class GraphOptimiserTests
    {
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
	}
}
