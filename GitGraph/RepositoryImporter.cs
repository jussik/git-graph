using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class GraphProcessor
	{
		private readonly IGit git;

		public GraphProcessor(IGit git)
		{
			this.git = git;
		}

		public Repository GetRepository()
		{
			ILookup<BigInteger, string> branchLookup = GetRefLookup(git.GetBranches());
			ILookup<BigInteger, string> tagLookup = GetRefLookup(git.GetTags());
			List<(Commit node, BigInteger[] ids)> commits = git.GetCommits()
				.Select(line =>
				{
					BigInteger[] ids = line.Split(' ')
						.Select(str => BigInteger.Parse(str, NumberStyles.HexNumber))
						.ToArray();
					return (node: new Commit(ids[0], branchLookup, tagLookup), ids);
				})
				.ToList();

			Dictionary<BigInteger, (Commit node, BigInteger[] ids)> map = commits.ToDictionary(c => c.node.Id);

			Commit ApplyNode(Commit commit, BigInteger[] ids, int ix)
			{
				if (ids.Length < ix + 1)
					return null;
				Commit parent = map.TryGetValue(ids[ix], out var c) ? c.node
					: throw new InvalidOperationException("No such commit " + ids[ix]);
				parent.ChildCommits.Add(commit);
				return parent;
			}

			foreach ((Commit node, BigInteger[] ids) pair in commits)
			{
				pair.node.Parent = ApplyNode(pair.node, pair.ids, 1);
				pair.node.MergeParent = ApplyNode(pair.node, pair.ids, 2);
			}

			return new Repository(commits.Select(c => map[c.ids[0]].node));
		}

		private static ILookup<BigInteger, string> GetRefLookup(IEnumerable<string> refs)
		{
			return refs.Select(line =>
			{
				int spIx = line.IndexOf(' ');
				if (spIx == -1)
					throw new NotSupportedException("Invalid ref syntax: " + line);
				return (
					name: line.Substring(spIx + 1),
					id: BigInteger.Parse(line.Substring(0, spIx), NumberStyles.HexNumber)
				);
			}).ToLookup(r => r.id, r => r.name);
		}
	}
}