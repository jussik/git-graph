using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
    public static class GraphOptimiser
    {
		/// <summary>
		/// Create a new simplified but topologically similar repository
		/// </summary>
		/// <param name="refs"></param>
		/// <returns></returns>
	    public static Repository GetOptimised(RefCollection refs)
	    {
		    List<Commit> whitelist = refs.All.Select(r => r.Commit).ToList();
		    IEnumerable<Graft> grafts = GetChainGrafts(whitelist).Union(GetLoopGrafts(whitelist));
		    Dictionary<BigInteger, Commit> grafted = GetGraftedCommits(refs.Repository.Commits, grafts);
		    PruneCommits(grafted, whitelist);

		    IEnumerable<Ref> newRefs = refs.All
			    .Select(r => grafted.TryGetValue(r.Commit.Id, out Commit commit) ? new Ref(r.Name, r.Type, commit) : null)
			    .Where(r => r != null);

			return new Repository(grafted.Values, newRefs);
	    }

	    public class Graft
	    {
		    public BigInteger[] Ids { get; }

		    public Graft(params BigInteger[] ids)
		    {
			    Ids = ids;
		    }
		}

		/// <summary>
		/// Get a map of new Commits based on a set of source commits and grafts
		/// </summary>
		/// <param name="commits"></param>
		/// <param name="grafts"></param>
		/// <returns></returns>
		public static Dictionary<BigInteger, Commit> GetGraftedCommits(IEnumerable<Commit> commits, IEnumerable<Graft> grafts)
	    {
		    Dictionary<BigInteger, Graft> graftsById = grafts.ToDictionary(g => g.Ids[0]);
		    List<BigInteger[]> graftedCommits = commits
			    .Select(c => graftsById.TryGetValue(c.Id, out Graft graft) ? graft.Ids : c.Ids)
			    .ToList();

		    return RepositoryImporter.GetCommitMap(graftedCommits);
	    }

		/// <summary>
		/// Remove all commits not accessible from whitelist
		/// </summary>
		/// <param name="commits"></param>
		/// <param name="whitelist"></param>
	    public static void PruneCommits(Dictionary<BigInteger, Commit> commits, IEnumerable<Commit> whitelist)
	    {
		    var refsById = new HashSet<Commit>(whitelist);

		    var childCounter = new Dictionary<Commit, List<Commit>>();
			foreach (Commit commit in commits.Values)
			{
				foreach (Commit parent in commit.Parents)
				{
					if (!childCounter.TryGetValue(parent, out List<Commit> children))
					{
						children = new List<Commit>(1);
						childCounter.Add(parent, children);
					}

					children.Add(commit);
				}
			}

			var danglingQueue = new Queue<Commit>(commits.Values.Where(c => !childCounter.ContainsKey(c)));

		    while (danglingQueue.TryDequeue(out Commit commit))
		    {
			    do
			    {
				    if (refsById.Contains(commit) || childCounter.ContainsKey(commit))
					    break;

					commits.Remove(commit.Id);

					foreach (Commit parent in commit.Parents)
					{
						if (childCounter.TryGetValue(parent, out List<Commit> children) && children.Remove(commit) && children.Count == 0)
							childCounter.Remove(parent);
					}

				    if (commit.MergeParent != null)
					    danglingQueue.Enqueue(commit.MergeParent);
			    } while ((commit = commit.Parent) != null);
		    }
	    }

		/// <summary>
		/// Remove unnecessary commits inside chains (successive commits without branches or merges)
		/// </summary>
		/// <param name="whitelist">commits to keep in the reduced repo</param>
		/// <returns>A map of commits to their new parents (grafts)</returns>
		public static IEnumerable<Graft> GetChainGrafts(IEnumerable<Commit> whitelist)
	    {
			var whitelistMap = new HashSet<Commit>(whitelist);
			var checkQueue = new Queue<Commit>(whitelistMap);
			var chainHeadQueue = new Queue<Commit>();
			var processedHeads = new HashSet<Commit>();

		    while (checkQueue.Count > 0)
		    {
			    while (checkQueue.TryDequeue(out Commit commit))
			    {
				    if (commit.Parent == null)
					    continue;

				    if (commit.MergeParent == null)
				    {
					    // single parent, may be head of chain
					    chainHeadQueue.Enqueue(commit);
				    }
				    else
					{
						// merge commit, check both parents for possible chains
						checkQueue.Enqueue(commit.Parent);
						checkQueue.Enqueue(commit.MergeParent);
					}
			    }

			    // chain head is always commit with only one parent
			    // root is always commit with zero or multiple parents
			    while (chainHeadQueue.TryDequeue(out Commit head))
			    {
				    if (processedHeads.Contains(head) || head.Parent == null)
					    continue;

				    Commit root = head;

				    do
				    {
					    root = root.Parent;

					    if (root.MergeParent != null || whitelistMap.Contains(root))
						{
							foreach (Commit parent in root.Parents)
							{
								checkQueue.Enqueue(parent);
							}
						    break;
					    }
				    } while (root.Parent != null);

				    if (root != head.Parent)
				    {
					    processedHeads.Add(head);
						yield return new Graft(head.Id, root.Id);
				    }
			    }
		    }
	    }

	    public static IEnumerable<Graft> GetLoopGrafts(IEnumerable<Commit> whitelist)
	    {
		    throw new NotImplementedException();
	    }

	    public static IEnumerable<Ref> GetUnmergedRefs(RefCollection refs)
	    {
		    return refs.All.Where(r => !refs.Repository.CommitChildren.Contains(r.Commit.Id));
	    }
    }
}
