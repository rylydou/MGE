
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	public class Layer : Node
	{
		[Prop] public bool transparent = true;
		[Prop] public bool sortDepth = false;
		[Prop] public int pixlesPerUnit = 16;
		[Prop] public Effect effect = null;

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
