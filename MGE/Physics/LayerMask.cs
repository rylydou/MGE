using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MGE
{
	[DataContract]
	public class LayerMask : IEnumerable<string>
	{
		[DataMember] List<string> layers;

		public LayerMask() => layers = new List<string>();
		public LayerMask(params string[] layers) => this.layers = new List<string>(layers);

		public bool Add(string layer)
		{
			if (layers.Contains(layer)) return false;
			layers.Add(layer);
			return true;
		}

		public void Remove(string layer) => layers.Remove(layer);

		public IEnumerator<string> GetEnumerator() => layers.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => layers.GetEnumerator();
	}
}