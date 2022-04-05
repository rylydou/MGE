using Box2D.NetStandard.Dynamics.Fixtures;

namespace MGE;

public abstract class Shape2D : Node2D
{
	// public Fixture fixture;

	protected abstract Fixture GetFixture();
}
