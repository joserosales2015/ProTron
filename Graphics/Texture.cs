using System.Runtime.CompilerServices;

namespace ProTron.Graphics
{
	public sealed class Texture
	{
		private readonly MipLevel[] _mipLevels;

		private readonly struct MipLevel
		{
			public int Width { get; }
			public int Height { get; }
			public uint[] Pixels { get; }

			public MipLevel(int width, int height, uint[] pixels)
			{
				Width = width;
				Height = height;
				Pixels = pixels;
			}
		}

		public int Width { get; }
		public int Height { get; }
		public int MipCount => _mipLevels.Length;
		public int MaxMipLevel => _mipLevels.Length - 1;

		public Texture(int width, int height, uint[] pixels)
		{
			ArgumentNullException.ThrowIfNull(pixels);

			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));

			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			int pixelCount = checked(width * height);

			if (pixels.Length != pixelCount)
				throw new ArgumentException(
					"La cantidad de píxeles no coincide con el tamaño de la textura.",
					nameof(pixels));

			Width = width;
			Height = height;
			_mipLevels = BuildMipChain(width, height, pixels);
		}

		public uint SampleNearest(float u, float v)
		{
			return Sample(u, v, TextureSampler.PixelArt, 0f);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal uint SampleNearestUnchecked(
			float u,
			float v,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			if (addressU == TextureAddressMode.Clamp &&
				addressV == TextureAddressMode.Clamp)
			{
				MipLevel mip = _mipLevels[0];
				u = System.Math.Clamp(u, 0f, 1f);
				v = System.Math.Clamp(v, 0f, 1f);
				int x = System.Math.Min((int)(u * mip.Width), mip.Width - 1);
				int y = System.Math.Min((int)(v * mip.Height), mip.Height - 1);

				return mip.Pixels[y * mip.Width + x];
			}

			return SampleNearestLevel(0, u, v, addressU, addressV);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal uint SampleBilinearUnchecked(
			float u,
			float v,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			return SampleBilinearLevel(0, u, v, addressU, addressV);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal uint SampleTrilinearUnchecked(
			float u,
			float v,
			float mipLevel,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			return SampleTrilinear(u, v, mipLevel, addressU, addressV);
		}

		public uint Sample(
			float u,
			float v,
			in TextureSampler sampler,
			float mipLevel = 0f)
		{
			if (!float.IsFinite(u) || !float.IsFinite(v))
				return ColorUtils.PackRgba(255, 0, 255);

			return sampler.Filter switch
			{
				TextureFilterMode.Nearest => SampleNearestLevel(
					0,
					u,
					v,
					sampler.AddressU,
					sampler.AddressV),
				TextureFilterMode.Bilinear => SampleBilinearLevel(
					0,
					u,
					v,
					sampler.AddressU,
					sampler.AddressV),
				TextureFilterMode.Trilinear => SampleTrilinear(
					u,
					v,
					mipLevel,
					sampler.AddressU,
					sampler.AddressV),
				_ => throw new ArgumentOutOfRangeException(nameof(sampler))
			};
		}

		private uint SampleNearestLevel(
			int level,
			float u,
			float v,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			MipLevel mip = _mipLevels[level];
			u = AddressCoordinate(u, addressU);
			v = AddressCoordinate(v, addressV);

			int x = System.Math.Min((int)(u * mip.Width), mip.Width - 1);
			int y = System.Math.Min((int)(v * mip.Height), mip.Height - 1);

			return mip.Pixels[y * mip.Width + x];
		}

		private uint SampleBilinearLevel(
			int level,
			float u,
			float v,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			MipLevel mip = _mipLevels[level];
			u = AddressCoordinate(u, addressU);
			v = AddressCoordinate(v, addressV);

			float texelX = u * mip.Width - 0.5f;
			float texelY = v * mip.Height - 0.5f;
			int x0 = (int)MathF.Floor(texelX);
			int y0 = (int)MathF.Floor(texelY);
			float tx = texelX - x0;
			float ty = texelY - y0;

			uint c00 = GetTexel(mip, x0, y0, addressU, addressV);
			uint c10 = GetTexel(mip, x0 + 1, y0, addressU, addressV);
			uint c01 = GetTexel(mip, x0, y0 + 1, addressU, addressV);
			uint c11 = GetTexel(mip, x0 + 1, y0 + 1, addressU, addressV);

			return ColorUtils.Bilerp(c00, c10, c01, c11, tx, ty);
		}

		private uint SampleTrilinear(
			float u,
			float v,
			float mipLevel,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			mipLevel = float.IsFinite(mipLevel)
				? System.Math.Clamp(mipLevel, 0f, MaxMipLevel)
				: 0f;

			int firstLevel = (int)MathF.Floor(mipLevel);
			int secondLevel = System.Math.Min(firstLevel + 1, MaxMipLevel);
			float amount = mipLevel - firstLevel;

			uint first = SampleBilinearLevel(
				firstLevel, u, v, addressU, addressV);

			if (firstLevel == secondLevel)
				return first;

			uint second = SampleBilinearLevel(
				secondLevel, u, v, addressU, addressV);

			return ColorUtils.LerpPremultiplied(first, second, amount);
		}

		private static MipLevel[] BuildMipChain(
			int width,
			int height,
			uint[] sourcePixels)
		{
			int levelCount = 1;
			int levelWidth = width;
			int levelHeight = height;

			while (levelWidth > 1 || levelHeight > 1)
			{
				levelWidth = System.Math.Max(1, levelWidth / 2 + levelWidth % 2);
				levelHeight = System.Math.Max(1, levelHeight / 2 + levelHeight % 2);
				levelCount++;
			}

			MipLevel[] levels = new MipLevel[levelCount];
			levels[0] = new MipLevel(width, height, (uint[])sourcePixels.Clone());

			for (int level = 1; level < levelCount; level++)
			{
				MipLevel previous = levels[level - 1];
				int nextWidth = System.Math.Max(
					1,
					previous.Width / 2 + previous.Width % 2);
				int nextHeight = System.Math.Max(
					1,
					previous.Height / 2 + previous.Height % 2);
				uint[] nextPixels = new uint[checked(nextWidth * nextHeight)];

				for (int y = 0; y < nextHeight; y++)
				{
					for (int x = 0; x < nextWidth; x++)
					{
						int sourceX = x * 2;
						int sourceY = y * 2;
						uint c00 = previous.Pixels[sourceY * previous.Width + sourceX];
						uint c10 = previous.Pixels[
							sourceY * previous.Width +
							System.Math.Min(sourceX + 1, previous.Width - 1)];
						uint c01 = previous.Pixels[
							System.Math.Min(sourceY + 1, previous.Height - 1) * previous.Width +
							sourceX];
						uint c11 = previous.Pixels[
							System.Math.Min(sourceY + 1, previous.Height - 1) * previous.Width +
							System.Math.Min(sourceX + 1, previous.Width - 1)];

						nextPixels[y * nextWidth + x] =
							ColorUtils.AveragePremultiplied(c00, c10, c01, c11);
					}
				}

				levels[level] = new MipLevel(nextWidth, nextHeight, nextPixels);
			}

			return levels;
		}

		private static float AddressCoordinate(
			float coordinate,
			TextureAddressMode mode)
		{
			return mode switch
			{
				TextureAddressMode.Clamp => System.Math.Clamp(coordinate, 0f, 1f),
				TextureAddressMode.Repeat => coordinate - MathF.Floor(coordinate),
				TextureAddressMode.Mirror => MirrorCoordinate(coordinate),
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};
		}

		private static float MirrorCoordinate(float coordinate)
		{
			float wrapped = coordinate % 2f;

			if (wrapped < 0f)
				wrapped += 2f;

			return wrapped <= 1f ? wrapped : 2f - wrapped;
		}

		private static uint GetTexel(
			MipLevel mip,
			int x,
			int y,
			TextureAddressMode addressU,
			TextureAddressMode addressV)
		{
			x = AddressIndex(x, mip.Width, addressU);
			y = AddressIndex(y, mip.Height, addressV);

			return mip.Pixels[y * mip.Width + x];
		}

		private static int AddressIndex(
			int index,
			int size,
			TextureAddressMode mode)
		{
			if (size == 1)
				return 0;

			return mode switch
			{
				TextureAddressMode.Clamp => System.Math.Clamp(index, 0, size - 1),
				TextureAddressMode.Repeat => PositiveModulo(index, size),
				TextureAddressMode.Mirror => MirrorIndex(index, size),
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};
		}

		private static int PositiveModulo(int value, int modulus)
		{
			int result = value % modulus;
			return result < 0 ? result + modulus : result;
		}

		private static int MirrorIndex(int index, int size)
		{
			int period = checked(size * 2);
			int mirrored = PositiveModulo(index, period);
			return mirrored < size ? mirrored : period - mirrored - 1;

		}

		public static Texture LoadFromFile(string path)
		{
			Raylib_cs.Image image = Raylib_cs.Raylib.LoadImage(path);

			try
			{
				if (image.Width <= 0 || image.Height <= 0)
					throw new InvalidOperationException(
						$"No se pudo cargar la textura: {path}");

			uint[] pixels = new uint[checked(image.Width * image.Height)];

				for (int y = 0; y < image.Height; y++)
				{
					for (int x = 0; x < image.Width; x++)
					{
						Raylib_cs.Color color =
							Raylib_cs.Raylib.GetImageColor(image, x, y);

						pixels[y * image.Width + x] =
							ColorUtils.PackRgba(
								color.R,
								color.G,
								color.B,
								color.A);
					}
				}

				return new Texture(image.Width, image.Height, pixels);
			}
			finally
			{
				Raylib_cs.Raylib.UnloadImage(image);
			}
		}

		public static Texture CreateCheckerboard(
			int width,
			int height,
			int cellsPerAxis,
			uint firstColor,
			uint secondColor)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));

			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			if (cellsPerAxis <= 0)
				throw new ArgumentOutOfRangeException(nameof(cellsPerAxis));

			uint[] pixels = new uint[checked(width * height)];

			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int cellX = x * cellsPerAxis / width;
					int cellY = y * cellsPerAxis / height;

					pixels[y * width + x] = (cellX + cellY) % 2 == 0
						? firstColor
						: secondColor;
				}
			}

			return new Texture(width, height, pixels);
		}
	}
}
