using System;
using System.Collections.Generic;
using System.Linq;

namespace GitGraph
{
    public static class GraphOptimiser
    {
	    public static Repository GetOptimised(RefCollection refs)
	    {
			throw new NotImplementedException();
	    }

		/// <summary>
		/// Remove unnecessary commits inside chains (successive commits without branches or merges)
		/// </summary>
		/// <param name="refs">refs to keep in the reduced repo</param>
		/// <returns>A map of commits to their new parents (grafts)</returns>
		public static Dictionary<Commit, Commit> GetChainGrafts(RefCollection refs)
	    {
			var whitelist = new HashSet<Commit>(refs.All.Select(r => r.Commit));
			var checkQueue = new Queue<Commit>(whitelist);
			var chainHeadQueue = new Queue<Commit>();
		    var chainHeadsToRoots = new Dictionary<Commit, Commit>();

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
				    if (chainHeadsToRoots.ContainsKey(head) || head.Parent == null)
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
					    chainHeadsToRoots.Add(head, root);
			    }
		    }

		    return chainHeadsToRoots;
	    }

	    public static Dictionary<Commit, Commit> GetLoopGrafts(RefCollection refs)
	    {
		    throw new NotImplementedException();
	    }

	    public static IEnumerable<Ref> GetUnmergedRefs(RefCollection refs)
	    {
		    return refs.All.Where(r => !refs.Repository.CommitChildren.Contains(r.Commit.Id));
	    }
    }
}
