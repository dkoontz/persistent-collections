using System;
using System.Collections;
using System.Collections.Generic;

namespace PersistentCollections {

	abstract class ListNode<T> {
		public const int NODE_ELEMENTS = 32;
		protected const int FIVE_LOWEST_BITS = 0x1F;

		public readonly int Level;
		public readonly int Count;
		public readonly bool HasCapacity;

		public abstract T this[int index] { get; }
		public abstract ListNode<T> AppendLeafNode(ListNode<T> leaf);

		internal ListNode(int level, int count, bool hasCapacity) {
			Level = level;
			Count = count;
			HasCapacity = hasCapacity;
		}
	}

	class ListNodeParent<T> : ListNode<T> {
		#region DEBUGGING_ONLY_DELETE_WHEN_WORKING
		public int _Level { get { return Level; } }
		public int _Count { get { return Count; } }
		public bool _HasCapacity { get { return HasCapacity; } }
		public ListNode<T>[] _Children { get { return _children; } }
		#endregion

		// masks for various level of the tree
		readonly int[] levelMasks = { (int)Util.LongPow(2, 5) - 1, (int)Util.LongPow(2, 10) - 1, (int)Util.LongPow(2, 15) - 1, (int)Util.LongPow(2, 20) - 1, (int)Util.LongPow(2, 25) - 1 };

		public override ListNode<T> AppendLeafNode(ListNode<T> leaf) {
			if (HasCapacity) {
				if (Level == 1) {
					return new ListNodeParent<T>(Level, Count + NODE_ELEMENTS, _children.Length + 1 < NODE_ELEMENTS, Util.ExtendArrayAndAppend(_children, leaf));
				}
			
				var lastChild = _children[_children.Length - 1];
				if (lastChild.HasCapacity) {
					var newChildren = new ListNode<T>[_children.Length];
					_children.CopyTo(newChildren, 0);
					var newChild = lastChild.AppendLeafNode(leaf);
					newChildren[newChildren.Length - 1] = newChild;
					return new ListNodeParent<T>(Level, Count + NODE_ELEMENTS, newChildren.Length < NODE_ELEMENTS || newChild.HasCapacity, newChildren);
				}
				var child = new ListNodeParent<T>(Level - 1, 0, true, null);
				return new ListNodeParent<T>(Level, Count + NODE_ELEMENTS, true, Util.ExtendArrayAndAppend(_children, child.AppendLeafNode(leaf)));
			}

			var sibling = new ListNodeParent<T>(Level, 0, true, null);
			return new ListNodeParent<T>(Level + 1, Count + NODE_ELEMENTS, true, new [] { 
				this, 
				sibling.AppendLeafNode(leaf)
			});
		}

		public override T this[int index] {
			get {
				var currentLevelIndex = ((uint)index) >> (Level * 5);
				var childLevelIndex = index & levelMasks[Level - 1];
				return _children[currentLevelIndex][childLevelIndex];
			}
		}

		readonly ListNode<T>[] _children;

		public ListNodeParent(int level, int count, bool hasCapacity, ListNode<T>[] children) : base(level, count, hasCapacity) {
			if (children == null) {
				if (level > 1) {
					_children = new [] { new ListNodeParent<T>(level - 1, 0, true, null) };
				}
				else  {
					_children = new ListNode<T>[0];
				}
			}
			else {
				_children = children;
			}
		}
	}

	class ListNodeLeaf<T> : ListNode<T> {

		public override T this[int index] { 
			get { return _values[index]; }
		}

		public override ListNode<T> AppendLeafNode(ListNode<T> child) {
			return new ListNodeParent<T>(1, Count + child.Count, true, new [] { this, child });
		}

		readonly T[] _values;

		public ListNodeLeaf(T[] values) : base(0, NODE_ELEMENTS, false) {
			_values = values;
		}
	}

	public class PersistentList<T> : IEnumerable<T>{
		public static readonly PersistentList<T> EMPTY;
		internal static readonly T[] EMPTY_VALUES;

		struct PersistentListEnumerator : IEnumerator<T> {
			const int resetIndex = -1;
			PersistentList<T> list;
			int currentIndex;

			public PersistentListEnumerator(PersistentList<T> list) {
				this.list = list;
				currentIndex = resetIndex;
			}

