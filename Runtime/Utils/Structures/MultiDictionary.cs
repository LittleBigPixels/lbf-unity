using System.Collections.Generic;
using System.Linq;

namespace LBF.Structures {
	public class MultiDictionary<TKey, TValue> {
		public IEnumerable<TKey> Keys { get { return m_lists.Keys; } }

		readonly Dictionary<TKey, List<TValue>> m_lists;

		public IEnumerable<TValue> this[ TKey key ] {
			get {
				if (m_lists.ContainsKey( key )) return m_lists[key];
				return Enumerable.Empty<TValue>();
			}
		}

		public MultiDictionary() {
			m_lists = new Dictionary<TKey, List<TValue>>();
		}

		public void Add( TKey key, TValue value ) {
			List<TValue> list = GetOrCreateList( key );
			list.Add( value );
		}

		public void AddKey( TKey key ) {
			GetOrCreateList( key );
		}

		public void Remove( TKey key, TValue value ) {
			m_lists[key].Remove( value );
		}

		public void RemoveKey( TKey key ) {
			m_lists[key].Clear();
			m_lists.Remove( key );
		}

		public void Clear() {
			m_lists.Clear();
		}

		public void Clear( TKey key ) {
			m_lists[key].Clear();
		}

		public bool ContainsKey( TKey key ) {
			return m_lists.ContainsKey( key );
		}

		public bool Contains( TKey key, TValue value ) {
			return this[key].Contains( value );
		}

		List<TValue> GetOrCreateList( TKey key ) {
			if (!m_lists.ContainsKey( key ))
				m_lists[key] = new List<TValue>();

			return m_lists[key];
		}
	}
}