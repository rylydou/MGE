namespace MGE;

public static class Ease
{
	public delegate float Easer(float t);

	public static readonly Easer linear = (float t) => { return t; };

	public static readonly Easer sineIn = (float t) => { return -Mathf.Cos(Mathf.HALF_PI * t) + 1; };
	public static readonly Easer sineOut = (float t) => { return Mathf.Sin(Mathf.HALF_PI * t); };
	public static readonly Easer sineInOut = (float t) => { return -Mathf.Cos(Mathf.HALF_PI * t) / 2f + .5f; };

	public static readonly Easer quadIn = (float t) => { return t * t; };
	public static readonly Easer quadOut = Invert(quadIn);
	public static readonly Easer quadInOut = Follow(quadIn, quadOut);

	public static readonly Easer cubeIn = (float t) => { return t * t * t; };
	public static readonly Easer cubeOut = Invert(cubeIn);
	public static readonly Easer cubeInOut = Follow(cubeIn, cubeOut);

	public static readonly Easer quintIn = (float t) => { return t * t * t * t * t; };
	public static readonly Easer quintOut = Invert(quintIn);
	public static readonly Easer quintInOut = Follow(quintIn, quintOut);

	public static readonly Easer expoIn = (float t) => { return Mathf.Pow(2, 10 * (t - 1)); };
	public static readonly Easer expoOut = Invert(expoIn);
	public static readonly Easer expoInOut = Follow(expoIn, expoOut);

	public static readonly Easer backIn = (float t) => { return t * t * (2.70158f * t - 1.70158f); };
	public static readonly Easer backOut = Invert(backIn);
	public static readonly Easer backInOut = Follow(backIn, backOut);

	public static readonly Easer bigBackIn = (float t) => { return t * t * (4f * t - 3f); };
	public static readonly Easer bigBackOut = Invert(bigBackIn);
	public static readonly Easer bigBackInOut = Follow(bigBackIn, bigBackOut);

	public static readonly Easer elasticIn = (float t) =>
	{
		var ts = t * t;
		var tc = ts * t;
		return (33 * tc * ts + -59 * ts * ts + 32 * tc + -5 * ts);
	};
	public static readonly Easer elasticOut = (float t) =>
	{
		var ts = t * t;
		var tc = ts * t;
		return (33 * tc * ts + -106 * ts * ts + 126 * tc + -67 * ts + 15 * t);
	};
	public static readonly Easer elasticInOut = Follow(elasticIn, elasticOut);

	private const float B1 = 1f / 2.75f;
	private const float B2 = 2f / 2.75f;
	private const float B3 = 1.5f / 2.75f;
	private const float B4 = 2.5f / 2.75f;
	private const float B5 = 2.25f / 2.75f;
	private const float B6 = 2.625f / 2.75f;

	public static readonly Easer bounceIn = (float t) =>
	{
		t = 1 - t;
		if (t < B1)
			return 1 - 7.5625f * t * t;
		if (t < B2)
			return 1 - (7.5625f * (t - B3) * (t - B3) + .75f);
		if (t < B4)
			return 1 - (7.5625f * (t - B5) * (t - B5) + .9375f);
		return 1 - (7.5625f * (t - B6) * (t - B6) + .984375f);
	};

	public static readonly Easer bounceOut = (float t) =>
	{
		if (t < B1)
			return 7.5625f * t * t;
		if (t < B2)
			return 7.5625f * (t - B3) * (t - B3) + .75f;
		if (t < B4)
			return 7.5625f * (t - B5) * (t - B5) + .9375f;
		return 7.5625f * (t - B6) * (t - B6) + .984375f;
	};

	public static readonly Easer bounceInOut = (float t) =>
	{
		if (t < .5f)
		{
			t = 1 - t * 2;
			if (t < B1)
				return (1 - 7.5625f * t * t) / 2;
			if (t < B2)
				return (1 - (7.5625f * (t - B3) * (t - B3) + .75f)) / 2;
			if (t < B4)
				return (1 - (7.5625f * (t - B5) * (t - B5) + .9375f)) / 2;
			return (1 - (7.5625f * (t - B6) * (t - B6) + .984375f)) / 2;
		}
		t = t * 2 - 1;
		if (t < B1)
			return (7.5625f * t * t) / 2 + .5f;
		if (t < B2)
			return (7.5625f * (t - B3) * (t - B3) + .75f) / 2 + .5f;
		if (t < B4)
			return (7.5625f * (t - B5) * (t - B5) + .9375f) / 2 + .5f;
		return (7.5625f * (t - B6) * (t - B6) + .984375f) / 2 + .5f;
	};

	public static Easer Invert(Easer easer)
	{
		return (float t) => { return 1 - easer(1 - t); };
	}

	public static Easer Follow(Easer first, Easer second)
	{
		return (float t) => { return (t <= 0.5f) ? first(t * 2) / 2 : second(t * 2 - 1) / 2 + 0.5f; };
	}

	public static float UpDown(float eased)
	{
		if (eased <= .5f)
			return eased * 2;
		else
			return 1 - (eased - .5f) * 2;
	}
}
