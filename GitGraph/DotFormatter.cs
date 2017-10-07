using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
    public static class DotFormatter
    {
	    public static void ToDigraph(IReadOnlyCollection<Ref> refs, TextWriter stream)
	    {
		    stream.WriteLine("digraph {");
		    stream.WriteLine("rankdir=LR");
		    stream.WriteLine("node [width=0.1, height=0.1, shape=point, fontsize=10]");
		    stream.WriteLine("edge [arrowhead=none]");

			IEnumerable<(Commit parent, Commit child)> GetCommitPairs(Ref r)
			{
				if (r.Commit.Parent != null)
					yield return (r.Commit.Parent, r.Commit);
				if (r.Commit.MergeParent != null)
					yield return (r.Commit.MergeParent, r.Commit);
			}

		    var processed = new HashSet<BigInteger>();
			var queue = new Queue<(Commit parent, Commit child)>(GraphOptimiser.GetUnmergedRefs(refs).SelectMany(GetCommitPairs));

			// commit chains
			int group = 0;
			while(queue.TryDequeue(out var commitPair))
			{
				stream.Write("node [group=");
				stream.Write(++group);
				stream.WriteLine("]");

				var firstParents = new List<Commit> {commitPair.child, commitPair.parent};
				if (commitPair.parent.MergeParent != null)
					queue.Enqueue((commitPair.parent.MergeParent, commitPair.parent));

				Commit commit = commitPair.parent;
				while (!processed.Contains(commit.Id) && (commit = commit.Parent) != null)
				{
					firstParents.Add(commit);

					if (processed.Contains(commit.Id))
						break;

					if (commit.MergeParent != null)
						queue.Enqueue((commit.MergeParent, commit));
				}

				AppendCommit(firstParents[firstParents.Count - 1], stream);
				for (int i = firstParents.Count - 2; i >= 0; --i)
				{
					stream.Write(" -> ");
					AppendCommit(firstParents[i], stream);
				}
				stream.WriteLine();

				processed.UnionWith(firstParents.Select(l => l.Id));
			}

			// ref labels
			Dictionary<BigInteger, IEnumerable<Ref>> refsById = refs
				.GroupBy(r => r.Commit.Id)
				.ToDictionary(g => g.Key, g => (IEnumerable<Ref>)g);
			foreach (Ref r in refs)
			{
				if (!refsById.TryGetValue(r.Commit.Id, out IEnumerable<Ref> commitRefs))
					continue; // already processed this commit

				AppendCommit(r.Commit, stream);
				stream.Write(" [shape=none, label=\"");
				refsById.Remove(r.Commit.Id);
				using (IEnumerator<Ref> crefs = commitRefs.GetEnumerator())
				{
					if (crefs.MoveNext())
					{
						stream.Write(crefs.Current);
						while (crefs.MoveNext())
						{
							stream.Write("\\n");
							stream.Write(crefs.Current);
						}
					}
				}
				stream.WriteLine("\"]");
			}

			stream.WriteLine("}");
		}

	    private static void AppendCommit(Commit commit, TextWriter stream)
	    {
		    stream.Write('"');
		    stream.Write(commit);
		    stream.Write('"');
		}
	}
}
