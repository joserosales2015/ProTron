using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public class Mesh
	{
		public List<Vertex> Vertices { get; } = new();

		public List<Edge> Edges { get; } = new();

		public List<Triangle> Triangles { get; } = new();
	}
}
