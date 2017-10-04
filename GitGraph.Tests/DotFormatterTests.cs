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
		    var repo = new GraphProcessor(new MockGit()).GetRepository();
		    using (StringWriter sw = new StringWriter())
			{
				DotFormatter.ToDigraphAsync(repo, sw).Wait();
				Assert.That(sw.ToString(), Is.EqualTo(@"digraph {
rankdir=LR
1 -> 2 -> 4 -> 5 -> 7
2 -> 3 -> 6
3 -> 5
1 [label=""1\n<initial>""]
5 [label=""5\n<merged>""]
7 [label=""7\nmaster""]
6 [label=""6\nother-branch""]
}
"));
			}
		}
    }
}
