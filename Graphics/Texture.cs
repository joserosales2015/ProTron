namespace ProTron.Graphics
{
	public sealed class Texture
	{
		private readonly uint[] _pixels;

		public int Width { get; }
		public int Height { get; }

		public Texture(int width, int height, uint[] pixels)
		{
			if (width <= 0)
				throw new ArgumentOutOfRangeException(nameof(width));

			if (height <= 0)
				throw new ArgumentOutOfRangeException(nameof(height));

			if (pixels.Length != width * height)
				throw new ArgumentException(
					"La cantidad de píxeles no coincide con el tamaño de la textura.",
					nameof(pixels));

			Width = width;
			Height = height;
			_pixels = pixels;
		}

		public uint SampleNearest(float u, float v)
		{
			u = System.Math.Clamp(u, 0f, 0.999999f);
			v = System.Math.Clamp(v, 0f, 0.999999f);

			int x = (int)(u * Width);
			int y = (int)(v * Height);

			return _pixels[y * Width + x];
		}

		public static Texture LoadFromFile(string path)
		{
			Raylib_cs.Image image = Raylib_cs.Raylib.LoadImage(path);

			try
			{
				if (image.Width <= 0 || image.Height <= 0)
					throw new InvalidOperationException(
						$"No se pudo cargar la textura: {path}");

				uint[] pixels = new uint[image.Width * image.Height];

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
			if (cellsPerAxis <= 0)
				throw new ArgumentOutOfRangeException(nameof(cellsPerAxis));

			uint[] pixels = new uint[width * height];

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
