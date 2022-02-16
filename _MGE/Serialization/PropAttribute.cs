namespace MGE;

[System.AttributeUsage(System.AttributeTargets.All, Inherited = true, AllowMultiple = false)]
sealed class PropAttribute : System.Attribute
{
	public string? name = null;
	public readonly int? order = null;
}
