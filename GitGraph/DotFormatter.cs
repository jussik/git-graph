using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
    public static class DotFormatter
    {
	    public static void ToDigraph(Repository repo, IReadOnlyCollection<Ref> refs, TextWriter stream)
	    {
		    stream.WriteLine("digraph {");
		    stream.WriteLine("rankdir=LR");
		    stream.WriteLine("node [width=0.1, height=0.1, shape=point, fontsize=10]");
		    stream.WriteLine("edge [arrowhead=none, weight=1]");

		    var processedCommits = new HashSet<Commit>();
		    var processedMerges = new HashSet<(Commit parent, Commit commit)>();
			var commitQueue = new Queue<Commit>(GraphOptimiser.GetUnmergedRefs(repo, refs)
				.OrderBy(r => r.Name == "master" ? "" : r.Name)
				.Select(r => r.Commit)
				.Distinct());
			var mergeQueue = new Queue<(Commit parent, Commit commit)>();

			int group = 0;
		    int weight = 1;
		    while (commitQueue.Count > 0)
		    {
			    while (commitQueue.TryDequeue(out Commit commit))
			    {
				    var firstParents = new Stack<Commit>();

				    while (commit != null)
				    {
					    firstParents.Push(commit);
					    if (!processedCommits.Add(commit))
							break;

						if (commit.MergeParent != null)
						{
							var merge = (commit.MergeParent, commit);
							if (processedMerges.Add(merge))
							    mergeQueue.Enqueue(merge);
					    }

					    commit = commit.Parent;
				    }

					if(firstParents.Count < 2)
						continue;

				    stream.Write("node [group=");
				    stream.Write(++group);
				    stream.WriteLine("]");
				    if (weight != 1)
				    {
					    stream.WriteLine("edge [style=solid, weight=1]");
					    weight = 1;
				    }

					using (var nodes = firstParents.GetEnumerator())
				    {
					    if (nodes.MoveNext())
					    {
						    AppendCommit(nodes.Current, repo, stream);
						    while (nodes.MoveNext())
						    {
								stream.Write(" -> ");
								AppendCommit(nodes.Current, repo, stream);
						    }
							stream.WriteLine();
					    }
				    }
				}
			    while (mergeQueue.TryDequeue(out (Commit parent, Commit commit) merge))
				{
					if (weight != 0)
				    {
						stream.WriteLine("node [group=merges]");
					    stream.WriteLine("edge [style=dashed, weight=0]");
					    weight = 0;
				    }

					AppendCommit(merge.parent, repo, stream);
					stream.Write(" -> ");
					AppendCommit(merge.commit, repo, stream);
					stream.WriteLine();

					if(!processedCommits.Contains(merge.parent))
						commitQueue.Enqueue(merge.parent);
				}
		    }

			// ref labels
			Dictionary<BigInteger, IEnumerable<Ref>> refsById = refs
				.GroupBy(r => r.Commit.Id)
				.ToDictionary(g => g.Key, g => (IEnumerable<Ref>)g);
			foreach (Ref r in refs)
			{
				if (!refsById.TryGetValue(r.Commit.Id, out IEnumerable<Ref> commitRefs))
					continue; // already processed this commit

				AppendCommit(r.Commit, repo, stream);
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

	    private static void AppendCommit(Commit commit, Repository repo, TextWriter stream)
	    {
		    stream.Write('"');
		    stream.Write(repo.GetCommitAbbrev(commit));
		    stream.Write('"');
		}
	}
}
