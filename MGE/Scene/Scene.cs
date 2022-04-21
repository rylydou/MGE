using System.Collections.Generic;

namespace MGE;

public class Scene : Node
{
	public List<Body2D> bodies = new();

	public Scene()
	{
		scene = this;
	}

	protected override void OnChildAddedDeep(Node node)
	{
		Log.Info("Added deep " + node);
		if (node is Body2D body)
			bodies.Add(body);
	}

	protected override void OnChildRemovedDeep(Node node)
	{
		Log.Info("Removed deep " + node);
		if (node is Body2D body)
			bodies.Remove(body);
	}
}
