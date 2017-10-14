using System;
using System.Collections.Generic;
using System.Linq;

namespace GitGraph
{
	public class RefCollection
	{
		public Repository Repository { get; }
		public IReadOnlyList<Ref> Refs { get; }

		public RefCollection(Repository repo, IReadOnlyList<Ref> refs)
		{
			Repository = repo;
			Refs = refs;
		}

		public RefCollection Subset(Func<IReadOnlyList<Ref>, IEnumerable<Ref>> filter)
		{
			return new RefCollection(Repository, filter(Refs).ToList());
		}
	}
}