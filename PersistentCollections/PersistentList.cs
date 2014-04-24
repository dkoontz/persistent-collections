using System;
using System.Collections;
using System.Collections.Generic;

namespace PersistentCollections {

	abstract class ListNode<T> {
		public readonly int Layer;
		public readonly int Count;
		public readonly bool HasCapacity;

		public abstract T this[int index] { get; }
		public abstract ListNode<T> With(T value);
		public abstract ListNode<T> AppendLeafNode(ListNode<T> leaf);

		internal ListNode(int layer, int count, bool hasCapacity) {
			Layer = layer;
			Count = count;
			HasCapacity = hasCapacity;
		}
	}

	class ListNodeParent<T> : ListNode<T> {
		public int _Layer { get { return Layer; } }
		public int _Count { get { return Count; } }
		public bool _HasCapacity { get { return HasCapacity; } }
		public ListNode<T>[] _Children { get { return _children; } }

		public override ListNode<T> With(T value) {
			throw new NotImplementedException();
		}

		public override ListNode<T> AppendLeafNode(ListNode<T> leaf) {
			if (HasCapacity) {
				if (Layer == 1) {
					return new ListNodeParent<T>(Layer, Count + 32, _children.Length + 1 < 32, Util.ExtendArrayAndAppend(_children, leaf));
				}
			
				var lastChild = _children[_children.Length - 1];
				if (lastChild.HasCapacity) {
					var newChildren = new ListNode<T>[_children.Length];
					_children.CopyTo(newChildren, 0);
					var newChild = lastChild.AppendLeafNode(leaf);
					newChildren[newChildren.Length - 1] = newChild;
					return new ListNodeParent<T>(Layer, Count + 32, newChildren.Length < 32 || newChild.HasCapacity, newChildren);
				}
				var child = new ListNodeParent<T>(Layer - 1, 0, true, null);
				return new ListNodeParent<T>(Layer, Count + 32, true, Util.ExtendArrayAndAppend(_children, child.AppendLeafNode(leaf)));
			}

			var sibling = new ListNodeParent<T>(Layer, 0, true, null);
			return new ListNodeParent<T>(Layer + 1, Count + 32, true, new [] { 
				this, 
				sibling.AppendLeafNode(leaf)
			});
		}

		public override T this[int index] {
			get {
				throw new NotImplementedException();
			}
		}

		readonly ListNode<T>[] _children;

		public ListNodeParent(int layer, int count, bool hasCapacity, ListNode<T>[] children) : base(layer, count, hasCapacity) {
			if (children == null) {
				if (layer > 1) {
					_children = new [] { new ListNodeParent<T>(layer - 1, 0, true, null) };
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
		const int FIVE_LOWEST_BITS = 0x1F;

		public override T this[int index] { 
			get { return _values[index & FIVE_LOWEST_BITS]; } 
		}

		public override ListNode<T> With(T value) {
			throw new NotImplementedException();
		}

		public override ListNode<T> AppendLeafNode(ListNode<T> child) {
			return new ListNodeParent<T>(1, Count + child.Count, true, new [] { this, child });
		}

		readonly T[] _values;

		public ListNodeLeaf(T[] values) : base(0, 32, false) {
			_values = values;
		}
	}

	public class PersistentList<T> {
		public static readonly PersistentList<T> EMPTY;
		internal static readonly T[] EMPTY_VALUES;

		static PersistentList() {
			EMPTY_VALUES = new T[0];
			EMPTY = new PersistentList<T>(null, EMPTY_VALUES);
		}

		public int Count { 
			get { return (_root == null ? 0 : _root.Count) + _tail.Length; } 
		}

		readonly ListNode<T> _root;
		readonly T[] _tail = EMPTY_VALUES;

		// public PersistentList(IEnumerable<T>) {
		//
		//}

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
	}
}