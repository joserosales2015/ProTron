using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public class Vertex
	{
		public Vector3f Position;

		public Vector3f Normal { get; set; }

		public Vertex(Vector3f position)
		{
			Position = position;
		}

		public Vertex(float x, float y, float z)
		{
			Position = new Vector3f(x, y, z);
		}

		public static Vertex Lerp(Vertex a, Vertex b, float t)
		{
			return new Vertex(Vector3f.Lerp(a.Position, b.Position, t));
		}
	}
}
