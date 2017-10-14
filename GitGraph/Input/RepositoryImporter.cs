using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;

namespace GitGraph.Input
{
	public class RepositoryImporter
	{
		private readonly IGit git;

		public RepositoryImporter(IGit git)
		{
			this.git = git;
		}

		public Repository GetRepository()
		{
			List<BigInteger[]> commits = git.GetCommits()
				.Select(line => line.Split(' ')
					.Select(str => BigInteger.Parse(str, NumberStyles.HexNumber))
					.ToArray())
				.ToList();

			Dictionary<BigInteger, Commit> commitMap = CommitUtils.GetCommitMap(commits);

			IEnumerable<Ref> branches = GetRefs(git.GetBranches(), Ref.RefType.Branch, commitMap);
			IEnumerable<Ref> tags = GetRefs(git.GetTags(), Ref.RefType.Tag, commitMap);

			return new Repository(commitMap.Values, branches.Concat(tags));
		}

		private static IEnumerable<Ref> GetRefs(IEnumerable<string> refs, Ref.RefType refType, Dictionary<BigInteger, Commit> commits)
		{
			return refs.Select(line =>
			{
				int spIx = line.IndexOf(' ');
				if (spIx == -1)
					throw new NotSupportedException("Invalid ref syntax: " + line);
				return new Ref(
					line.Substring(spIx + 1),
					refType,
					commits[BigInteger.Parse(line.Substring(0, spIx), NumberStyles.HexNumber)]);
			});
		}
	}
}