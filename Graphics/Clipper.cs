using ProTron.Geometry;
using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Graphics
{
	public static class Clipper
	{
		public struct ClippedTriangle
		{
			public Vertex A;
			public Vertex B;
			public Vertex C;

			public ClippedTriangle(Vertex a, Vertex b, Vertex c)
			{
				A = a;
				B = b;
				C = c;
			}
		}

		public struct ClipResult
		{
			public int Count;

			public ClippedTriangle Triangle1;
			public ClippedTriangle Triangle2;
		}

		public static ClipResult ClipTriangle(Vertex a, Vertex b, Vertex c, float nearPlane)
		{
			Vector3f originalNormal = Vector3f.Zero;
			ClipResult result = new ClipResult();
			bool insideA = a.Position.Z >= nearPlane;
			bool insideB = b.Position.Z >= nearPlane;
			bool insideC = c.Position.Z >= nearPlane;
			int insideCount = 0;
			
			if (insideA) insideCount++;
			if (insideB) insideCount++;
			if (insideC) insideCount++;

			switch (insideCount)
			{
				case 0:
					return result;

				case 3:
					result.Count = 1;
					result.Triangle1 = new ClippedTriangle(a, b, c);

					return result;

				case 1:
					originalNormal = Vector3f.Cross(b.Position - a.Position, c.Position - a.Position);

					if (insideA)
					{
						Vertex p = IntersectNearPlane(a, b, nearPlane);
						Vertex q = IntersectNearPlane(a, c, nearPlane);

						result.Count = 1;
						result.Triangle1 = new ClippedTriangle(a, p, q);

						EnsureWinding(originalNormal, ref result.Triangle1);

						return result;
					}
					else if (insideB)
					{
						Vertex p = IntersectNearPlane(b, a, nearPlane);
						Vertex q = IntersectNearPlane(b, c, nearPlane);

						result.Count = 1;
						result.Triangle1 = new ClippedTriangle(b, p, q);
						
						EnsureWinding(originalNormal, ref result.Triangle1);

						return result;
					}
					else if (insideC)
					{
						Vertex p = IntersectNearPlane(c, a, nearPlane);
						Vertex q = IntersectNearPlane(c, b, nearPlane);

						result.Count = 1;
						result.Triangle1 = new ClippedTriangle(c, p, q);

						EnsureWinding(originalNormal, ref result.Triangle1);

						return result;
					}
					break;
				case 2:
					originalNormal = Vector3f.Cross(b.Position - a.Position, c.Position - a.Position);

					if (insideA && insideB)
					{
						Vertex p = IntersectNearPlane(a, c, nearPlane);
						Vertex q = IntersectNearPlane(b, c, nearPlane);

						result.Count = 2;

						result.Triangle1 =
							new ClippedTriangle(
								a,
								b,
								p);

						EnsureWinding(originalNormal, ref result.Triangle1);

						result.Triangle2 =
							new ClippedTriangle(
								b,
								q,
								p);

						EnsureWinding(originalNormal, ref result.Triangle2);

						return result;
					}
					else if (insideA && insideC)
					{
						Vertex p = IntersectNearPlane(a, b, nearPlane);
						Vertex q = IntersectNearPlane(c, b, nearPlane);

						result.Count = 2;

						result.Triangle1 =
							new ClippedTriangle(
								a,
								c,
								p);

						EnsureWinding(originalNormal, ref result.Triangle1);

						result.Triangle2 =
							new ClippedTriangle(
								c,
								q,
								p);

						EnsureWinding(originalNormal, ref result.Triangle2);

						return result;
					}
					else if (insideB && insideC)
					{
						Vertex p = IntersectNearPlane(b, a, nearPlane);
						Vertex q = IntersectNearPlane(c, a, nearPlane);

						result.Count = 2;

						result.Triangle1 =
							new ClippedTriangle(
								b,
								c,
								p);

						EnsureWinding(originalNormal, ref result.Triangle1);

						result.Triangle2 =
							new ClippedTriangle(
								c,
								q,
								p);

						EnsureWinding(originalNormal, ref result.Triangle2);

						return result;
					}

					return result;
			}

			return result;
		}

		private static void EnsureWinding(Vector3f originalNormal, ref ClippedTriangle triangle)
		{
			Vector3f edge1 = triangle.B.Position - triangle.A.Position;
			Vector3f edge2 = triangle.C.Position - triangle.A.Position;
			Vector3f normal = Vector3f.Cross(edge1, edge2);

			if (Vector3f.Dot(normal, originalNormal) < 0)
			{
				Vertex temp = triangle.B;
				triangle.B = triangle.C;
				triangle.C = temp;
			}
		}

		private static float SignedArea(Vertex a, Vertex b, Vertex c)
		{
			Vector3f ab = b.Position - a.Position;
			Vector3f ac = c.Position - a.Position;

			return Vector3f.Cross(ab, ac).Z;
		}

		private static float SignedArea2D(Vertex a, Vertex b, Vertex c)
		{
			return
				(b.Position.X - a.Position.X) * (c.Position.Y - a.Position.Y)
			  - (b.Position.Y - a.Position.Y) * (c.Position.X - a.Position.X);
		}

		public static Vertex IntersectNearPlane(Vertex a, Vertex b, float nearPlane)
		{
			float dz = b.Position.Z - a.Position.Z;

			if (MathF.Abs(dz) < 0.000001f)
				return a;

			float t = (nearPlane - a.Position.Z) / dz;

			return Vertex.Lerp(a, b, t);
		}
	}
}
