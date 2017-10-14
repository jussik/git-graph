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

	    public static IEnumerable<Ref> GetUnmergedRefs(Repository repo, IReadOnlyCollection<Ref> refs = null)
	    {
		    return (refs ?? repo.Refs).Where(r => !repo.CommitChildren.Contains(r.Commit.Id));
	    }
    }
}
