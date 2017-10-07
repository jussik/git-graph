using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class Repository
	{
		public List<Ref> Refs { get; }
		public Dictionary<BigInteger, Commit> CommitsById => commitsById.Value;

		private readonly Lazy<Dictionary<BigInteger, Commit>> commitsById;

		public Repository(IEnumerable<Ref> refs)
		{
			Refs = refs.ToList();
			commitsById = new Lazy<Dictionary<BigInteger, Commit>>(() =>
			{
				var map = new Dictionary<BigInteger, Commit>();
				var commits = new Stack<Commit>(Refs.Select(r => r.Commit));
				while (commits.TryPop(out Commit commit))
				{
					if (map.TryAdd(commit.Id, commit) && commit.Parent != null)
					{
						commits.Push(commit.Parent);
						if (commit.MergeParent != null)
							commits.Push(commit.MergeParent);
					}
				}
				return map;
			});
		}
	}

	public class Ref
	{
		public string Name { get; }
		public RefType Type { get; }
		public Commit Commit { get; }

		public enum RefType { Branch, Tag }

		public Ref(string name, RefType type, Commit commit)
		{
			Name = name;
			Type = type;
			Commit = commit;
		}

		public override string ToString() => Type == RefType.Tag ? $"<{Name}>" : Name;
	}
}