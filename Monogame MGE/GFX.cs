using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MGE
{
	public static class GFX
	{
		public static SpriteBatch sb { get; internal set; }
		public static GraphicsDeviceManager gfxManager { get; internal set; }

		public static bool vsync { get => gfxManager.SynchronizeWithVerticalRetrace; set => gfxManager.SynchronizeWithVerticalRetrace = value; }
		public static bool capFps { get => Engine.game.IsFixedTimeStep; set => Engine.game.IsFixedTimeStep = value; }
		public static int maxFps { get => Math.RoundToInt(1f / (float)Engine.game.TargetElapsedTime.TotalSeconds); set => Engine.game.TargetElapsedTime = System.TimeSpan.FromSeconds(1.0 / value); }

		public static int pixelsPerUnit { get; private set; } = 16;

		static Stack<Matrix> _transformStack = new Stack<Matrix>();
		public static Matrix? transform { get; private set; }

		static int _attemptedDraws;
		public static int attemptedDraws { get; private set; }
		static int _realDraws;
		public static int realDraws { get; private set; }
		static int _batches;
		public static int batches { get; private set; }

		static readonly Dictionary<int, Vector2[]> _circleCache = new Dictionary<int, Vector2[]>();
		static Vector2[] GetCircle(float radius, int sides = 16)
		{
			var circleKey = ((byte)radius ^ (byte)sides).GetHashCode();
			if (_circleCache.ContainsKey(circleKey)) return _circleCache[circleKey];

			var points = new List<Vector2>();
			var step = Math.pi2 / sides;

			for (float theta = 0; theta < Math.pi2; theta += step)
				points.Add(new Vector2(radius * Math.Cos(theta), radius * Math.Sin(theta)));
			points.Add(new Vector2(radius * 1, radius * 1));

			var result = points.ToArray();
			_circleCache.Add(circleKey, result);
			return result;
		}

		internal static void Update()
		{
			attemptedDraws = _attemptedDraws;
			_attemptedDraws = 0;
			realDraws = _realDraws;
			_realDraws = 0;
			batches = _batches;
			_batches = 0;
		}

		#region Drawing

		public static void Draw(Texture2D texture, Rect destination, RectInt? source = null, Color? color = null, float rotation = 0, Vector2? origin = null, float depth = 0)
		{
			_attemptedDraws++;
			_realDraws++;
			if (!color.HasValue) color = Color.white;
			if (!origin.HasValue) origin = Vector2.zero;

			// var trueDest = new Rect(
			// 	destination.x * pixelsPerUnit,
			// 	destination.y * pixelsPerUnit,
			// 	destination.width * pixelsPerUnit,
			// 	destination.height * pixelsPerUnit);
			var trueDest = new RectInt(
				Math.RoundToInt(destination.x * pixelsPerUnit),
				Math.RoundToInt(destination.y * pixelsPerUnit),
				Math.RoundToInt(destination.width * pixelsPerUnit),
				Math.RoundToInt(destination.height * pixelsPerUnit));

			sb.Draw(texture, trueDest, source, color.Value, rotation, origin.Value, SpriteEffects.None, depth);
		}

		public static void Draw(Texture2D texture, Vector2 position, RectInt? source = null, Color? color = null, float rotation = 0, Vector2? origin = null, Vector2? scale = null, float depth = 0)
		{
			_attemptedDraws++;
			_realDraws++;
			if (!color.HasValue) color = Color.white;
			if (!origin.HasValue) origin = Vector2.zero;
			if (!scale.HasValue) scale = Vector2.one;

			// var truePos = new Vector2(position.x * pixelsPerUnit, position.y * pixelsPerUnit);
			var truePos = new Vector2Int(Math.RoundToInt(position.x * pixelsPerUnit), Math.RoundToInt(position.y * pixelsPerUnit));

			sb.Draw(texture, truePos, source, color.Value, rotation, origin.Value, scale.Value, SpriteEffects.None, depth);
		}

		public static void DrawPoint(Vector2 position, Color color, float depth = 0)
		{
			Draw(Texture.pixel, position, null, color, depth);
		}

		public static void DrawLine(Vector2 from, Vector2 to, Color color, float thickness = 1, float depth = 0)
		{
			DrawLine(from, Vector2.Distance(from, to), color, Math.Atan2(to.y - from.y, to.x - from.x), thickness, depth);
		}

		public static void DrawLine(Vector2 position, float length, Color color, float angle = 0, float thickness = 1, float depth = 0)
		{
			thickness = thickness / pixelsPerUnit;
			Draw(Texture.pixel, new Rect(position, length, thickness), null, color, angle, Vector2.zero, depth);
		}

		public static void DrawBox(Rect rect, Color color, float depth = 0) => Draw(Texture.pixel, rect, null, color, depth);

		public static void DrawRectOutwards(Rect rect, Color color, float thickness = 1, float depth = 0)
		{
			thickness = thickness / pixelsPerUnit;
			DrawBox(new Rect(rect.x - thickness, rect.y - thickness, rect.width + thickness * 2, thickness), color, depth); // Top
			DrawBox(new Rect(rect.x - thickness, rect.y, thickness, rect.height), color, depth); // Left
			DrawBox(new Rect(rect.x - thickness, rect.y + rect.height, rect.width + thickness * 2, thickness), color, depth); // Bottom
			DrawBox(new Rect(rect.x + rect.width, rect.y, thickness, rect.height), color, depth); // Right
		}

		public static void DrawRectInwards(Rect rect, Color color, float thickness = 1, float depth = 0)
		{
			thickness = thickness / pixelsPerUnit;
			DrawBox(new Rect(rect.x, rect.y, rect.width, thickness), color, depth); // Top
			DrawBox(new Rect(rect.x, rect.y, thickness, rect.height), color, depth); // Left
			DrawBox(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), color, depth); // Bottom
			DrawBox(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), color, depth); // Right
		}

		public static void DrawCircle(Vector2 position, float radius, Color color, float thickness = 1, int sides = 16, float depth = 0)
		{
			DrawPoints(position, GetCircle(radius, sides), color, thickness, depth);
		}

		public static void DrawPoints(Vector2 position, IEnumerable<Vector2> points, Color color, float thickness = 1, float depth = 0)
		{
			DrawPoints(position, points.ToArray(), color, thickness, depth);
		}
		public static void DrawPoints(Vector2 position, Vector2[] points, Color color, float thickness = 1, float depth = 0)
		{
			if (points.Length < 2) return;
			for (int i = 1; i < points.Length; i++)
				DrawLine(points[i - 1] + position, points[i] + position, color, thickness, depth);
		}

		#endregion

		public static void StartTransform(Matrix transform)
		{
			GFX.transform = transform;
			_transformStack.Push(transform);
		}

		public static void EndTransform()
		{
			_transformStack.Pop();
			if (_transformStack.Count < 1) transform = null;
			else transform = _transformStack.Peek();
		}

		public static void StartBatch(bool transparent = true, bool sortDepth = false, int pixlesPerUnit = 1, Effect effect = null)
		{
			_batches++;
			GFX.pixelsPerUnit = pixlesPerUnit;
			GFX.sb.Begin(
				sortDepth ? SpriteSortMode.BackToFront : SpriteSortMode.Deferred,
				transparent ? Engine.config.transparentBlend : Engine.config.opaqueBlend,
				SamplerState.PointClamp,
				DepthStencilState.None,
				RasterizerState.CullNone,
				effect,
				transform
			);
		}

		public static void EndBatch()
		{
			GFX.sb.End();
			GFX.pixelsPerUnit = 1;
		}
	}
}