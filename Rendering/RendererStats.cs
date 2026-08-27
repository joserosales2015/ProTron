using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Rendering
{
	public class RendererStats
	{
		// Camara
		public float CameraZ { get; internal set; }

		// Rendimiento
		public float FPS { get; internal set; }
		public float FrameTime { get; internal set; }

		// Escena
		public int DrawCalls { get; internal set; }
		public int Objects { get; internal set; }

		// Geometría
		public int VerticesTransformed { get; internal set; }
		public int TrianglesSubmitted { get; internal set; }
		public int TrianglesClipped { get; internal set; }
		public int TrianglesRendered { get; internal set; }
		public int TrianglesCulled { get; internal set; }

		// Rasterización
		public int PixelsDrawn { get; internal set; }
		public int DepthTests { get; internal set; }
		public int DepthRejected { get; internal set; }

		internal Stopwatch Stopwatch = new Stopwatch();

		private int _frameCounter;

		public RendererStats()
		{
			Stopwatch.Start();
		}

		public void BeginFrame()
		{
			DrawCalls = 0;
			Objects = 0;

			VerticesTransformed = 0;
			TrianglesSubmitted = 0;
			TrianglesClipped = 0;
			TrianglesRendered = 0;
			TrianglesCulled = 0;

			PixelsDrawn = 0;
			DepthTests = 0;
			DepthRejected = 0;
		}

		public void EndFrame()
		{
			_frameCounter++;

			if (Stopwatch.ElapsedMilliseconds >= 1000)
			{
				FPS = _frameCounter * 1000f / Stopwatch.ElapsedMilliseconds;

				FrameTime = 1000f / FPS;

				_frameCounter = 0;

				Stopwatch.Restart();
			}
		}
	}
}
