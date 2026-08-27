using ProTron.Buffer;
using ProTron.Geometry;
using ProTron.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ProTron.Geometry.Triangle;

namespace ProTron.Graphics
{
	public class Rasterizer
	{
		private readonly FrameBuffer _frameBuffer;
		private readonly DepthBuffer _depthBuffer;
		public RendererStats Stats;
		public float DepthBias { get; set; } = 0.0f;

		public Rasterizer(
			FrameBuffer frameBuffer,
			DepthBuffer depthBuffer)
		{
			_frameBuffer = frameBuffer;
			_depthBuffer = depthBuffer;
		}

		public void DrawTriangleWireframe(VertexOut v1, VertexOut v2, VertexOut v3, Triangle triangle, uint baseColor)
		{
			if (triangle.HasEdge(TriangleEdges.AB))
				DrawLine(v1, v2, baseColor, DepthBias);

			if (triangle.HasEdge(TriangleEdges.BC))
				DrawLine(v2, v3, baseColor, DepthBias);

			if (triangle.HasEdge(TriangleEdges.CA))
				DrawLine(v3, v1, baseColor, DepthBias);
		}

		public void DrawFilledTriangle(VertexOut v1, VertexOut v2, VertexOut v3, uint baseColor)
		{
			int minX = (int)MathF.Floor(MathF.Min(v1.Position.X, MathF.Min(v2.Position.X, v3.Position.X)));
			int maxX = (int)MathF.Ceiling(MathF.Max(v1.Position.X, MathF.Max(v2.Position.X, v3.Position.X)));

			int minY = (int)MathF.Floor(MathF.Min(v1.Position.Y, MathF.Min(v2.Position.Y, v3.Position.Y)));
			int maxY = (int)MathF.Ceiling(MathF.Max(v1.Position.Y, MathF.Max(v2.Position.Y, v3.Position.Y)));

			minX = System.Math.Max(0, minX);
			minY = System.Math.Max(0, minY);

			maxX = System.Math.Min(_frameBuffer.Width - 1, maxX);
			maxY = System.Math.Min(_frameBuffer.Height - 1, maxY);

			float area = Edge(v1.Position, v2.Position, v3.Position);

			if (area == 0)
				return;

			float invArea = 1.0f / area;

			float e0dx = (v3.Position.Y - v2.Position.Y);
			float e0dy = -(v3.Position.X - v2.Position.X);
			float e1dx = (v1.Position.Y - v3.Position.Y);
			float e1dy = -(v1.Position.X - v3.Position.X);
			float e2dx = (v2.Position.Y - v1.Position.Y);
			float e2dy = -(v2.Position.X - v1.Position.X);

			float w0dx = e0dx * invArea;
			float w0dy = e0dy * invArea;
			float w1dx = e1dx * invArea;
			float w1dy = e1dy * invArea;
			float w2dx = e2dx * invArea;
			float w2dy = e2dy * invArea;

			Vector2 p0 = new Vector2(minX + 0.5f, minY + 0.5f);

			float rowW0 = Edge(v2.Position, v3.Position, p0) * invArea;
			float rowW1 = Edge(v3.Position, v1.Position, p0) * invArea;
			float rowW2 = Edge(v1.Position, v2.Position, p0) * invArea;
			
			float rowDepth = rowW0 * v1.Depth + rowW1 * v2.Depth + rowW2 * v3.Depth;

			float depthDx = w0dx * v1.Depth	+ w1dx * v2.Depth + w2dx * v3.Depth;
			float depthDy = w0dy * v1.Depth + w1dy * v2.Depth + w2dy * v3.Depth;

			for (int y = minY; y <= maxY; y++)
			{
				float w0 = rowW0;
				float w1 = rowW1;
				float w2 = rowW2;
				float depth = rowDepth;

				for (int x = minX; x <= maxX; x++)
				{
					if (w0 >= 0 && w1 >= 0 && w2 >= 0)
					{
						Stats.DepthTests++;
						if (_depthBuffer.TestAndSet(x, y, depth))
						{
							_frameBuffer.PutPixelUnChecked(x, y, baseColor);
							Stats.PixelsDrawn++;
						}
						else
						{
							Stats.DepthRejected++;
						}
					}
					w0 += w0dx;
					w1 += w1dx;
					w2 += w2dx;

					depth += depthDx;
				}

				rowW0 += w0dy;
				rowW1 += w1dy;
				rowW2 += w2dy;

				rowDepth += depthDy;
			}
		}

		public void DrawLine(VertexOut a, VertexOut b, uint color, float depthBias = 0f)
		{
			int x0 = (int)a.Position.X;
			int y0 = (int)a.Position.Y;

			int x1 = (int)b.Position.X;
			int y1 = (int)b.Position.Y;

			float z0 = a.Depth;
			float z1 = b.Depth;

			int dx = System.Math.Abs(x1 - x0);
			int dy = System.Math.Abs(y1 - y0);

			int steps = System.Math.Max(dx, dy);
			int currentStep = 0;

			if (steps == 0)
				steps = 1;

			int sx = x0 < x1 ? 1 : -1;
			int sy = y0 < y1 ? 1 : -1;

			int err = dx - dy;

			while (true)
			{
				float t = (float)currentStep / steps;
				float depth = (z0 + (z1 - z0) * t) - depthBias;

				if (x0 >= 0 &&
					x0 < _frameBuffer.Width &&
					y0 >= 0 &&
					y0 < _frameBuffer.Height)
				{
					Stats.DepthTests++;
					if (_depthBuffer.TestAndSet(x0, y0, depth))
					{
						_frameBuffer.PutPixel(x0, y0, color);
						Stats.PixelsDrawn++;
					}
					else
					{
						Stats.DepthRejected++;
					}
				}

				if (x0 == x1 && y0 == y1)
					break;

				int e2 = 2 * err;

				if (e2 > -dy)
				{
					err -= dy;
					x0 += sx;
				}

				if (e2 < dx)
				{
					err += dx;
					y0 += sy;
				}

				currentStep++;
			}
		}

		private float Edge(Vector2 a, Vector2 b, Vector2 p)
		{
			return
				(p.X - a.X) * (b.Y - a.Y)
			  - (p.Y - a.Y) * (b.X - a.X);
		}
	}
}
