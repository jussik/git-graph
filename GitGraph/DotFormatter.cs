using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitGraph
{
    public static class DotFormatter
    {
		public static async Task ToDigraphAsync(IEnumerable<Commit> commits, TextWriter stream)
		{
			var queue = new Queue<(Commit parent, Commit commit)>(commits
				.Where(c => c.Parent == null)
				.SelectMany(parent => parent.ChildCommits.Select(commit => (parent, commit))));
			var visited = new HashSet<Commit>();
			var line = new StringBuilder();

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
					line.Append(commit.Id.ToString("x"));

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
				line.Append(pair.parent.Id.ToString("x"));
				line.Append(" -> ");
				line.Append(pair.commit.Id.ToString("x"));

				ProcessChildren(pair.commit);
				await stream.WriteLineAsync(line.ToString());
			}
			await stream.WriteLineAsync("}");
		}
    }
}
