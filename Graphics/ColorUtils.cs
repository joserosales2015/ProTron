using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Graphics
{
	public static class ColorUtils
	{
		public static uint Shade(uint color, float intensity)
		{
			byte a = (byte)((color >> 24) & 0xFF);
			byte r = (byte)((color >> 16) & 0xFF);
			byte g = (byte)((color >> 8) & 0xFF);
			byte b = (byte)(color & 0xFF);

			r = (byte)(r * intensity);
			g = (byte)(g * intensity);
			b = (byte)(b * intensity);

			return ((uint)a << 24) |
				   ((uint)r << 16) |
				   ((uint)g << 8) |
				   b;
		}
	}
}
