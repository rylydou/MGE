using MGE;

namespace Demo;

public abstract class GameScreen
{
	public abstract void Start();
	public abstract void Update(float delta);
	public abstract void Tick(float delta);
	public abstract void Render(Batch2D batch);
}
