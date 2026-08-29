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
			public bool WasClipped;
		}

		public static ClipResult ClipTriangleAgainstZPlane(Vertex a, Vertex b, Vertex c, float planeZ, bool keepGreater)
		{
			Vector3f originalNormal = Vector3f.Zero;
			ClipResult result = new ClipResult();

			bool insideA = keepGreater ? a.Position.Z >= planeZ : a.Position.Z <= planeZ;
			bool insideB = keepGreater ? b.Position.Z >= planeZ : b.Position.Z <= planeZ;
			bool insideC = keepGreater ? c.Position.Z >= planeZ : c.Position.Z <= planeZ;

			int insideCount = 0;
			
			if (insideA) insideCount++;
			if (insideB) insideCount++;
			if (insideC) insideCount++;

			result.WasClipped = insideCount != 3;

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
						Vertex p = IntersectPlaneZ(a, b, planeZ);
						Vertex q = IntersectPlaneZ(a, c, planeZ);

						result.Count = 1;
						result.Triangle1 = new ClippedTriangle(a, p, q);

						EnsureWinding(originalNormal, ref result.Triangle1);

						return result;
					}
					else if (insideB)
					{
						Vertex p = IntersectPlaneZ(b, a, planeZ);
						Vertex q = IntersectPlaneZ(b, c, planeZ);

						result.Count = 1;
						result.Triangle1 = new ClippedTriangle(b, p, q);
						
						EnsureWinding(originalNormal, ref result.Triangle1);

						return result;
					}
					else if (insideC)
					{
						Vertex p = IntersectPlaneZ(c, a, planeZ);
						Vertex q = IntersectPlaneZ(c, b, planeZ);

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
						Vertex p = IntersectPlaneZ(a, c, planeZ);
						Vertex q = IntersectPlaneZ(b, c, planeZ);

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
						Vertex p = IntersectPlaneZ(a, b, planeZ);
						Vertex q = IntersectPlaneZ(c, b, planeZ);

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
						Vertex p = IntersectPlaneZ(b, a, planeZ);
						Vertex q = IntersectPlaneZ(c, a, planeZ);

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

		public static Vertex IntersectPlaneZ(Vertex a, Vertex b, float planeZ)
		{
			float dz = b.Position.Z - a.Position.Z;

			if (MathF.Abs(dz) < 0.000001f)
				return a;

			float t = (planeZ - a.Position.Z) / dz;

			return Vertex.Lerp(a, b, t);
		}
	}
}
