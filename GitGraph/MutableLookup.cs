using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GitGraph
{
	public interface IMutableLookup<TKey, TValue> : ILookup<TKey, TValue>
	{
		void Add(TKey key, TValue value);
		void Remove(TKey key);
		void Remove(TKey key, TValue value);
		bool TryGetValues(TKey key, out IEnumerable<TValue> values);
	}

	public abstract class MutableLookup<TKey, TValue, TContainer> : IMutableLookup<TKey, TValue>
		where TContainer : ICollection<TValue>, new()
	{
		private readonly Dictionary<TKey, TContainer> lookup;

		protected MutableLookup() => lookup = new Dictionary<TKey, TContainer>();

		public void Add(TKey key, TValue value)
		{
			if (!lookup.TryGetValue(key, out TContainer container))
			{
				container = new TContainer();
				lookup.Add(key, container);
			}

			container.Add(value);
		}

		public void Remove(TKey key)
		{
			lookup.Remove(key);
		}

		public void Remove(TKey key, TValue value)
		{
			if (lookup.TryGetValue(key, out TContainer container))
			{
				container.Remove(value);
				if (ContainerIsEmpty(container))
					lookup.Remove(key);
			}
		}

		public bool TryGetValues(TKey key, out IEnumerable<TValue> values)
		{
			bool success = lookup.TryGetValue(key, out TContainer container);
			values = container;
			return success;
		}

		protected virtual bool ContainerIsEmpty(TContainer container) => container.Count == 0;

		public IEnumerator<IGrouping<TKey, TValue>> GetEnumerator() => lookup
			.SelectMany(p => p.Value.Select(v => (k: p.Key, v)))
			.GroupBy(p => p.k, p => p.v)
			.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		public bool Contains(TKey key) => lookup.ContainsKey(key);
		public int Count => lookup.Count;
		public IEnumerable<TValue> this[TKey key] => lookup.TryGetValue(key, out TContainer values) ? values : Enumerable.Empty<TValue>();
	}

	public sealed class ListLookup<TKey, TValue> : MutableLookup<TKey, TValue, List<TValue>> { }
	public sealed class HashSetLookup<TKey, TValue> : MutableLookup<TKey, TValue, HashSet<TValue>> { }
	public sealed class LinkedListLookup<TKey, TValue> : MutableLookup<TKey, TValue, LinkedList<TValue>>
	{
		protected override bool ContainerIsEmpty(LinkedList<TValue> container) => container.First == null;
	}

	public static class MutableLookupExtensions
	{
		private static TLookup FillLookup<TLookup, TKey, TElement>(TLookup lookup, IEnumerable<TElement> source, Func<TElement, TKey> keySelector)
			where TLookup : IMutableLookup<TKey, TElement>
		{
			foreach (TElement element in source)
			{
				lookup.Add(keySelector(element), element);
			}
			return lookup;
		}

		private static TLookup FillLookup<TLookup, TSource, TKey, TElement>(TLookup lookup, IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
			where TLookup : IMutableLookup<TKey, TElement>
		{
			foreach (TSource element in source)
			{
				lookup.Add(keySelector(element), elementSelector(element));
			}
			return lookup;
		}

		public static ListLookup<TKey, TElement> ToListLookup<TKey, TElement>(this IEnumerable<TElement> source, Func<TElement, TKey> keySelector)
			=> FillLookup(new ListLookup<TKey, TElement>(), source, keySelector);

		public static ListLookup<TKey, TElement> ToListLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
			=> FillLookup(new ListLookup<TKey, TElement>(), source, keySelector, elementSelector);

		public static HashSetLookup<TKey, TElement> ToHashSetLookup<TKey, TElement>(this IEnumerable<TElement> source, Func<TElement, TKey> keySelector)
			=> FillLookup(new HashSetLookup<TKey, TElement>(), source, keySelector);

		public static HashSetLookup<TKey, TElement> ToHashSetLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
			=> FillLookup(new HashSetLookup<TKey, TElement>(), source, keySelector, elementSelector);

		public static LinkedListLookup<TKey, TElement> ToLinkedListLookup<TKey, TElement>(this IEnumerable<TElement> source, Func<TElement, TKey> keySelector)
			=> FillLookup(new LinkedListLookup<TKey, TElement>(), source, keySelector);

		public static LinkedListLookup<TKey, TElement> ToLinkedListLookup<TSource, TKey, TElement>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
			=> FillLookup(new LinkedListLookup<TKey, TElement>(), source, keySelector, elementSelector);
	}
}