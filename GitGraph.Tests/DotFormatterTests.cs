﻿using System;
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
				DotFormatter.ToDigraph(repo, repo.Refs, sw);
				Console.WriteLine(sw.ToString());
				Assert.That(sw.ToString(), Is.EqualTo(@"
digraph {
rankdir=LR
node [width=0.1, height=0.1, shape=point, fontsize=10]
edge [arrowhead=none]
node [group=1]
""356a192"" -> ""da4b923"" -> ""1b64538"" -> ""ac3478d"" -> ""902ba3c""
node [group=2]
""da4b923"" -> ""77de68d"" -> ""c1dfd96""
node [group=3]
""77de68d"" -> ""ac3478d""
""902ba3c"" [shape=none, label=""master""]
""c1dfd96"" [shape=none, label=""other-branch""]
""ac3478d"" [shape=none, label=""<merged>""]
""356a192"" [shape=none, label=""<initial>""]
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
			    DotFormatter.ToDigraph(repo, repo.Refs.Where(r => r.Type != Ref.RefType.Tag).ToList(), sw);
				Console.WriteLine(sw.ToString());
			    Assert.That(sw.ToString(), Is.EqualTo(@"
digraph {
rankdir=LR
node [width=0.1, height=0.1, shape=point, fontsize=10]
edge [arrowhead=none]
node [group=1]
""356a192"" -> ""da4b923"" -> ""1b64538"" -> ""ac3478d"" -> ""902ba3c""
node [group=2]
""da4b923"" -> ""77de68d"" -> ""c1dfd96""
node [group=3]
""77de68d"" -> ""ac3478d""
""902ba3c"" [shape=none, label=""master""]
""c1dfd96"" [shape=none, label=""other-branch""]
}
".TrimStart()));
		    }
	    }
	}
}
