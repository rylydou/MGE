using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	[DataContract]
	public class Layer : Node
	{
		[DataMember] public bool transparent = true;
		[DataMember] public bool sortDepth = false;
		[DataMember] public int pixlesPerUnit = 16;
		[DataMember] public Effect effect = null;

		public Layer() { }

		public Layer(bool transparent, bool sortDepth, int pixlesPerUnit, Effect effect)
		{
			this.transparent = transparent;
			this.sortDepth = sortDepth;
			this.pixlesPerUnit = pixlesPerUnit;
			this.effect = effect;
		}

		protected override void Draw()
		{
			GFX.StartBatch(transparent, sortDepth, pixlesPerUnit, effect);

			base.Draw();

			GFX.EndBatch();
		}
	}
}