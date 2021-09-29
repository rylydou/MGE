using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace MGE.Graphics
{
	public class SpriteBatch
	{
		// // SpriteSortMode _sortMode;
		// BlendMode _blendMode;
		// SpriteBatcher _batcher;
		// Matrix _matrix;

		// public void Begin()
		// {
		// 	// _sortMode = SpriteSortMode.Deferred;
		// 	_blendMode = BlendMode.AlphaBlend;
		// 	_matrix = Matrix.identity;
		// }

		// public void Begin(/* SpriteSortMode sortMode, */ BlendMode blendMode)
		// {
		// 	// _sortMode = sortMode;
		// 	_blendMode = blendMode;
		// 	_matrix = Matrix.identity;
		// }

		// public void Begin(/* SpriteSortMode sortMode, */ BlendMode blendMode, Matrix transformMatrix)
		// {
		// 	_blendMode = blendMode;
		// 	// _sortMode = sortMode;
		// 	_matrix = transformMatrix;
		// }

		// public void End()
		// {
		// 	// set the blend mode
		// 	switch (_blendMode)
		// 	{
		// 		case BlendMode.PreMultiplied:
		// 			GL.Enable(EnableCap.Blend);
		// 			GL.BlendFunc(BlendingFactor.One, BlendingFactor.OneMinusSrcAlpha);
		// 			break;
		// 		case BlendMode.AlphaBlend:
		// 			GL.Enable(EnableCap.Blend);
		// 			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
		// 			break;
		// 		case BlendMode.Additive:
		// 			GL.Enable(EnableCap.Blend);
		// 			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.One);
		// 			break;
		// 		case BlendMode.None:
		// 			GL.Disable(EnableCap.Blend);
		// 			break;
		// 	}

		// 	// set camera
		// 	GL.MatrixMode(MatrixMode.Projection);
		// 	GL.LoadIdentity();

		// 	// GL.Ortho(0, _graphicsDevice.Viewport.width, _graphicsDevice.Viewport.height, 0, -1, 1);

		// 	GL.MatrixMode(MatrixMode.Modelview);
		// 	GL.LoadMatrix(ref _matrix.m11);

		// 	// GL.Viewport(0, 0, _graphicsDevice.Viewport.width, _graphicsDevice.Viewport.height);

		// 	// Initialize OpenGL states (ideally move this to initialize somewhere else)
		// 	GL.Disable(EnableCap.DepthTest);
		// 	GL.TexEnv(TextureEnvTarget.TextureEnv, TextureEnvParameter.TextureEnvMode, (int)All.BlendSrc);
		// 	GL.Enable(EnableCap.Texture2D);
		// 	GL.EnableClientState(ArrayCap.VertexArray);
		// 	GL.EnableClientState(ArrayCap.ColorArray);
		// 	GL.EnableClientState(ArrayCap.TextureCoordArray);

		// 	// Enable Culling for better performance
		// 	GL.Enable(EnableCap.CullFace);
		// 	GL.FrontFace(FrontFaceDirection.Cw);
		// 	GL.Color4(Color.white);

		// 	_batcher.DrawBatch(/* _sortMode */);
		// }

		// public void Draw(Texture texture, Vector2 position, Rect? sourceRectangle, Color color, float rotation, Vector2 origin, Vector2 scale, /* SpriteEffects effect, */ float depth)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = depth;
		// 	item.texture = texture;

		// 	Rect rect;
		// 	if (sourceRectangle.HasValue) rect = sourceRectangle.Value;
		// 	else rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);

		// 	// if (effect == SpriteEffects.FlipVertically)
		// 	// {
		// 	// 	var temp = texCoordBR.y;
		// 	// 	texCoordBR.y = texCoordTL.y;
		// 	// 	texCoordTL.y = temp;
		// 	// }
		// 	// else if (effect == SpriteEffects.FlipHorizontally)
		// 	// {
		// 	// 	var temp = texCoordBR.x;
		// 	// 	texCoordBR.x = texCoordTL.x;
		// 	// 	texCoordTL.x = temp;
		// 	// }

		// 	item.Set(position.x, position.y, -origin.x * scale.x, -origin.y * scale.y, rect.width * scale.x, rect.height * scale.y, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Vector2 position, Nullable<Rect> sourceRectangle, Color color, float rotation, Vector2 origin, float scale, /* SpriteEffects effect, */ float depth)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = depth;
		// 	item.texture = texture;

		// 	Rect rect;
		// 	if (sourceRectangle.HasValue) rect = sourceRectangle.Value;
		// 	else rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);

		// 	// if (effect == SpriteEffects.FlipVertically)
		// 	// {
		// 	// 	var temp = texCoordBR.y;
		// 	// 	texCoordBR.y = texCoordTL.y;
		// 	// 	texCoordTL.y = temp;
		// 	// }
		// 	// else if (effect == SpriteEffects.FlipHorizontally)
		// 	// {
		// 	// 	var temp = texCoordBR.x;
		// 	// 	texCoordBR.x = texCoordTL.x;
		// 	// 	texCoordTL.x = temp;
		// 	// }
		// 	item.Set(position.x, position.y, -origin.x * scale, -origin.y * scale, rect.width * scale, rect.height * scale, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Rect destinationRectangle, Nullable<Rect> sourceRectangle, Color color, float rotation, Vector2 origin, /* SpriteEffects effect, */ float depth)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = depth;
		// 	item.texture = texture;

		// 	Rect rect;
		// 	if (sourceRectangle.HasValue)
		// 		rect = sourceRectangle.Value;
		// 	else
		// 		rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);
		// 	// if (effect == SpriteEffects.FlipVertically)
		// 	// {
		// 	// 	var temp = texCoordBR.y;
		// 	// 	texCoordBR.y = texCoordTL.y;
		// 	// 	texCoordTL.y = temp;
		// 	// }
		// 	// else if (effect == SpriteEffects.FlipHorizontally)
		// 	// {
		// 	// 	var temp = texCoordBR.x;
		// 	// 	texCoordBR.x = texCoordTL.x;
		// 	// 	texCoordTL.x = temp;
		// 	// }

		// 	item.Set(destinationRectangle.x, destinationRectangle.y, -origin.x, -origin.y, destinationRectangle.width, destinationRectangle.height, (float)Math.Sin(rotation), (float)Math.Cos(rotation), color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Vector2 position, Rect? sourceRectangle, Color color)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = 0.0f;
		// 	item.texture = texture;

		// 	Rect rect;
		// 	if (sourceRectangle.HasValue)
		// 		rect = sourceRectangle.Value;
		// 	else
		// 		rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);

		// 	item.Set(position.x, position.y, rect.width, rect.height, color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Rect destinationRectangle, Rect? sourceRectangle, Color color)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = 0;
		// 	item.texture = texture;

		// 	Rect rect;
		// 	if (sourceRectangle.HasValue)
		// 		rect = sourceRectangle.Value;
		// 	else
		// 		rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);

		// 	item.Set(destinationRectangle.x, destinationRectangle.y, destinationRectangle.width, destinationRectangle.height, color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Vector2 position, Color color)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.depth = 0;
		// 	item.texture = texture;

		// 	var rect = new Rect(0, 0, texture.width, texture.height);

		// 	var texCoordTL = texture.GetTextureCoord(rect.x, rect.y);
		// 	var texCoordBR = texture.GetTextureCoord(rect.x + rect.width, rect.y + rect.height);

		// 	item.Set(position.x, position.y, rect.width, rect.height, color, texCoordTL, texCoordBR);
		// }

		// public void Draw(Texture texture, Rect rectangle, Color color)
		// {
		// 	var item = _batcher.CreateBatchItem();

		// 	item.Depth = 0;
		// 	item.texture = texture;

		// 	var texCoordTL = texture.GetTextureCoord(0, 0);
		// 	var texCoordBR = texture.GetTextureCoord(texture.width, texture.height);

		// 	item.Set(rectangle.x, rectangle.y, rectangle.width, rectangle.height, color, texCoordTL, texCoordBR);
		// }
	}
}
