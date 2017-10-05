using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GitGraph
{
	public class Git : IGit
	{
		private readonly string workingDirectory;

		public Git(string workingDirectory)
		{
			this.workingDirectory = workingDirectory;
		}

		public IEnumerable<string> GetCommits() => GetLines("rev-list --remotes --parents");
		public IEnumerable<string> GetTags() => GetRefs("refs/tags");
		public IEnumerable<string> GetBranches() => GetRefs("refs/remotes/origin");

		private IEnumerable<string> GetRefs(string type) => GetLines("for-each-ref --format=\"%(objectname) %(refname:short)\" " + type);

		private IEnumerable<string> GetLines(string command)
		{
			var p = new Process
			{
				StartInfo = new ProcessStartInfo("git", command)
				{
					RedirectStandardOutput = true,
					WorkingDirectory = workingDirectory,
					UseShellExecute = false,
					StandardOutputEncoding = Encoding.UTF8
				}
			};
			p.Start();

			string line;
			while ((line = p.StandardOutput.ReadLine()) != null)
			{
				yield return line;
			}

			p.WaitForExit();
			if (p.ExitCode != 0)
				throw new ApplicationException("Git command failed");
		}
	}
}