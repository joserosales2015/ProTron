using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Lighting
{
	public class DirectionalLight
	{
		// Dirección en la que viaja la luz, en espacio mundo.
		public Vector3f WorldDirection { get; set; }
			= new Vector3f(0.4f, -1f, 0.3f).Normalized();

		public float Intensity { get; set; } = 0.85f;
	}
}