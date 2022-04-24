using System.Collections.Generic;

namespace MGE;

public class Scene : Node
{
	public List<Body2D> bodies = new();

	public Scene()
	{
		name = "(Scene root)";
	}

	protected override void OnChildAddedDeep(Node node)
	{
		if (node is Body2D body)
			bodies.Add(body);
	}

	protected override void OnChildRemovedDeep(Node node)
	{
		if (node is Body2D body)
			bodies.Remove(body);
	}
}
