using System.Runtime.CompilerServices;

namespace ProTron.Graphics
{
	public static class ColorUtils
	{
		// En un uint little-endian, AABBGGRR produce bytes RGBA en memoria.
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint PackRgba(byte r, byte g, byte b, byte a = 255)
		{
			return r |
				   ((uint)g << 8) |
				   ((uint)b << 16) |
				   ((uint)a << 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Multiply(uint first, uint second)
		{
			byte r = (byte)((first & 0xFF) * (second & 0xFF) / 255);
			byte g = (byte)(((first >> 8) & 0xFF) * ((second >> 8) & 0xFF) / 255);
			byte b = (byte)(((first >> 16) & 0xFF) * ((second >> 16) & 0xFF) / 255);
			byte a = (byte)(((first >> 24) & 0xFF) * ((second >> 24) & 0xFF) / 255);

			return PackRgba(r, g, b, a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Shade(uint color, float intensity)
		{
			intensity = System.Math.Clamp(intensity, 0f, 1f);

			byte r = (byte)((color & 0xFF) * intensity);
			byte g = (byte)(((color >> 8) & 0xFF) * intensity);
			byte b = (byte)(((color >> 16) & 0xFF) * intensity);
			byte a = (byte)((color >> 24) & 0xFF);

			return PackRgba(r, g, b, a);
		}
	}
}
