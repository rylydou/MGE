using MGE.Graphics;

namespace MGE.UI;

public class UIStyle
{
	internal UIStyle? parentStyle = new();

	// Unique
	public Color backgroundColor = Color.transparent;
	public Texture backgroundTexture = Texture.pixelTexture;

	public Color borderColor = Color.transparent;
	public float borderWidth = 0;

	// Inherited
	Optional<Color> _midgroundColor = new();
	public Color midgroundColor { get => _midgroundColor.TryGetValue(() => parentStyle?.midgroundColor ?? throw new MGEException()); }

	Optional<Color> _foregroundColor = new();
	public Color foregroundColor { get => _foregroundColor.TryGetValue(() => parentStyle?.foregroundColor ?? throw new MGEException()); }

	Optional<Font> _font = new();
	public Font font { get => _font.TryGetValue(() => parentStyle?.font ?? throw new MGEException()); }

	Optional<float> _fontSize = new();
	public float fontSize { get => _fontSize.TryGetValue(() => parentStyle?.fontSize ?? throw new MGEException()); }
}
