using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
    public static class GraphOptimiser
    {
	    public static Repository GetOptimised(Repository source, bool includeTags = true)
	    {
		    throw new NotImplementedException();
	    }

	    public static IEnumerable<Ref> GetUnmergedRefs(IReadOnlyCollection<Ref> refs)
	    {
		    Dictionary<BigInteger, Ref> unmerged = refs.ToDictionary(r => r.Commit.Id);
		    var queue = new Queue<Commit>();
		    foreach (var commit in refs.Select(r => r.Commit))
		    {
			    if (commit.Parent != null && !queue.Contains(commit.Parent))
				    queue.Enqueue(commit.Parent);
			    if (commit.MergeParent != null && !queue.Contains(commit.MergeParent))
				    queue.Enqueue(commit.MergeParent);
		    }

		    while (queue.TryDequeue(out Commit commit))
		    {
			    do
			    {
				    unmerged.Remove(commit.Id);
				    if (commit.MergeParent != null)
					    queue.Enqueue(commit.MergeParent);
			    } while ((commit = commit.Parent) != null);
		    }

		    return refs.Where(r => unmerged.ContainsKey(r.Commit.Id));
	    }
    }
}
