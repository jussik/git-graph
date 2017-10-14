using System.Linq;
using GitGraph.Input;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
	public class CommitUtilsTests
	{
		[Test]
		public void TestUnmerged()
		{
			Repository repo = new RepositoryImporter(new MockGit()).GetRepository();
			Assert.That(
				CommitUtils.GetUnmergedRefs(repo.Refs).Select(r => r.Name),
				Is.EquivalentTo(new[] { "master", "other-branch" }));
		}
	}
}