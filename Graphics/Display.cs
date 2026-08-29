using ProTron.Buffer;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Graphics
{
	public class Display
	{
		private readonly FrameBuffer _frameBuffer;
		private Texture2D _texture;
		private Rectangle source;
		private Rectangle dest;

		public Display(FrameBuffer frameBuffer)
		{
			_frameBuffer = frameBuffer;
		}

		public void Initialize()
		{
			Image image = Raylib.GenImageColor(
				_frameBuffer.Width,
				_frameBuffer.Height,
				Color.Black);

			_texture = Raylib.LoadTextureFromImage(image);

			Raylib.UnloadImage(image);

			source = new Rectangle(0, 0, _frameBuffer.Width, -_frameBuffer.Height);
			dest = new Rectangle(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			Raylib.SetTextureFilter(
				_texture,
				TextureFilter.Point);
		}

		public void Present()
		{
			// aquí actualizaremos la textura
			// con los píxeles del framebuffer
			Raylib.UpdateTexture(_texture, _frameBuffer.Pixels);
			Raylib.DrawTexturePro(_texture, source, dest, Vector2.Zero, 0.0f, Color.White);
		}

		public void Unload()
		{
			Raylib.UnloadTexture(_texture);
		}
	}
}
