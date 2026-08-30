using ProTron.Math;
using System.Numerics;

namespace ProTron.Geometry
{
	public readonly struct Vertex
	{
		public Vector3f Position { get; }
		public Vector3f Normal { get; }
		public Vector2 UV { get; }

		public Vertex(Vector3f position)
			: this(position, Vector3f.Zero, Vector2.Zero)
		{
		}

		public Vertex(float x, float y, float z)
			: this(new Vector3f(x, y, z), Vector3f.Zero, Vector2.Zero)
		{
		}

		public Vertex(Vector3f position, Vector3f normal, Vector2 uv)
		{
			Position = position;
			Normal = normal;
			UV = uv;
		}

		public static Vertex Lerp(Vertex a, Vertex b, float t)
		{
			return new Vertex(
				Vector3f.Lerp(a.Position, b.Position, t),
				Vector3f.Lerp(a.Normal, b.Normal, t),
				Vector2.Lerp(a.UV, b.UV, t));
		}
	}
}
