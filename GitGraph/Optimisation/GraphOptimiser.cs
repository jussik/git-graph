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
				.GraftLoops()
			    .GraftChains()
			    .GetCommitMap(refs.Repository.Commits);

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

			// linked list because we need memory efficiency and fast remove and there will only be a few elements at most
			var childCounter = new LinkedListLookup<Commit, Commit>();
			foreach (Commit commit in commits.Values)
			{
				foreach (Commit parent in commit.Parents)
				{
					childCounter.Add(parent, commit);
				}
			}

			var danglingQueue = new Queue<Commit>(commits.Values.Where(c => !childCounter.Contains(c)));

		    while (danglingQueue.TryDequeue(out Commit commit))
		    {
			    do
			    {
				    if (refsById.Contains(commit) || childCounter.Contains(commit))
					    break;

					commits.Remove(commit.Id);

					foreach (Commit parent in commit.Parents)
					{
						childCounter.Remove(parent, commit);
					}

				    if (commit.MergeParent != null)
					    danglingQueue.Enqueue(commit.MergeParent);
			    } while ((commit = commit.Parent) != null);
		    }
	    }
    }
}
