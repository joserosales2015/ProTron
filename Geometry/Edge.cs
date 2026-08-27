using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Geometry
{
	public class Edge
	{
		public int Start;
		public int End;

		public Edge(int start, int end)
		{
			Start = start;
			End = end;
		}
	}
}
