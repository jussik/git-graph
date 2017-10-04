using System.IO;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class DotFormatterTests
    {
	    [Test]
	    public void TestDigraph()
	    {
			var commits = new GraphProcessor(new MockGit()).GetCommits();
			using (StringWriter sw = new StringWriter())
			{
				DotFormatter.ToDigraphAsync(commits.Values, sw).Wait();
				Assert.That(sw.ToString(), Is.EqualTo(@"digraph {
1 -> 2 -> 4 -> 5 -> 7
2 -> 3 -> 6
3 -> 5
}
"));
			}
		}
    }
}
