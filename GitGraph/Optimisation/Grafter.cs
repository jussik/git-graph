using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GitGraph.Optimisation
{
	public class Grafter
	{
		public Dictionary<BigInteger, Graft> Grafts { get; }

		private readonly HashSet<Commit> whitelist;

		public Grafter(IEnumerable<Commit> whitelist)
		{
			this.whitelist = new HashSet<Commit>(whitelist);
			Grafts = new Dictionary<BigInteger, Graft>();
		}

		private Commit Parent(Commit commit) => Grafts.TryGetValue(commit.Id, out Graft graft) ? graft.Parent : commit.Parent;
		private Commit MergeParent(Commit commit) => Grafts.TryGetValue(commit.Id, out Graft graft) ? graft.MergeParent : commit.MergeParent;
		private IEnumerable<Commit> Parents(Commit commit) => Grafts.TryGetValue(commit.Id, out Graft graft) ? graft.Commits.Skip(1) : commit.Parents;
		private BigInteger[] Ids(Commit commit) => Grafts.TryGetValue(commit.Id, out Graft graft) ? graft.Ids : commit.Ids;

		/// <summary>
		/// Get an unpruned map of new Commits based on a set of source commits
		/// </summary>
		public Dictionary<BigInteger, Commit> GetCommitMap(IEnumerable<Commit> commits) => CommitUtils.GetCommitMap(commits.Select(Ids).ToList());

		/// <summary>
		/// Remove unnecessary commits inside chains (successive commits without branches or merges)
		/// </summary>
		public Grafter GraftChains()
		{
			var checkQueue = new Queue<Commit>(whitelist);
			var chainHeadQueue = new Queue<Commit>();
			var processedHeads = new HashSet<Commit>();

			while (checkQueue.Count > 0)
			{
				while (checkQueue.TryDequeue(out Commit commit))
				{
					Commit parent = Parent(commit);
					if (parent == null)
						continue;

					Commit mergeParent = MergeParent(commit);

					if (mergeParent == null)
					{
						// single parent, may be head of chain
						chainHeadQueue.Enqueue(commit);
					}
					else
					{
						// merge commit, check both parents for possible chains
						checkQueue.Enqueue(parent);
						checkQueue.Enqueue(mergeParent);
					}
				}

				// chain head is always commit with only one parent
				// root is always commit with zero or multiple parents
				while (chainHeadQueue.TryDequeue(out Commit head))
				{
					if (processedHeads.Contains(head))
						continue;

					Commit headParent = Parent(head);
					Commit rootParent = headParent;

					if (headParent == null)
						continue;

					Commit root;
					do
					{
						root = rootParent;

						if (whitelist.Contains(root) || MergeParent(root) != null)
						{
							foreach (Commit parent in Parents(root))
							{
								checkQueue.Enqueue(parent);
							}
							break;
						}
					} while ((rootParent = Parent(root)) != null);

					if (root != headParent)
					{
						processedHeads.Add(head);
						Grafts[head.Id] = new Graft(head, root);
					}
				}
			}

			return this;
		}

		public Grafter GraftLoops()
		{
			// map commits to following merge commits and if there is a whitelisted commit between them
			// record all instances where a branch -> merge is duplicated
			// remove duplicates, keeping any branches with whitelisted commits

			var queue = new Queue<Commit>(whitelist);
			var processedMerges = new HashSet<Commit>();
			var nextMerge = new ListLookup<Commit, (Commit merge, bool whitelist)>();

			while (queue.TryDequeue(out Commit merge))
			{
				if(processedMerges.Contains(merge))
					continue;

				processedMerges.Add(merge);

				bool inWhitelist = false;
				Commit commit = Parent(merge);
				if (commit == null)
					continue;

				do
				{
					var mergeParent = MergeParent(commit);
					if (mergeParent != null)
					{
						queue.Enqueue(Parent(commit));
						queue.Enqueue(mergeParent);
					}

					inWhitelist = inWhitelist || whitelist.Contains(commit);
					nextMerge.Add(commit, (merge, inWhitelist));
				} while ((commit = Parent(commit)) != null);
			}
			return this;
		}
	}
}