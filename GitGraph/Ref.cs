namespace GitGraph
{
	public class Ref
	{
		public string Name { get; }
		public RefType Type { get; }
		public Commit Commit { get; }

		public enum RefType { Branch, Tag }

		public Ref(string name, RefType type, Commit commit)
		{
			Name = name;
			Type = type;
			Commit = commit;
		}

		public override string ToString() => Type == RefType.Tag ? $"<{Name}>" : Name;
	}
}