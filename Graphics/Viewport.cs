using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Graphics
{
	public class Viewport
	{
		public int Width { get; set; }

		public int Height { get; set; }

		public float CenterX => Width / 2f;

		public float CenterY => Height / 2f;

		public float AspectRatio => (float)Width / Height;

		public Viewport(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}
}
