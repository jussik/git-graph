using System.Collections.Generic;
using System.Globalization;
using System.Numerics;

namespace GitGraph.Tests
{
	internal class RepositoryBuilder
	{
		private readonly List<Commit> commits;
		private readonly List<Ref> refs;
		private BigInteger nextId;

		public RepositoryBuilder()
		{
			commits = new List<Commit>();
			refs = new List<Ref>();
			nextId = 1;
		}

		public Commit AddCommit(Commit parent = null, Commit mergeParent = null)
		{
			var commit = new Commit(nextId++, parent, mergeParent);
			commits.Add(commit);
			return commit;
		}

		public Commit AddCommit(string sha, Commit parent = null, Commit mergeParent = null)
		{
			var commit = new Commit(BigInteger.Parse(sha, NumberStyles.HexNumber), parent, mergeParent);
			commits.Add(commit);
			return commit;
		}

		public void AddBranch(string name, Commit commit)
		{
			refs.Add(new Ref(name, Ref.RefType.Branch, commit));
		}

		public void AddTag(string name, Commit commit)
		{
			refs.Add(new Ref(name, Ref.RefType.Tag, commit));
		}

		public Repository BuildRepository()
		{
			return new Repository(commits, refs);
		}
	}
}