			public PersistentListEnumerator(PersistentList<T> list, int startIndex) {
				this.list = list;
				currentIndex = startIndex;
			}

			public bool MoveNext() {
				++currentIndex;
				return (currentIndex < list.Count);
			}

			public T Current {
				get { return list[currentIndex]; }
			}

			object IEnumerator.Current {
				get { return list[currentIndex]; }
			}

			public void Reset() {
				currentIndex = resetIndex;
			}

			public void Dispose() {}
		}

		static PersistentList() {
			EMPTY_VALUES = new T[0];
			EMPTY = new PersistentList<T>(null, EMPTY_VALUES);
		}

		public IEnumerator<T> GetEnumerator() {
			return new PersistentListEnumerator(this);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return new PersistentListEnumerator(this);
		}

		public int Count { 
			get { return (_root == null ? 0 : _root.Count) + _tail.Length; } 
		}

		readonly ListNode<T> _root;
		readonly T[] _tail = EMPTY_VALUES;

		public PersistentList(IEnumerable<T> values) {
			var currentLevel = new Queue<ListNode<T>>();
			var value = new T[ListNode<T>.NODE_ELEMENTS];
			int index = 0;
			T[] tail = null;

			foreach (var v in values) {
				value[index] = v;
				if (index == ListNode<T>.NODE_ELEMENTS - 1) {
					currentLevel.Enqueue(new ListNodeLeaf<T>(value));
					value = new T[ListNode<T>.NODE_ELEMENTS];
					index = 0;
				}
				else {
					++index;
				}
			}

			if (index > 0) {
				tail = new T[index];
				Array.Copy(value, tail, index);
			}

			// build up elements starting at level 1 -> 2 -> 3 each time consuming 
			// all of the previous level in NODE_ELEMENT element chunks
			var nextLevel = new Queue<ListNode<T>>();
			var children = new ListNode<T>[ListNode<T>.NODE_ELEMENTS];
			int level = 1;
			index = 0;

			while (currentLevel.Count > 1) {
				while (currentLevel.Count > 0) {
					children[index] = currentLevel.Dequeue();
					if (index == ListNode<T>.NODE_ELEMENTS - 1) {
						nextLevel.Enqueue(new ListNodeParent<T>(level, (int)Util.LongPow(32, level + 1), false, children));
						index = 0;
						children = new ListNode<T>[ListNode<T>.NODE_ELEMENTS];
					}
					else {
						++index;
					}
				}
				if (index > 0) {
					var partialChildren = new ListNode<T>[index];
					var count = 0;
					for (var i = 0; i < index; ++i) {
						partialChildren[i] = children[i];
						count += children[i].Count;
					}
					nextLevel.Enqueue(new ListNodeParent<T>(level, count, true, partialChildren));
					children = new ListNode<T>[ListNode<T>.NODE_ELEMENTS];
					index = 0;
				}

				var temp = nextLevel;
				nextLevel = currentLevel;
				currentLevel = temp;
				++level;
			}

			_tail = tail;
			_root = currentLevel.Dequeue();
		}

		internal PersistentList(ListNode<T> root) {
			_root = root;
		}

		internal PersistentList(ListNode<T> root, T[] values) {
			_root = root;
			_tail = values;
		}

		public PersistentList<T> With(T value) {
			var values = Util.ExtendArrayAndAppend(_tail, value);
			if (_tail.Length < 31) {
				return new PersistentList<T>(_root, values);
			}

			var leaf = new ListNodeLeaf<T>(values);

			if (_root == null) {
				return new PersistentList<T>(leaf);
			}

			return new PersistentList<T>(_root.AppendLeafNode(leaf));
		}

		public PersistentList<T> Without(T value) {
			// find location of element in list
			// copy all elements from the current node, up to the located value to the tail
			// get an enumerator starting at the located value + 1
			// for each element until end of list, 

			throw new NotImplementedException("Last thing to implement is removal of an element");
		}

		public T this[int index] {
			get { 
				if (_root == null) {
					return _tail[index];
				}

				if (index > _root.Count - 1) {
					return _tail[index - _root.Count];
				}

				return _root[index];
			}
		}
	}
}