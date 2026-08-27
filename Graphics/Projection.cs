using ProTron.Core;
using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;


namespace ProTron.Graphics
{
	public class Projection
	{
		private readonly Viewport _viewport;

		public float FocalLength { get; set; } = 500f;

		public Projection(Viewport viewport)
		{
			_viewport = viewport;
		}

		public Vector2 Project(Vector3f point)
		{
			if (point.Z <= 0.01f)
				point.Z = 0.01f;

			float x = point.X * FocalLength / point.Z;
			float y = point.Y * FocalLength / point.Z;

			x += _viewport.CenterX;
			y = _viewport.CenterY - y;

			return new Vector2(x, y);
		}
	}
}
