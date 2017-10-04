using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitGraph
{
    public static class DotFormatter
    {
		public static async Task ToDigraphAsync(Repository repo, TextWriter stream)
		{
			var queue = new Queue<(Commit parent, Commit commit)>(repo.CommitsById.Values
				.Where(c => c.Parent == null)
				.SelectMany(parent => parent.ChildCommits.Select(commit => (parent, commit))));
			var visited = new HashSet<Commit>();
			var line = new StringBuilder();
			var labels = new Dictionary<Commit, string>();

			string FormatCommit(Commit commit)
			{
				var id = commit.Id.ToString("x");

				if ((commit.Branches.Length > 0 || commit.Tags.Length > 0) && !labels.ContainsKey(commit))
				{
					string label = id;
					if (commit.Branches.Length > 0)
					{
						label += "\\n" + string.Join("\\n", commit.Branches);
					}
					if (commit.Tags.Length > 0)
					{
						label += "\\n<" + string.Join(", ", commit.Tags) + ">";
					}
					labels[commit] = $"{id} [label=\"{label}\"]";
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

			while (queue.TryDequeue(out var pair))
			{
				line.Clear();
				line.Append(FormatCommit(pair.parent));
				line.Append(" -> ");
				line.Append(FormatCommit(pair.commit));

				ProcessChildren(pair.commit);
				await stream.WriteLineAsync(line.ToString());
			}

			foreach (var label in labels.Values)
			{
				await stream.WriteLineAsync(label);
			}

			await stream.WriteLineAsync("}");
		}
    }
}
