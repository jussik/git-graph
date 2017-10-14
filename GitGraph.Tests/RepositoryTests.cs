using NUnit.Framework;

namespace GitGraph.Tests
{
	[TestFixture]
    public class RepositoryTests
    {
	    [Test]
	    public void TestShortestPrefix()
	    {
			var builder = new RepositoryBuilder();
		    var c1 = builder.AddCommit("356a192b7913b04c54574d18c28d46e6395428ab");
		    var c2 = builder.AddCommit("356a192bbacccdf19c0760cab7aec4a8359010b0", c1);
			var c3 = builder.AddCommit("356a192bbac823babbb58edb1c8e14d7106e83bb", c2);
			var c4 = builder.AddCommit("1b6453892473a467d07372d45eb05abc2031647a", c3);
			builder.AddBranch("master", c4);
		    var repo = builder.BuildRepository();

			Assert.That(repo.GetCommitAbbrev(c1), Is.EqualTo("356a192b7")); 
			Assert.That(repo.GetCommitAbbrev(c2), Is.EqualTo("356a192bbacc")); 
			Assert.That(repo.GetCommitAbbrev(c3), Is.EqualTo("356a192bbac8")); 
			Assert.That(repo.GetCommitAbbrev(c4), Is.EqualTo("1b64538"));
		}

	    [Test]
	    public void TestPrefixSearch()
	    {
			var builder = new RepositoryBuilder();
		    var c1 = builder.AddCommit("356a192b7913b04c54574d18c28d46e6395428ab");
		    var c2 = builder.AddCommit("356a192bbacccdf19c0760cab7aec4a8359010b0", c1);
		    var c3 = builder.AddCommit("356a192bbac823babbb58edb1c8e14d7106e83bb", c2);
		    var c4 = builder.AddCommit("1b6453892473a467d07372d45eb05abc2031647a", c3);
		    builder.AddBranch("master", c4);
		    var repo = builder.BuildRepository();

			Assert.That(repo.FindCommit("1b64538"), Is.EqualTo(c4));
			Assert.That(repo.FindCommit("1b6453892473a467d0"), Is.EqualTo(c4));
			Assert.That(repo.FindCommit("1b6453892473a467d07372d45eb05abc2031647a"), Is.EqualTo(c4));
		}

	    [Test]
	    public void TestPrefixCollision()
	    {
			var builder = new RepositoryBuilder();
		    var c1 = builder.AddCommit("356a192b7913b04c54574d18c28d46e6395428ab");
		    var c2 = builder.AddCommit("356a192bbacccdf19c0760cab7aec4a8359010b0", c1);
		    var c3 = builder.AddCommit("356a192bbac823babbb58edb1c8e14d7106e83bb", c2);
		    var c4 = builder.AddCommit("1b6453892473a467d07372d45eb05abc2031647a", c3);
		    builder.AddBranch("master", c4);
		    var repo = builder.BuildRepository();

			Assert.That(() => repo.FindCommit("356a192bb"), Throws.Exception);
		}

	    [Test]
	    public void TestPrefixSearchMissing()
	    {
			var builder = new RepositoryBuilder();
		    var c1 = builder.AddCommit("356a192b7913b04c54574d18c28d46e6395428ab");
		    var c2 = builder.AddCommit("356a192bbacccdf19c0760cab7aec4a8359010b0", c1);
		    var c3 = builder.AddCommit("356a192bbac823babbb58edb1c8e14d7106e83bb", c2);
		    var c4 = builder.AddCommit("1b6453892473a467d07372d45eb05abc2031647a", c3);
		    builder.AddBranch("master", c4);
		    var repo = builder.BuildRepository();

			Assert.That(repo.FindCommit("6395428ab"), Is.Null);
		    Assert.That(repo.FindCommit("356a19292473a467d07372d45eb05abc2031647a"), Is.Null);
	    }
	}
}
