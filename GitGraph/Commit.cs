using System;
using System.Numerics;

namespace GitGraph
{
	public class Commit
	{
		public BigInteger Id { get; }
		public Commit Parent { get; }
		public Commit MergeParent { get; }
		public BigInteger[] Ids => ids.Value;
		public Commit[] Parents => parents.Value;

		private readonly Lazy<BigInteger[]> ids;
		private readonly Lazy<Commit[]> parents;

		public Commit(BigInteger id, Commit parent = null, Commit mergeParent = null)
		{
			Id = id;
			Parent = parent;
			MergeParent = mergeParent;
			ids = new Lazy<BigInteger[]>(() =>
			{
				if (MergeParent != null)
					return new[] {Id, Parent.Id, MergeParent.Id};
				if (Parent != null)
					return new[] {Id, Parent.Id};
				return new[] {Id};
			});
			parents = new Lazy<Commit[]>(() =>
			{
				if (MergeParent != null)
					return new[] {Parent, MergeParent};
				if (Parent != null)
					return new[] {Parent};
				return Array.Empty<Commit>();
			});
		}

		public override string ToString() => Id.ToString("x40");

		private static bool Equals(Commit left, Commit right) => ReferenceEquals(left, right) || left?.Id == right?.Id;
		public static bool operator ==(Commit left, Commit right) => Equals(left, right);
		public static bool operator !=(Commit left, Commit right) => !Equals(left, right);
		public override bool Equals(object obj) => Equals(this, obj as Commit);
		public override int GetHashCode() => Id.GetHashCode();
	}
}