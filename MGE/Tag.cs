using System;
using System.Collections.Generic;

namespace MGE;

public class Tag
{
	internal static int totalTags = 0;
	internal static Tag[] byID = new Tag[32];
	private static Dictionary<string, Tag> byName = new Dictionary<string, Tag>((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase);
	public int id;
	public int value;
	public string name;

	public static Tag Get(string name) => Tag.byName[name];

	public Tag(string name)
	{
		this.id = Tag.totalTags;
		this.value = 1 << Tag.totalTags;
		this.name = name;
		Tag.byID[this.id] = this;
		Tag.byName[name] = this;
		++Tag.totalTags;
	}

	public static implicit operator int(Tag tag) => tag.value;
}
