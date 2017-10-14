using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
    public static class GraphOptimiser
    {
	    public static Repository GetOptimised(RefCollection refs)
	    {
		    IEnumerable<Graft> grafts = GetChainGrafts(refs);
		    Dictionary<BigInteger, Commit> grafted = GetGraftedCommits(refs, grafts);
		    PruneCommits(grafted, refs.All.Select(r => r.Commit));

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

	    public static Dictionary<BigInteger, Commit> GetGraftedCommits(RefCollection refs, IEnumerable<Graft> grafts)
	    {
		    Dictionary<BigInteger, Graft> graftsById = grafts.ToDictionary(g => g.Ids[0]);
		    List<BigInteger[]> graftedCommits = refs.Repository.Commits
			    .Select(c => graftsById.TryGetValue(c.Id, out Graft graft) ? graft.Ids : c.Ids)
			    .ToList();

		    return RepositoryImporter.GetCommitMap(graftedCommits);
	    }

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
		/// <param name="refs">refs to keep in the reduced repo</param>
		/// <returns>A map of commits to their new parents (grafts)</returns>
		public static IEnumerable<Graft> GetChainGrafts(RefCollection refs)
	    {
			var whitelist = new HashSet<Commit>(refs.All.Select(r => r.Commit));
			var checkQueue = new Queue<Commit>(whitelist);
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

					    if (root.MergeParent != null)
					    {
						    checkQueue.Enqueue(root.Parent);
						    checkQueue.Enqueue(root.MergeParent);
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

	    public static IEnumerable<Graft> GetLoopGrafts(RefCollection refs)
	    {
		    throw new NotImplementedException();
	    }

	    public static IEnumerable<Ref> GetUnmergedRefs(RefCollection refs)
	    {
		    return refs.All.Where(r => !refs.Repository.CommitChildren.Contains(r.Commit.Id));
	    }
    }
}
