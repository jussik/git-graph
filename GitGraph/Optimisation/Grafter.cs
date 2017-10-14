using System;
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
					if (commit.Parent == null)
						continue;

					if (commit.MergeParent == null)
					{
						// single parent, may be head of chain
						chainHeadQueue.Enqueue(commit);
					}
					else
					{
						// merge commit, check both parents for possible chains
						checkQueue.Enqueue(commit.Parent);
						checkQueue.Enqueue(commit.MergeParent);
					}
				}

				// chain head is always commit with only one parent
				// root is always commit with zero or multiple parents
				while (chainHeadQueue.TryDequeue(out Commit head))
				{
					if (processedHeads.Contains(head) || head.Parent == null)
						continue;

					Commit root = head;

					do
					{
						root = root.Parent;

						if (root.MergeParent != null || whitelist.Contains(root))
						{
							foreach (Commit parent in root.Parents)
							{
								checkQueue.Enqueue(parent);
							}
							break;
						}
					} while (root.Parent != null);

					if (root != head.Parent)
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Get a map of new Commits based on a set of source commits and grafts
		/// </summary>
		public Dictionary<BigInteger, Commit> GetCommits(IEnumerable<Commit> commits)
		{
			List<BigInteger[]> graftedCommits = commits
				.Select(c => Grafts.TryGetValue(c.Id, out Graft graft) ? graft.Ids : c.Ids)
				.ToList();

			return CommitUtils.GetCommitMap(graftedCommits);
		}
	}
}