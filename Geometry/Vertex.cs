using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public readonly struct Vertex
	{
		public Vector3f Position { get; }
		public Vector3f Normal { get; }

		public Vertex(Vector3f position)
			: this(position, Vector3f.Zero)
		{
		}

		public Vertex(float x, float y, float z)
			: this(new Vector3f(x, y, z), Vector3f.Zero)
		{
		}

		public Vertex(Vector3f position, Vector3f normal)
		{
			Position = position;
			Normal = normal;
		}

		public static Vertex Lerp(Vertex a, Vertex b, float t)
		{
			return new Vertex(
				Vector3f.Lerp(a.Position, b.Position, t),
				Vector3f.Lerp(a.Normal, b.Normal, t));
		}
	}
}
