using MGE.Graphics;

namespace MGE.UI;

public class UIStyle
{
	internal UIStyle? parentStyle = new();

	// Not inherited
	public Color backgroundColor { get; set; } = Color.transparent;
	public Texture backgroundTexture { get; set; } = Texture.pixelTexture;

	public Color borderColor { get; set; } = Color.transparent;
	public float borderWidth { get; set; } = 0;

	// Inherited
	Optional<Color> _midgroundColor = new();
	Optional<Color> _foregroundColor = new();

	Optional<Font> _font = new();
	Optional<float> _fontSize = new();

	public Color midgroundColor { get => _midgroundColor.TryGetValue(() => parentStyle?.midgroundColor ?? throw new MGEException()); }
	public Color foregroundColor { get => _foregroundColor.TryGetValue(() => parentStyle?.foregroundColor ?? throw new MGEException()); }

	public Font font { get => _font.TryGetValue(() => parentStyle?.font ?? throw new MGEException()); }
	public float fontSize { get => _fontSize.TryGetValue(() => parentStyle?.fontSize ?? throw new MGEException()); }
}
