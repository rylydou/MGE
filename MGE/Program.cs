using System;
using MGE;

using (var window = new GameWindow())
{
	window.Run();
}

/* var write = new TestObject()
{
	name = "Name",
	enabled = true,
	visible = false,
	position = Vector2.right,
	priority = -1,
	order = -1,
	hitbox = new Square(12, 14),
};

Folder.dataFolder.FileCreate("object.json").WriteObject(write);

var read = Folder.dataFolder.GetFile("object.json").ReadObject<TestObject>();

Console.WriteLine(write.Equals(read));

class TestObject
{
	[Prop] public string? name;

	[Prop] public bool enabled;
	[Prop] public bool visible;

	[Prop] public Vector2 position;

	[Prop] public int priority;
	[Prop] public float order;

	[Prop] public Shape? hitbox;

	public override bool Equals(object? obj)
	{
		return obj is TestObject @object &&
			name == @object.name &&
			enabled == @object.enabled &&
			visible == @object.visible &&
			priority == @object.priority &&
			order == @object.order;
	}

	public override int GetHashCode() => HashCode.Combine(name, enabled, visible, priority, order);
}

abstract class Shape
{
}

class Square : Shape
{
	[Prop] public float width;
	[Prop] public float height;

	public Square(float width, float height)
	{
		this.width = width;
		this.height = height;
	}
}

class Circle : Shape
{
	[Prop] public float radius;

	public Circle(float radius)
	{
		this.radius = radius;
	}
}
 */
