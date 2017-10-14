using System;
using System.Numerics;

namespace GitGraph
{
	public class Graft
	{
		public BigInteger Id { get; }
		public Commit[] Commits { get; }
		public BigInteger[] Ids { get; }

		public Commit Commit => Commits[0];
		public Commit Parent => Commits.Length > 1 ? Commits[1] : null;
		public Commit MergeParent => Commits.Length > 2 ? Commits[2] : null;

		public Graft(params Commit[] commits)
		{
			Id = commits[0].Id;
			Commits = commits;
			Ids = Array.ConvertAll(commits, c => c.Id);
		}
	}
}