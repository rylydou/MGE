namespace MGE.Utilities;

public class GUID
{
	public static long next;

	public static GUID New()
	{
		return new GUID(++next);
	}

	public long id;

	public GUID(long id)
	{
		this.id = id;
	}
}
