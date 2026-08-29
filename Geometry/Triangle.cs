using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public struct Triangle
	{
		public int A { get; }
		public int B { get; }
		public int C { get; }

		[Flags]
		public enum TriangleEdges
		{
			None = 0,

			AB = 1 << 0,   // 001
			BC = 1 << 1,   // 010
			CA = 1 << 2,   // 100

			All = AB | BC | CA   // 111
		}

		public TriangleEdges VisibleEdges { get; set; }

		public Triangle(int a, int b, int c)
		{
			A = a;
			B = b;
			C = c;

			VisibleEdges = TriangleEdges.All;
		}

		public bool HasEdge(TriangleEdges edge)
		{
			return (VisibleEdges & edge) != 0;
		}
		public void ShowEdge(TriangleEdges edge)
		{
			VisibleEdges |= edge;
		}

		public void HideEdge(TriangleEdges edge)
		{
			VisibleEdges &= ~edge;
		}

	}
}
