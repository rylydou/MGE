using MGE;
using Playground.Nodes;

namespace Playground
{
	public class Main : Microsoft.Xna.Framework.Game
	{
		public Main()
		{
			Engine.Setup(this, new Config());

			Assets.RegisterAssetType(new TextureLoader(), ".png");
			Assets.RegisterAssetType(new FontLoader(), ".font.png");
			Assets.RegisterAssetType(new NodeLoader(), ".node.xml");
		}

		protected override void Initialize()
		{
			Engine.Init();

			Hierarchy.root.AttachNode(
				new PixelCamera(new Vector2Int(320, 180) * 2, 16).AttachNode(
					new PhysicsWorld().AttachNode(
						new Layer().AttachNode(
							new TestGrid(),
							new Player().AttachNode(
								new CollidableNode(new HitboxCollider(new Vector2(0.5f, 1f / 16), new Vector2(0, 1))) { name = "Ground Check" },
								new SpriteRenderer("Icon 16px")
							)
						)
					)
				)
			);

			base.Initialize();
		}

		protected override void LoadContent()
		{
			Engine.Load();
		}

		protected override void UnloadContent()
		{
			Engine.Unload();
		}

		protected override void Update(Microsoft.Xna.Framework.GameTime gameTime)
		{
			Engine.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(Microsoft.Xna.Framework.GameTime gameTime)
		{
			Engine.Draw(gameTime);

			base.Draw(gameTime);
		}
	}
}
