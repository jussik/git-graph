using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public static class CommitUtils
	{
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

		public static IEnumerable<Ref> GetUnmergedRefs(RefCollection refs)
		{
			return refs.All.Where(r => !refs.Repository.CommitChildren.Contains(r.Commit.Id));
		}
	}
}