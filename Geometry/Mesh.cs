using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public class Mesh
	{
		private BoundingSphere _boundingSphere;
		private int _boundsVertexCount = -1;

		public List<Vertex> Vertices { get; } = new();

		public List<Edge> Edges { get; } = new();

		public List<Triangle> Triangles { get; } = new();

		public BoundingSphere GetBoundingSphere()
		{
			if (_boundsVertexCount != Vertices.Count)
				RecalculateBounds();

			return _boundingSphere;
		}

		public void RecalculateBounds()
		{
			if (Vertices.Count == 0)
			{
				_boundingSphere = new BoundingSphere(Math.Vector3f.Zero, 0f);
				_boundsVertexCount = 0;
				return;
			}

			Math.Vector3f minimum = Vertices[0].Position;
			Math.Vector3f maximum = Vertices[0].Position;

			for (int i = 1; i < Vertices.Count; i++)
			{
				Math.Vector3f position = Vertices[i].Position;
				minimum.X = MathF.Min(minimum.X, position.X);
				minimum.Y = MathF.Min(minimum.Y, position.Y);
				minimum.Z = MathF.Min(minimum.Z, position.Z);
				maximum.X = MathF.Max(maximum.X, position.X);
				maximum.Y = MathF.Max(maximum.Y, position.Y);
				maximum.Z = MathF.Max(maximum.Z, position.Z);
			}

			Math.Vector3f center = (minimum + maximum) * 0.5f;
			float radiusSquared = 0f;

			foreach (Vertex vertex in Vertices)
			{
				float distanceSquared = (vertex.Position - center).LengthSquared;
				radiusSquared = MathF.Max(radiusSquared, distanceSquared);
			}

			_boundingSphere = new BoundingSphere(center, MathF.Sqrt(radiusSquared));
			_boundsVertexCount = Vertices.Count;
		}
	}
}
