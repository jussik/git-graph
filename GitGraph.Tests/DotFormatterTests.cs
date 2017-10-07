using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class DotFormatterTests
    {
	    [Test]
	    public void TestDigraph()
	    {
		    var repo = new RepositoryImporter(new MockGit()).GetRepository();
		    using (StringWriter sw = new StringWriter())
			{
				DotFormatter.ToDigraph(repo.Refs, sw);
				Console.WriteLine(sw.ToString());
				Assert.That(sw.ToString(), Is.EqualTo(@"
digraph {
rankdir=LR
node [width=0.1, height=0.1, shape=point, fontsize=10]
edge [arrowhead=none]
node [group=1]
""1"" -> ""2"" -> ""4"" -> ""5"" -> ""7""
node [group=2]
""2"" -> ""3"" -> ""6""
node [group=3]
""3"" -> ""5""
""7"" [shape=none, label=""master""]
""6"" [shape=none, label=""other-branch""]
""5"" [shape=none, label=""<merged>""]
""1"" [shape=none, label=""<initial>""]
}
".TrimStart()));
			}
		}

	    [Test]
	    public void TestDigraphNoTags()
	    {
		    var repo = new RepositoryImporter(new MockGit()).GetRepository();
		    using (StringWriter sw = new StringWriter())
		    {
			    DotFormatter.ToDigraph(repo.Refs.Where(r => r.Type != Ref.RefType.Tag).ToList(), sw);
				Console.WriteLine(sw.ToString());
			    Assert.That(sw.ToString(), Is.EqualTo(@"
digraph {
rankdir=LR
node [width=0.1, height=0.1, shape=point, fontsize=10]
edge [arrowhead=none]
node [group=1]
""1"" -> ""2"" -> ""4"" -> ""5"" -> ""7""
node [group=2]
""2"" -> ""3"" -> ""6""
node [group=3]
""3"" -> ""5""
""7"" [shape=none, label=""master""]
""6"" [shape=none, label=""other-branch""]
}
".TrimStart()));
		    }
	    }
	}
}
