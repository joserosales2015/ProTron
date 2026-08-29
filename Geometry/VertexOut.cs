using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public readonly struct VertexOut
	{
		// Posición en pantalla
		public Vector2 Position { get; }

		// Profundidad respecto a la cámara
		public float Depth { get; }

		public VertexOut()
		{
		}

		public VertexOut(Vector2 screenPosition, float depth)
		{
			Position = screenPosition;
			Depth = depth;
		}

	}
}
