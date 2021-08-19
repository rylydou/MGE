using System.Collections.Generic;
using MGE;

namespace Playground
{
	public class TestData
	{
		public bool enabled = true;
		public string text = "The quick brown fox jumps over the lazy dog";
		public int number = 69420;
		public Vector2 position = new Vector2(4.25f, 7.4f);
		public List<Shape> shapes = new List<Shape>()
		{
			new Square() { position = new Vector2(-4, 7.5f), width = 69.42f, height = 983.4f },
			new Circle() { position = new Vector2(6.3f, 85.8f), radius = 7.54f }
		};
	}

	public abstract class Shape
	{
		public Vector2 position;
	}

	public class Square : Shape
	{
		public float width;
		public float height;
	}

	public class Circle : Shape
	{
		public float radius;
	}
}

/*
enabled true
visible true
name "hello there"
children
	!MGE.PixelCamera MGE
	enabled true
	visible true
	position 0 0
	rotation 0
	zoom 0
	clearScreen true
	screenClearColor "#122020"
	renderToScreenWithTransparency false
	postProcessEffect null
	renderTint "#FF7504"
	targetResolution 640 360
	pixelsPerUnit 16
	children
		!MGE.PhysicsWorld MGE
		enabled true
		visible true
		gravity 20
		stepsPerUnit 0.015625
		children
			!MGE.Layer MGE
			enabled true
			visible true
			children
				!Playground.Nodes.TestGrid Playground
				enabled true
				visible true
				children
				!Playground.Nodes.Player Playground
				enabled true
				visible true
				children
					!MGE.SpriteRenderer MGE
					enabled true
					visible true
					texture "Players/_Generic/Player"
 */