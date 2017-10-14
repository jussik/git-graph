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

	    public static IEnumerable<Ref> GetUnmergedRefs(RefCollection refs)
	    {
		    return refs.All.Where(r => !refs.Repository.CommitChildren.Contains(r.Commit.Id));
	    }
    }
}
