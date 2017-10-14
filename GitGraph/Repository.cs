using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Gma.DataStructures.StringSearch;

namespace GitGraph
{
	public class Repository
	{
		public List<Ref> Refs { get; }
		public Dictionary<BigInteger, Commit> CommitsById => commitsById.Value;
		public ILookup<BigInteger, Commit> CommitChildren => commitChildren.Value;

		private readonly Lazy<Dictionary<BigInteger, Commit>> commitsById;
		private readonly Lazy<ITrie<Commit>> commitsByPrefix;
		private readonly Lazy<ILookup<BigInteger, Commit>> commitChildren;

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
			commitsByPrefix = new Lazy<ITrie<Commit>>(() =>
			{
				var trie = new PatriciaTrie<Commit>();
				foreach (Commit commit in CommitsById.Values)
				{
					trie.Add(commit.ToString(), commit);
				}
				return trie;
			});
			commitChildren = new Lazy<ILookup<BigInteger, Commit>>(() => CommitsById.Values
				.SelectMany(c => c.Parents.Select(p => (parent: p, child: c)))
				.ToLookup(t => t.parent.Id, t => t.child));
		}

		public Commit FindCommit(string abbrev)
		{
			using (IEnumerator<Commit> match = commitsByPrefix.Value.Retrieve(abbrev).GetEnumerator())
			{
				if (!match.MoveNext())
					return null;
				Commit firstResult = match.Current;
				if (match.MoveNext())
					throw new InvalidOperationException("Ambiguous commit id " + abbrev);
				return firstResult;
			}
		}

		public string GetCommitAbbrev(Commit commit)
		{
			ITrie<Commit> trie = commitsByPrefix.Value;
			string name = commit.ToString();
			int maxLen = name.Length;
			for (int i = 7; i < maxLen; i++)
			{
				string attempt = name.Substring(0, i);
				using (IEnumerator<Commit> match = trie.Retrieve(attempt).GetEnumerator())
				{
					if (!match.MoveNext())
						throw new InvalidOperationException("Commit not in repository");
					if (match.Current == commit && !match.MoveNext())
						return attempt;
				}
			}
			return name;
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