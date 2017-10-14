using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class RepositoryImporter
	{
		private readonly IGit git;

		public RepositoryImporter(IGit git)
		{
			this.git = git;
		}

		public Repository GetRepository()
		{
			List<BigInteger[]> commits = git.GetCommits()
				.Select(line => line.Split(' ')
					.Select(str => BigInteger.Parse(str, NumberStyles.HexNumber))
					.ToArray())
				.ToList();

			Dictionary<BigInteger, Commit> commitMap = GetCommitMap(commits);

			IEnumerable<Ref> branches = GetRefs(git.GetBranches(), Ref.RefType.Branch, commitMap);
			IEnumerable<Ref> tags = GetRefs(git.GetTags(), Ref.RefType.Tag, commitMap);

			return new Repository(commitMap.Values, branches.Concat(tags));
		}

		public static Dictionary<BigInteger, Commit> GetCommitMap(List<BigInteger[]> commits)
		{
			ILookup<BigInteger, BigInteger[]> childLookup = commits
				.SelectMany(c => c.Skip(1).Select(p => (parent: p, commit: c)))
				.ToLookup(t => t.parent, t => t.commit);

			var commitMap = new Dictionary<BigInteger, Commit>();
			var remainingCommits = new Queue<BigInteger[]>();

			void ProcessCommit(Commit commit)
			{
				if (!commitMap.TryAdd(commit.Id, commit))
					return;

				foreach (BigInteger[] childCommit in childLookup[commit.Id])
				{
					remainingCommits.Enqueue(childCommit);
				}
			}

			foreach (BigInteger[] root in commits.Where(c => c.Length == 1))
			{
				ProcessCommit(new Commit(root[0]));
			}
			while (remainingCommits.TryDequeue(out BigInteger[] ids))
			{
				Commit mergeParent = null;
				if (commitMap.TryGetValue(ids[1], out Commit parent)
					&& (ids.Length < 3 || commitMap.TryGetValue(ids[2], out mergeParent)))
					ProcessCommit(new Commit(ids[0], parent, mergeParent));
				else
					remainingCommits.Enqueue(ids);
			}
			return commitMap;
		}

		private static IEnumerable<Ref> GetRefs(IEnumerable<string> refs, Ref.RefType refType, Dictionary<BigInteger, Commit> commits)
		{
			return refs.Select(line =>
			{
				int spIx = line.IndexOf(' ');
				if (spIx == -1)
					throw new NotSupportedException("Invalid ref syntax: " + line);
				return new Ref(
					line.Substring(spIx + 1),
					refType,
					commits[BigInteger.Parse(line.Substring(0, spIx), NumberStyles.HexNumber)]);
			});
		}
	}
}