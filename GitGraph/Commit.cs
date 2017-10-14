using System.Collections.Generic;
using System.Numerics;

namespace GitGraph
{
	public class Commit
	{
		public BigInteger Id { get; }
		public Commit Parent { get; }
		public Commit MergeParent { get; }

		public Commit(BigInteger id, Commit parent = null, Commit mergeParent = null)
		{
			Id = id;
			Parent = parent;
			MergeParent = mergeParent;
		}

		public IEnumerable<Commit> Parents
		{
			get
			{
				if (Parent != null)
				{
					yield return Parent;
					if (MergeParent != null)
						yield return MergeParent;
				}
			}
		}

		public override string ToString() => Id.ToString("x40");

		private static bool Equals(Commit left, Commit right) => ReferenceEquals(left, right) || left?.Id == right?.Id;
		public static bool operator ==(Commit left, Commit right) => Equals(left, right);
		public static bool operator !=(Commit left, Commit right) => !Equals(left, right);
		public override bool Equals(object obj) => Equals(this, obj as Commit);
		public override int GetHashCode() => Id.GetHashCode();
	}
}