namespace ProTron.Graphics
{
	public enum TextureFilterMode
	{
		Nearest,
		Bilinear,
		Trilinear
	}

	public enum TextureAddressMode
	{
		Clamp,
		Repeat,
		Mirror
	}

	public readonly record struct TextureSampler(
		TextureFilterMode Filter,
		TextureAddressMode AddressU,
		TextureAddressMode AddressV)
	{
		public static TextureSampler Default { get; } = new(
			TextureFilterMode.Nearest,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp);

		public static TextureSampler PixelArt { get; } = new(
			TextureFilterMode.Nearest,
			TextureAddressMode.Clamp,
			TextureAddressMode.Clamp);
	}
}
