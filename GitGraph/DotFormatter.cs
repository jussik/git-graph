using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitGraph
{
    public static class DotFormatter
    {
		public static async Task ToDigraphAsync(Repository repo, TextWriter stream, bool includeTags)
		{
			var queue = new Queue<(Commit parent, Commit commit)>(repo.Commits
				.Where(c => c.Parent == null)
				.SelectMany(parent => parent.ChildCommits.Select(commit => (parent, commit))));
			var visited = new HashSet<Commit>();
			var line = new StringBuilder();
			var labels = new Dictionary<Commit, string>();

			string FormatCommit(Commit commit)
			{
				var id = '"' + commit.Id.ToString("x") + '"';

				if ((commit.Branches.Length > 0 || commit.Tags.Length > 0) && !labels.ContainsKey(commit))
				{
					string label = null;
					if (commit.Branches.Length > 0)
					{
						label = string.Join("\\n", commit.Branches);
					}
					if (includeTags && commit.Tags.Length > 0)
					{
						if (label != null)
							label += "\\n";
						label += "<" + string.Join(", ", commit.Tags) + ">";
					}
					labels[commit] = label != null ? $"{id} [shape=none, label=\"{label}\"]" : null;
				}

				return id;
			}

			void ProcessChildren(Commit parent)
			{
				if (!visited.Add(parent))
					return;

				using (var children = parent.ChildCommits.GetEnumerator())
				{
					if (!children.MoveNext())
						return;

					Commit commit = children.Current;
					line.Append(" -> ");
					line.Append(FormatCommit(commit));

					while (children.MoveNext())
					{
						queue.Enqueue((parent, children.Current));
					}

					ProcessChildren(commit);
				}
			}

			await stream.WriteLineAsync("digraph {");
			await stream.WriteLineAsync("rankdir=LR");
			await stream.WriteLineAsync("node [width=0.1, height=0.1, shape=point, fontsize=10]");
			await stream.WriteLineAsync("edge [arrowhead=none]");

			int groupNum = 0;
			while (queue.TryDequeue(out var pair))
			{
				await stream.WriteLineAsync($"node [group={++groupNum}]");
				line.Clear();
				line.Append(FormatCommit(pair.parent));
				line.Append(" -> ");
				line.Append(FormatCommit(pair.commit));

				ProcessChildren(pair.commit);
				await stream.WriteLineAsync(line.ToString());
			}

			foreach (var label in labels.Values)
			{
				if(label != null)
					await stream.WriteLineAsync(label);
			}

			await stream.WriteLineAsync("}");
		}
    }
}
