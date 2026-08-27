using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Objects
{
	public class Material
	{
		public uint Color { get; set; }

		public Material(uint color)
		{
			Color = color;
		}
	}
}
