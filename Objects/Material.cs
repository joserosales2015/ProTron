using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Objects
{
	public enum MaterialBlendMode
	{
		Opaque,
		Cutout,
		AlphaBlend
	}

	public class Material
	{
		public uint Color { get; set; }

		public string? TexturePath { get; set; }

		public ProTron.Graphics.Texture? Texture { get; set; }

		public ProTron.Graphics.TextureSampler Sampler { get; set; }
			= ProTron.Graphics.TextureSampler.Default;

		public MaterialBlendMode BlendMode { get; set; }
			= MaterialBlendMode.Opaque;

		public byte AlphaCutoff { get; set; } = 128;

		public Material(uint color)
		{
			Color = color;
		}
	}
}
