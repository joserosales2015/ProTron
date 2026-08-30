using ProTron.Geometry;
using ProTron.Graphics;
using ProTron.Math;
using System.Numerics;

namespace ProTron.Objects
{
	public class Cube : GameObject
	{
		public Cube(float size)
		{
			Mesh = new Mesh();
			Build(size);
		}

		private void Build(float size)
		{
			float h = size / 2f;

			// Cada cara tiene sus propios vértices porque las UV tienen
			// discontinuidades en las esquinas del cubo.
			AddQuad(
				new Vector3f(-h, -h, -h),
				new Vector3f(h, -h, -h),
				new Vector3f(h, h, -h),
				new Vector3f(-h, h, -h));

			AddQuad(
				new Vector3f(-h, -h, h),
				new Vector3f(-h, h, h),
				new Vector3f(h, h, h),
				new Vector3f(h, -h, h));

			AddQuad(
				new Vector3f(-h, -h, -h),
				new Vector3f(-h, h, -h),
				new Vector3f(-h, h, h),
				new Vector3f(-h, -h, h));

			AddQuad(
				new Vector3f(h, -h, -h),
				new Vector3f(h, -h, h),
				new Vector3f(h, h, h),
				new Vector3f(h, h, -h));

			AddQuad(
				new Vector3f(-h, h, -h),
				new Vector3f(h, h, -h),
				new Vector3f(h, h, h),
				new Vector3f(-h, h, h));

			AddQuad(
				new Vector3f(-h, -h, -h),
				new Vector3f(-h, -h, h),
				new Vector3f(h, -h, h),
				new Vector3f(h, -h, -h));
		}

		private void AddQuad(
			Vector3f v0,
			Vector3f v1,
			Vector3f v2,
			Vector3f v3)
		{
			int first = Mesh.Vertices.Count;

			Mesh.Vertices.Add(new Vertex(v0, Vector3f.Zero, new Vector2(0f, 1f)));
			Mesh.Vertices.Add(new Vertex(v1, Vector3f.Zero, new Vector2(1f, 1f)));
			Mesh.Vertices.Add(new Vertex(v2, Vector3f.Zero, new Vector2(1f, 0f)));
			Mesh.Vertices.Add(new Vertex(v3, Vector3f.Zero, new Vector2(0f, 0f)));

			Mesh.Triangles.Add(new Triangle(first, first + 1, first + 2)
			{
				VisibleEdges = Triangle.TriangleEdges.AB |
					Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(first, first + 2, first + 3)
			{
				VisibleEdges = Triangle.TriangleEdges.BC |
					Triangle.TriangleEdges.CA
			});
		}

		public override void Update(float deltaTime)
		{
			var rotation = Transform.Rotation;

			rotation.X += 10 * deltaTime;
			rotation.Y += 20 * deltaTime;
			rotation.Z += 15 * deltaTime;

			Transform.Rotation = rotation;
		}
	}
}
