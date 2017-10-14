using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class GraphOptimiserTests
    {
		[Test]
	    public void TestCollapseChain()
	    {
			var root = new Commit(1);
			var middle = new Commit(2, root);
			var head = new Commit(3, middle);
		    var repo = new Repository(new List<Ref> {new Ref("master", Ref.RefType.Branch, head)});

		    Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedHead = optimised.Refs.All[0].Commit;
		    Assert.That(optimisedHead, Is.EqualTo(head));
			Assert.That(optimisedHead.Parent, Is.EqualTo(root));
	    }

	    [Test]
	    public void TestCollapseMergeLoop()
		{
			var root = new Commit(1);
			var branch1 = new Commit(2, root);
			var branch2 = new Commit(3, root);
			var head = new Commit(4, branch1, branch2);
			var repo = new Repository(new List<Ref> { new Ref("master", Ref.RefType.Branch, head) });

			Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

			Commit optimisedHead = optimised.Refs.All[0].Commit;
			Assert.That(optimisedHead, Is.EqualTo(head));
			Assert.That(optimisedHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestCollapseMergeLoopWithChains()
	    {
		    var root = new Commit(1);
			var preBranch = new Commit(2, root);
		    var branch1 = new Commit(3, preBranch);
			var chain1 = new Commit(4, branch1);
		    var branch2 = new Commit(5, preBranch);
			var chain2 = new Commit(6, branch2);
		    var merge = new Commit(7, chain1, chain2);
		    var head = new Commit(8, merge);
		    var repo = new Repository(new List<Ref> { new Ref("master", Ref.RefType.Branch, head) });

		    Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedHead = optimised.Refs.All[0].Commit;
		    Assert.That(optimisedHead, Is.EqualTo(head));
		    Assert.That(optimisedHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestCollapseMergeDoubleLoop()
	    {
		    var root = new Commit(1);
		    var preBranch = new Commit(2, root);
		    var branch1 = new Commit(3, preBranch);
		    var branch2 = new Commit(4, preBranch);
		    var merge1 = new Commit(5, branch1, branch2);
		    var branch3 = new Commit(6, branch2);
		    var merge2 = new Commit(7, merge1, branch3);
		    var head = new Commit(8, merge2);
		    var repo = new Repository(new List<Ref> { new Ref("master", Ref.RefType.Branch, head) });

		    Repository optimised = GraphOptimiser.GetOptimised(repo.Refs);

		    Commit optimisedHead = optimised.Refs.All[0].Commit;
		    Assert.That(optimisedHead, Is.EqualTo(head));
		    Assert.That(optimisedHead.Parent, Is.EqualTo(root));
		}

	    [Test]
	    public void TestCollapseSplitHeads()
	    {
		    var root = new Commit(1);
		    var middle = new Commit(2, root);
		    var master = new Commit(3, middle);
		    var feature = new Commit(4, middle);
		    var repo = new Repository(new List<Ref>
		    {
			    new Ref("master", Ref.RefType.Branch, master),
				new Ref("feature", Ref.RefType.Branch, feature)
		    });

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
		    var root = new Commit(1);
		    var preBranch = new Commit(2, root);
		    var masterMid = new Commit(3, preBranch);
		    var featureMid = new Commit(4, preBranch);
		    var master = new Commit(3, masterMid);
		    var feature = new Commit(4, featureMid);
			var repo = new Repository(new List<Ref>
		    {
			    new Ref("master", Ref.RefType.Branch, master),
			    new Ref("feature", Ref.RefType.Branch, feature)
		    });

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
		    var repo = new RepositoryImporter(new MockGit()).GetRepository();
		    Assert.That(
			    GraphOptimiser.GetUnmergedRefs(repo.Refs).Select(r => r.Name),
			    Is.EquivalentTo(new[] { "master", "other-branch" }));
	    }
	}
}
