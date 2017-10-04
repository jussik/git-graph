using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph
{
	public class Commit
	{
		public BigInteger Id { get; }
		public Commit Parent { get; internal set; }
		public Commit MergeParent { get; internal set; }
		public List<Commit> ChildCommits { get; }
		public string[] Branches { get; }
		public string[] Tags { get; }

		public Commit(BigInteger id, ILookup<BigInteger, string> branches, ILookup<BigInteger, string> tags)
		{
			Id = id;
			Branches = branches[id].ToArray();
			Tags = tags[id].ToArray();
			ChildCommits = new List<Commit>(1);
		}
	}
}