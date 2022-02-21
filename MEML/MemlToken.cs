﻿namespace MGE
{
	/// <summary>
	/// A Meml Token
	/// </summary>
	public enum MemlToken
	{
		Null = 0,
		ObjectStart = 1,
		ObjectEnd = 2,
		ObjectKey = 3,
		ArrayStart = 4,
		ArrayEnd = 5,
		Boolean = 6,
		String = 7,
		Number = 8,
		Binary = 9,
	}
}
