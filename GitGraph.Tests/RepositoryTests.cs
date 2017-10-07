using System.Globalization;
using System.Numerics;
using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class RepositoryTests
    {
	    [Test]
	    public void TestShortestPrefix()
	    {
		    var c1 = new Commit(BigInteger.Parse("356a192b7913b04c54574d18c28d46e6395428ab", NumberStyles.HexNumber));
		    var c2 = new Commit(BigInteger.Parse("356a192bbacccdf19c0760cab7aec4a8359010b0", NumberStyles.HexNumber), c1);
			var c3 = new Commit(BigInteger.Parse("356a192bbac823babbb58edb1c8e14d7106e83bb", NumberStyles.HexNumber), c2);
			var c4 = new Commit(BigInteger.Parse("1b6453892473a467d07372d45eb05abc2031647a", NumberStyles.HexNumber), c3);
		    var repo = new Repository(new[] {new Ref("HEAD", Ref.RefType.Branch, c4)});

			Assert.That(repo.GetCommitAbbrev(c1), Is.EqualTo("356a192b7")); 
			Assert.That(repo.GetCommitAbbrev(c2), Is.EqualTo("356a192bbacc")); 
			Assert.That(repo.GetCommitAbbrev(c3), Is.EqualTo("356a192bbac8")); 
			Assert.That(repo.GetCommitAbbrev(c4), Is.EqualTo("1b64538"));
		}

	    [Test]
	    public void TestPrefixSearch()
	    {
		    var c1 = new Commit(BigInteger.Parse("356a192b7913b04c54574d18c28d46e6395428ab", NumberStyles.HexNumber));
		    var c2 = new Commit(BigInteger.Parse("356a192bbacccdf19c0760cab7aec4a8359010b0", NumberStyles.HexNumber), c1);
		    var c3 = new Commit(BigInteger.Parse("356a192bbac823babbb58edb1c8e14d7106e83bb", NumberStyles.HexNumber), c2);
		    var c4 = new Commit(BigInteger.Parse("1b6453892473a467d07372d45eb05abc2031647a", NumberStyles.HexNumber), c3);
		    var repo = new Repository(new[] { new Ref("HEAD", Ref.RefType.Branch, c4) });

			Assert.That(repo.FindCommit("1b64538"), Is.EqualTo(c4));
			Assert.That(repo.FindCommit("1b6453892473a467d0"), Is.EqualTo(c4));
			Assert.That(repo.FindCommit("1b6453892473a467d07372d45eb05abc2031647a"), Is.EqualTo(c4));
		}

	    [Test]
	    public void TestPrefixCollision()
	    {
		    var c1 = new Commit(BigInteger.Parse("356a192b7913b04c54574d18c28d46e6395428ab", NumberStyles.HexNumber));
		    var c2 = new Commit(BigInteger.Parse("356a192bbacccdf19c0760cab7aec4a8359010b0", NumberStyles.HexNumber), c1);
		    var c3 = new Commit(BigInteger.Parse("356a192bbac823babbb58edb1c8e14d7106e83bb", NumberStyles.HexNumber), c2);
		    var c4 = new Commit(BigInteger.Parse("1b6453892473a467d07372d45eb05abc2031647a", NumberStyles.HexNumber), c3);
		    var repo = new Repository(new[] { new Ref("HEAD", Ref.RefType.Branch, c4) });

		    Assert.That(() => repo.FindCommit("356a192bb"), Throws.Exception);
		}

	    [Test]
	    public void TestPrefixSearchMissing()
	    {
		    var c1 = new Commit(BigInteger.Parse("356a192b7913b04c54574d18c28d46e6395428ab", NumberStyles.HexNumber));
		    var c2 = new Commit(BigInteger.Parse("356a192bbacccdf19c0760cab7aec4a8359010b0", NumberStyles.HexNumber), c1);
		    var c3 = new Commit(BigInteger.Parse("356a192bbac823babbb58edb1c8e14d7106e83bb", NumberStyles.HexNumber), c2);
		    var c4 = new Commit(BigInteger.Parse("1b6453892473a467d07372d45eb05abc2031647a", NumberStyles.HexNumber), c3);
		    var repo = new Repository(new[] { new Ref("HEAD", Ref.RefType.Branch, c4) });

		    Assert.That(repo.FindCommit("6395428ab"), Is.Null);
		    Assert.That(repo.FindCommit("356a19292473a467d07372d45eb05abc2031647a"), Is.Null);
	    }
	}
}
