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
			var branches = GetRefs(git.GetBranches());
			var tags = GetRefs(git.GetTags());

			var branchLookup = GetRefLookup(branches);
			var tagLookup = GetRefLookup(tags);
			var map = git.GetCommits()
				.Select(line =>
				{
					var ids = line.Split(' ').Select(str => BigInteger.Parse(str, NumberStyles.HexNumber)).ToArray();
					return new
					{
						node = new Commit(ids[0], branchLookup, tagLookup),
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

			var commits =  map.ToDictionary(m => m.Key, m => m.Value.node);
			return new Repository
			{
				CommitsById = commits,
				BranchesByName = branches.ToDictionary(r => r.name, r => commits[r.commitId]),
				TagsByName = tags.ToDictionary(r => r.name, r => commits[r.commitId]),
				BranchesById = branchLookup,
				TagsById = tagLookup
			};
		}

		private List<(string name, BigInteger commitId)> GetRefs(IEnumerable<string> refs)
		{
			return refs.Select(line =>
			{
				var spIx = line.IndexOf(' ');
				if (spIx == -1)
					throw new NotSupportedException("Invalid ref syntax: " + line);
				return (
					line.Substring(spIx + 1),
					BigInteger.Parse(line.Substring(0, spIx), NumberStyles.HexNumber)
				);
			})
			.ToList();
		}
		private ILookup<BigInteger, string> GetRefLookup(IEnumerable<(string name, BigInteger commitId)> refs)
		{
			return refs.ToLookup(r => r.commitId, r => r.name);
		}
	}
}