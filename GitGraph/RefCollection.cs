using System;
using System.Collections.Generic;
using System.Linq;

namespace GitGraph
{
	public class RefCollection
	{
		public Repository Repository { get; }
		public IReadOnlyList<Ref> All { get; }

		public RefCollection(Repository repo, IReadOnlyList<Ref> refs)
		{
			Repository = repo;
			All = refs;
		}

		public IEnumerable<Ref> Tags =>
			All.Where(r => r.Type == Ref.RefType.Tag);
		public IEnumerable<Ref> Branches =>
			All.Where(r => r.Type == Ref.RefType.Branch);

		public Ref ByName(string name) =>
			All.FirstOrDefault(r => r.Name == name) ?? throw new InvalidOperationException($"No such ref '{name}'");

		public RefCollection Subset(Func<IReadOnlyList<Ref>, IEnumerable<Ref>> filter) =>
			new RefCollection(Repository, filter(All).ToList());
	}
}