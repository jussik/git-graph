using System.Collections.Generic;

namespace GitGraph.Input
{
	public interface IGit
	{
		IEnumerable<string> GetBranches();
		IEnumerable<string> GetCommits();
		IEnumerable<string> GetTags();
	}
}