using ProTron.Geometry;
using ProTron.Math;

namespace ProTron.Graphics
{
	public static class Clipper
	{
		// El clipping de un triángulo contra un frustum puede producir hasta
		// siete triángulos. Ocho deja margen y evita asignaciones por frame.
		public const int MaxClippedTriangles = 8;

		public readonly struct ClippedTriangle
		{
			public Vertex A { get; }
			public Vertex B { get; }
			public Vertex C { get; }

			public ClippedTriangle(Vertex a, Vertex b, Vertex c)
			{
				A = a;
				B = b;
				C = c;
			}
		}

		public readonly struct ClipResult
		{
			public int TriangleCount { get; }
			public bool WasClipped { get; }
			public bool WasRejected => TriangleCount == 0;

			public ClipResult(int triangleCount, bool wasClipped)
			{
				TriangleCount = triangleCount;
				WasClipped = wasClipped;
			}
		}

		private readonly struct Plane
		{
			public Vector3f Normal { get; }
			public float Distance { get; }

			public Plane(Vector3f normal, float distance = 0f)
			{
				Normal = normal;
				Distance = distance;
			}

			public float SignedDistance(Vertex vertex)
			{
				return Vector3f.Dot(Normal, vertex.Position) + Distance;
			}
		}

		public static ClipResult ClipTriangleAgainstFrustum(
			Vertex a,
			Vertex b,
			Vertex c,
			float nearPlane,
			float farPlane,
			float fieldOfView,
			float aspectRatio,
			Span<ClippedTriangle> output)
		{
			if (output.Length < MaxClippedTriangles)
				throw new ArgumentException(
					$"El buffer de salida debe tener al menos {MaxClippedTriangles} elementos.",
					nameof(output));

			float halfFovRadians = fieldOfView * MathF.PI / 360f;
			float tanHalfFovY = MathF.Tan(halfFovRadians);
			float tanHalfFovX = tanHalfFovY * aspectRatio;

			Span<Vertex> firstBuffer = stackalloc Vertex[10];
			Span<Vertex> secondBuffer = stackalloc Vertex[10];

			firstBuffer[0] = a;
			firstBuffer[1] = b;
			firstBuffer[2] = c;

			int vertexCount = 3;
			bool wasClipped = false;

			Span<Plane> planes = stackalloc Plane[6];
			planes[0] = new Plane(new Vector3f(0f, 0f, 1f), -nearPlane);
			planes[1] = new Plane(new Vector3f(0f, 0f, -1f), farPlane);
			planes[2] = new Plane(new Vector3f(1f, 0f, tanHalfFovX));
			planes[3] = new Plane(new Vector3f(-1f, 0f, tanHalfFovX));
			planes[4] = new Plane(new Vector3f(0f, 1f, tanHalfFovY));
			planes[5] = new Plane(new Vector3f(0f, -1f, tanHalfFovY));

			foreach (Plane plane in planes)
			{
				vertexCount = ClipPolygonAgainstPlane(
					firstBuffer,
					vertexCount,
					plane,
					secondBuffer,
					out bool planeClipped);

				wasClipped |= planeClipped;

				if (vertexCount == 0)
					return new ClipResult(0, true);

				Span<Vertex> temporary = firstBuffer;
				firstBuffer = secondBuffer;
				secondBuffer = temporary;
			}

			int triangleCount = System.Math.Max(0, vertexCount - 2);

			for (int i = 0; i < triangleCount; i++)
			{
				output[i] = new ClippedTriangle(
					firstBuffer[0],
					firstBuffer[i + 1],
					firstBuffer[i + 2]);
			}

			return new ClipResult(triangleCount, wasClipped);
		}

		private static int ClipPolygonAgainstPlane(
			ReadOnlySpan<Vertex> input,
			int inputCount,
			Plane plane,
			Span<Vertex> output,
			out bool wasClipped)
		{
			wasClipped = false;

			if (inputCount == 0)
				return 0;

			int outputCount = 0;
			Vertex previous = input[inputCount - 1];
			float previousDistance = plane.SignedDistance(previous);
			bool previousInside = previousDistance >= 0f;

			for (int i = 0; i < inputCount; i++)
			{
				Vertex current = input[i];
				float currentDistance = plane.SignedDistance(current);
				bool currentInside = currentDistance >= 0f;

				if (currentInside != previousInside)
				{
					wasClipped = true;
					float t = previousDistance /
						(previousDistance - currentDistance);

					output[outputCount++] = Vertex.Lerp(previous, current, t);
				}

				if (currentInside)
					output[outputCount++] = current;

				previous = current;
				previousDistance = currentDistance;
				previousInside = currentInside;
			}

			if (outputCount != inputCount)
				wasClipped = true;

			return outputCount;
		}
	}
}
