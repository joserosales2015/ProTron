using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Graphics
{
	public class Viewport
	{
		public int Width { get; }

		public int Height { get; }

		public float CenterX => Width / 2f;

		public float CenterY => Height / 2f;

		public float AspectRatio => (float)Width / Height;

		public Viewport(int width, int height)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));

			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			Width = width;
			Height = height;
		}
	}
}
