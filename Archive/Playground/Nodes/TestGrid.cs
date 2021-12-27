using MGE;

namespace Playground.Nodes
{
	public class TestGrid : CollidableNode
	{
		public Vector2Int mapSize = new Vector2Int(80, 45);
		// public Vector2Int mapSize = new Vector2Int(40, 23);

		GridCollider _grid;
		Camera _camera;

		public TestGrid()
		{
			_grid = new GridCollider(mapSize, 0.5f);
			SetCollider(_grid);
			for (int y = _grid.cellsY - 3; y < _grid.cellsY; y++)
				for (int x = 0; x < _grid.cellsX; x++)
					_grid.data[x, y] = true;
			layer = new LayerMask("Wall");
		}

		protected override void Init()
		{
			_camera = GetParentNode<Camera>();

			base.Init();
		}

		protected override void Update()
		{
			if (Input.IsButtonHeld(Buttons.Mouse_Left))
			{
				var mousePos = _camera.WindowToCamera(Input.mousePosition);
				_grid.data[(int)(mousePos.x * 2), (int)(mousePos.y * 2)] = true;
			}
			else if (Input.IsButtonHeld(Buttons.Mouse_Right))
			{
				var mousePos = _camera.WindowToCamera(Input.mousePosition);
				_grid.data[(int)(mousePos.x * 2), (int)(mousePos.y * 2)] = false;
			}

			base.Update();
		}
	}
}