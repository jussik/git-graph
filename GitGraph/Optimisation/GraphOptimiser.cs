using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph.Optimisation
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

		    Dictionary<BigInteger, Commit> graftedCommits = new Grafter(whitelist)
			    .GraftChains()
			    .GetCommits(refs.Repository.Commits);

		    PruneCommits(graftedCommits, whitelist);

		    IEnumerable<Ref> newRefs = refs.All
			    .Select(r => graftedCommits.TryGetValue(r.Commit.Id, out Commit commit) ? new Ref(r.Name, r.Type, commit) : null)
			    .Where(r => r != null);

			return new Repository(graftedCommits.Values, newRefs);
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
    }
}
