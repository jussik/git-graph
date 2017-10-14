using System.Linq;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class GraphOptimiserTests
    {
		[Test]
	    public void TestCollapseChildren()
	    {
		}

	    [Test]
	    public void TestUnmerged()
	    {
		    var repo = new RepositoryImporter(new MockGit()).GetRepository();
		    Assert.That(
			    GraphOptimiser.GetUnmergedRefs(repo).Select(r => r.Name),
			    Is.EquivalentTo(new[] { "master", "other-branch" }));
	    }
	}
}
