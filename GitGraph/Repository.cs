using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class Repository
	{
		public Dictionary<BigInteger, Commit> CommitsById { get; internal set; }
		public Dictionary<string, Commit> BranchesByName { get; internal set; }
		public Dictionary<string, Commit> TagsByName { get; internal set; }
		public ILookup<BigInteger, string> BranchesById { get; internal set; }
		public ILookup<BigInteger, string> TagsById { get; internal set; }
	}
}