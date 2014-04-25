using System;
using System.Collections;
using System.Collections.Generic;

namespace PersistentCollections {

	abstract class ListNode<T> {
		public const int NODE_ELEMENTS = 32;
		protected const int FIVE_LOWEST_BITS = 0x1F;

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
					return new ListNodeParent<T>(Layer, Count + NODE_ELEMENTS, _children.Length + 1 < NODE_ELEMENTS, Util.ExtendArrayAndAppend(_children, leaf));
				}
			
				var lastChild = _children[_children.Length - 1];
				if (lastChild.HasCapacity) {
					var newChildren = new ListNode<T>[_children.Length];
					_children.CopyTo(newChildren, 0);
					var newChild = lastChild.AppendLeafNode(leaf);
					newChildren[newChildren.Length - 1] = newChild;
					return new ListNodeParent<T>(Layer, Count + NODE_ELEMENTS, newChildren.Length < NODE_ELEMENTS || newChild.HasCapacity, newChildren);
				}
				var child = new ListNodeParent<T>(Layer - 1, 0, true, null);
				return new ListNodeParent<T>(Layer, Count + NODE_ELEMENTS, true, Util.ExtendArrayAndAppend(_children, child.AppendLeafNode(leaf)));
			}

			var sibling = new ListNodeParent<T>(Layer, 0, true, null);
			return new ListNodeParent<T>(Layer + 1, Count + NODE_ELEMENTS, true, new [] { 
				this, 
				sibling.AppendLeafNode(leaf)
			});
		}

		public override T this[int index] {
			get {
				return _children[((uint)index) >> (Layer * 5)][index & FIVE_LOWEST_BITS];
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

		public override T this[int index] { 
			get { return _values[index]; }
		}

		public override ListNode<T> With(T value) {
			throw new NotImplementedException("Cannot append to a leaf node, leaves should be created by parent nodes");
		}

		public override ListNode<T> AppendLeafNode(ListNode<T> child) {
			return new ListNodeParent<T>(1, Count + child.Count, true, new [] { this, child });
		}

		readonly T[] _values;

		public ListNodeLeaf(T[] values) : base(0, NODE_ELEMENTS, false) {
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

		public PersistentList(IEnumerable<T> values) {
			var currentLayer = new Queue<ListNode<T>>();
			var value = new T[ListNode<T>.NODE_ELEMENTS];
			int index = 0;
			T[] tail = null;

			foreach (var v in values) {
				value[index] = v;
				if (index == ListNode<T>.NODE_ELEMENTS - 1) {
					currentLayer.Enqueue(new ListNodeLeaf<T>(value));
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

			// build up elements starting at layer 1 -> 2 -> 3 each time consuming 
			// all of the previous layer in NODE_ELEMENT element chunks
			var nextLayer = new Queue<ListNode<T>>();
			var children = new ListNode<T>[ListNode<T>.NODE_ELEMENTS];
			int layer = 1;
			index = 0;

			while (currentLayer.Count > 1) {
				while (currentLayer.Count > 0) {
					children[index] = currentLayer.Dequeue();
					if (index == ListNode<T>.NODE_ELEMENTS - 1) {
						nextLayer.Enqueue(new ListNodeParent<T>(layer, (int)Util.IntPow(32, layer + 1), false, children));
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
					nextLayer.Enqueue(new ListNodeParent<T>(layer, count, true, partialChildren));
					children = new ListNode<T>[ListNode<T>.NODE_ELEMENTS];
					index = 0;
				}

				var temp = nextLayer;
				nextLayer = currentLayer;
				currentLayer = temp;
				++layer;
			}

			_tail = tail;
			_root = currentLayer.Dequeue();
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