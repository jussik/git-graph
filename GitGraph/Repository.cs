using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class Repository
	{
		public List<Commit> Commits { get; }
		public Dictionary<BigInteger, Commit> CommitsById => commitsById.Value;

		private readonly Lazy<Dictionary<BigInteger, Commit>> commitsById;

		public Repository(IEnumerable<Commit> commits)
		{
			Commits = commits.ToList();
			commitsById = new Lazy<Dictionary<BigInteger, Commit>>(() => Commits.ToDictionary(c => c.Id));
		}
	}
}