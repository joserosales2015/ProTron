using ProTron.Geometry;
using ProTron.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

			Mesh.Vertices.Add(new Vertex(-h, -h, -h)); //0
			Mesh.Vertices.Add(new Vertex(h, -h, -h)); //1
			Mesh.Vertices.Add(new Vertex(-h, h, -h)); //2
			Mesh.Vertices.Add(new Vertex(h, h, -h)); //3

			Mesh.Vertices.Add(new Vertex(-h, -h, h)); //4
			Mesh.Vertices.Add(new Vertex(h, -h, h)); //5
			Mesh.Vertices.Add(new Vertex(-h, h, h)); //6
			Mesh.Vertices.Add(new Vertex(h, h, h)); //7

			// Cara delantera
			Mesh.Triangles.Add(new Triangle(0, 1, 2)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(1, 3, 2)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.BC
			});

			// Cara trasera
			Mesh.Triangles.Add(new Triangle(4, 6, 5)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(6, 7, 5)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.BC
			});

			// Cara izquierda
			Mesh.Triangles.Add(new Triangle(0, 2, 6)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.BC
			});

			Mesh.Triangles.Add(new Triangle(0, 6, 4)
			{
				VisibleEdges = Triangle.TriangleEdges.BC | Triangle.TriangleEdges.CA
			});

			// Cara derecha
			Mesh.Triangles.Add(new Triangle(1, 5, 3)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(3, 5, 7)
			{
				VisibleEdges = Triangle.TriangleEdges.BC | Triangle.TriangleEdges.CA
			});

			// Cara superior
			Mesh.Triangles.Add(new Triangle(2, 3, 6)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(6, 3, 7)
			{
				VisibleEdges = Triangle.TriangleEdges.BC | Triangle.TriangleEdges.CA
			});

			// Cara inferior
			Mesh.Triangles.Add(new Triangle(0, 4, 1)
			{
				VisibleEdges = Triangle.TriangleEdges.AB | Triangle.TriangleEdges.CA
			});

			Mesh.Triangles.Add(new Triangle(1, 4, 5)
			{
				VisibleEdges = Triangle.TriangleEdges.BC | Triangle.TriangleEdges.CA
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
