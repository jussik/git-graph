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

		public Dictionary<BigInteger, Commit> GetCommits()
		{
			var branches = GetBranches();
			var tags = GetTags();
			var map = git.GetCommits()
				.Select(line =>
				{
					var ids = line.Split(' ').Select(str => BigInteger.Parse(str, NumberStyles.HexNumber)).ToArray();
					return new
					{
						node = new Commit(ids[0], branches, tags),
						ids
					};
				})
				.ToDictionary(c => c.node.Id);

			Commit ApplyNode(Commit node, BigInteger[] ids, int ix)
			{
				if (ids.Length < ix + 1)
					return null;
				Commit parent = map.GetValueOrDefault(ids[ix])?.node
					?? throw new InvalidOperationException("No such commit " + ids[ix]);
				parent.ChildCommits.Add(node);
				return parent;
			}

			foreach (var commit in map.Values)
			{
				commit.node.Parent = ApplyNode(commit.node, commit.ids, 1);
				commit.node.MergeParent = ApplyNode(commit.node, commit.ids, 2);
			}
			return map.ToDictionary(m => m.Key, m => m.Value.node);
		}

		private ILookup<BigInteger, string> GetTags() => GetRefs(git.GetTags());
		private ILookup<BigInteger, string> GetBranches() => GetRefs(git.GetBranches());

		private ILookup<BigInteger, string> GetRefs(IEnumerable<string> refs)
		{
			return refs.Select(line =>
				{
					var spIx = line.IndexOf(' ');
					if (spIx == -1)
						throw new NotSupportedException("Invalid ref syntax: " + line);
					return new
					{
						commitId = BigInteger.Parse(line.Substring(0, spIx), NumberStyles.HexNumber),
						name = line.Substring(spIx + 1)
					};
				})
				.ToLookup(r => r.commitId, r => r.name);
		}
	}
}