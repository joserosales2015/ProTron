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

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint MultiplyRgbOpaque(uint first, uint second)
		{
			byte r = (byte)((first & 0xFF) * (second & 0xFF) / 255);
			byte g = (byte)(((first >> 8) & 0xFF) * ((second >> 8) & 0xFF) / 255);
			byte b = (byte)(((first >> 16) & 0xFF) * ((second >> 16) & 0xFF) / 255);

			return PackRgba(r, g, b, 255);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint WithAlpha(uint color, byte alpha)
		{
			return (color & 0x00FFFFFFu) | ((uint)alpha << 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static byte GetAlpha(uint color)
		{
			return (byte)(color >> 24);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Lerp(uint first, uint second, float amount)
		{
			amount = System.Math.Clamp(amount, 0f, 1f);
			float inverse = 1f - amount;

			byte r = (byte)(((first & 0xFF) * inverse) + ((second & 0xFF) * amount));
			byte g = (byte)((((first >> 8) & 0xFF) * inverse) + (((second >> 8) & 0xFF) * amount));
			byte b = (byte)((((first >> 16) & 0xFF) * inverse) + (((second >> 16) & 0xFF) * amount));
			byte a = (byte)((((first >> 24) & 0xFF) * inverse) + (((second >> 24) & 0xFF) * amount));

			return PackRgba(r, g, b, a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint Bilerp(
			uint c00,
			uint c10,
			uint c01,
			uint c11,
			float tx,
			float ty)
		{
			return LerpPremultiplied(
				LerpPremultiplied(c00, c10, tx),
				LerpPremultiplied(c01, c11, tx),
				ty);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint LerpPremultiplied(uint first, uint second, float amount)
		{
			if ((first & second & 0xFF000000u) == 0xFF000000u)
				return Lerp(first, second, amount);

			amount = System.Math.Clamp(amount, 0f, 1f);
			float inverse = 1f - amount;
			float firstAlpha = (first >> 24) & 0xFF;
			float secondAlpha = (second >> 24) & 0xFF;
			float alpha = firstAlpha * inverse + secondAlpha * amount;

			if (alpha <= 0f)
				return 0;

			float r = ((first & 0xFF) * firstAlpha * inverse +
				(second & 0xFF) * secondAlpha * amount) / alpha;
			float g = (((first >> 8) & 0xFF) * firstAlpha * inverse +
				((second >> 8) & 0xFF) * secondAlpha * amount) / alpha;
			float b = (((first >> 16) & 0xFF) * firstAlpha * inverse +
				((second >> 16) & 0xFF) * secondAlpha * amount) / alpha;

			return PackRgba((byte)r, (byte)g, (byte)b, (byte)alpha);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint AveragePremultiplied(
			uint c00,
			uint c10,
			uint c01,
			uint c11)
		{
			uint a00 = (c00 >> 24) & 0xFF;
			uint a10 = (c10 >> 24) & 0xFF;
			uint a01 = (c01 >> 24) & 0xFF;
			uint a11 = (c11 >> 24) & 0xFF;
			uint alphaSum = a00 + a10 + a01 + a11;

			if (alphaSum == 0)
				return 0;

			uint r = ((c00 & 0xFF) * a00 + (c10 & 0xFF) * a10 +
				(c01 & 0xFF) * a01 + (c11 & 0xFF) * a11 + alphaSum / 2) / alphaSum;
			uint g = (((c00 >> 8) & 0xFF) * a00 + ((c10 >> 8) & 0xFF) * a10 +
				((c01 >> 8) & 0xFF) * a01 + ((c11 >> 8) & 0xFF) * a11 + alphaSum / 2) / alphaSum;
			uint b = (((c00 >> 16) & 0xFF) * a00 + ((c10 >> 16) & 0xFF) * a10 +
				((c01 >> 16) & 0xFF) * a01 + ((c11 >> 16) & 0xFF) * a11 + alphaSum / 2) / alphaSum;
			uint a = (alphaSum + 2) / 4;

			return PackRgba((byte)r, (byte)g, (byte)b, (byte)a);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static uint AlphaBlend(uint source, uint destination)
		{
			uint sourceAlpha = (source >> 24) & 0xFF;

			if (sourceAlpha == 0)
				return destination;

			if (sourceAlpha == 255)
				return source;

			uint inverseAlpha = 255 - sourceAlpha;
			uint r = ((source & 0xFF) * sourceAlpha + (destination & 0xFF) * inverseAlpha + 127) / 255;
			uint g = (((source >> 8) & 0xFF) * sourceAlpha + ((destination >> 8) & 0xFF) * inverseAlpha + 127) / 255;
			uint b = (((source >> 16) & 0xFF) * sourceAlpha + ((destination >> 16) & 0xFF) * inverseAlpha + 127) / 255;
			uint destinationAlpha = (destination >> 24) & 0xFF;
			uint a = sourceAlpha + (destinationAlpha * inverseAlpha + 127) / 255;

			return PackRgba((byte)r, (byte)g, (byte)b, (byte)a);
		}
	}
}
