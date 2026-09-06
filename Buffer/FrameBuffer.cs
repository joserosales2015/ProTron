using ProTron.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Buffer
{
	public class FrameBuffer
	{
		private readonly uint[] _pixels;

		public int Width { get; }

		public int Height { get; }

		public FrameBuffer(int width, int height)
		{
			Width = width;
			Height = height;

			_pixels = new uint[width * height];
		}

		public void Clear(uint color)
		{
			Array.Fill(_pixels, color);
		}

		public uint[] Pixels => _pixels;

		public void PutPixelUnChecked(int x, int y, uint color)
		{
			_pixels[y * Width + x] = color;
		}

		public void PutPixel(int x, int y, uint color)
		{
			if (x < 0 || x >= Width)
				return;

			if (y < 0 || y >= Height)
				return;

			_pixels[y * Width + x] = color;
		}

		public uint GetPixel(int x, int y)
		{
			if (x < 0 || x >= Width)
				return 0;

			if (y < 0 || y >= Height)
				return 0;

			return _pixels[y * Width + x];
		}

		public uint GetPixelUnchecked(int x, int y)
		{
			return _pixels[y * Width + x];
		}

		
	}
}
