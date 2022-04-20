using System.Collections.Generic;

namespace MGE;

public class Scene : Node
{
	public List<CollisionObject2D> collisionObjects = new();
	public

	public Scene()
	{
		scene = this;
	}

	protected override void OnChildAddedDeep(Node node)
	{
		if (node is CollisionObject2D phy)
			collisionObjects.Add(phy);
	}

	protected override void OnChildRemovedDeep(Node node)
	{
		if (node is CollisionObject2D phy)
			collisionObjects.Remove(phy);
	}
}

public class TagLists
{
	private List<Node>[] lists;
	private bool[] unsorted;
	private bool areAnyUnsorted;

	internal TagLists()
	{
		this.lists = new List<Node>[Tag.totalTags];
		this.unsorted = new bool[Tag.totalTags];
		for (int index = 0; index < this.lists.Length; ++index)
			this.lists[index] = new List<Node>();
	}

	public List<Node> this[int index] => this.lists[index];

	internal void MarkUnsorted(int index)
	{
		this.areAnyUnsorted = true;
		this.unsorted[index] = true;
	}

	internal void UpdateLists()
	{
		if (!this.areAnyUnsorted)
			return;
		for (int index = 0; index < this.lists.Length; ++index)
		{
			if (this.unsorted[index])
			{
				this.lists[index].Sort(EntityList.CompareDepth);
				this.unsorted[index] = false;
			}
		}
		this.areAnyUnsorted = false;
	}

	internal void EntityAdded(Node entity)
	{
		for (int index = 0; index < Tag.totalTags; ++index)
		{
			if (entity.TagCheck(1 << index))
			{
				this[index].Add(entity);
				this.areAnyUnsorted = true;
				this.unsorted[index] = true;
			}
		}
	}

	internal void EntityRemoved(Node entity)
	{
		for (int index = 0; index < Tag.totalTags; ++index)
		{
			if (entity.TagCheck(1 << index))
				this.lists[index].Remove(entity);
		}
	}
}
