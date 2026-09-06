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
		public float UpdateTime { get; internal set; }
		public float RenderTime { get; internal set; }
		public float PresentTime { get; internal set; }

		// Escena
		public int DrawCalls { get; internal set; }
		public int Objects { get; internal set; }
		public int ObjectsCulled { get; internal set; }

		// Geometría
		public int VerticesTransformed { get; internal set; }
		public int TrianglesSubmitted { get; internal set; }
		public int TrianglesClipped { get; internal set; }
		public int TrianglesRejected { get; internal set; }
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
			#if DEBUG
			Stopwatch.Start();
			#endif
		}

		[Conditional("DEBUG")]
		public void BeginFrame()
		{
			DrawCalls = 0;
			Objects = 0;
			ObjectsCulled = 0;

			VerticesTransformed = 0;
			TrianglesSubmitted = 0;
			TrianglesClipped = 0;
			TrianglesRejected = 0;
			TrianglesRendered = 0;
			TrianglesCulled = 0;

			PixelsDrawn = 0;
			DepthTests = 0;
			DepthRejected = 0;
		}

		[Conditional("DEBUG")]
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

		[Conditional("DEBUG")]
		public void UpdateCameraZ(float cameraZ)
		{
			CameraZ = cameraZ;
		}

		[Conditional("DEBUG")]
		public void UpdateFPS(float fps)
		{
			FPS = fps;
		}

		[Conditional("DEBUG")]
		public void UpdateFrameTime(float frameTime)
		{
			FrameTime = frameTime;
		}

		[Conditional("DEBUG")]
		public void IncrementDrawCalls() => DrawCalls++;

		[Conditional("DEBUG")]
		public void IncrementObjects() => Objects++;

		[Conditional("DEBUG")]
		public void IncrementObjectsCulled() => ObjectsCulled++;

		[Conditional("DEBUG")]
		public void IncrementVerticesTransformed() => VerticesTransformed++;

		[Conditional("DEBUG")]
		public void IncrementTrianglesSubmitted() => TrianglesSubmitted++;

		[Conditional("DEBUG")]
		public void IncrementTrianglesClipped() => TrianglesClipped++;

		[Conditional("DEBUG")]
		public void IncrementTrianglesRejected() => TrianglesRejected++;

		[Conditional("DEBUG")]
		public void IncrementTrianglesRendered() => TrianglesRendered++;

		[Conditional("DEBUG")]
		public void IncrementTrianglesCulled() => TrianglesCulled++;

		[Conditional("DEBUG")]
		public void IncrementPixelsDrawn() => PixelsDrawn++;

		[Conditional("DEBUG")]
		public void IncrementDepthTests() => DepthTests++;

		[Conditional("DEBUG")]
		public void IncrementDepthRejected() => DepthRejected++;

		[Conditional("DEBUG")]
		public void AddRasterization(
			int depthTests,
			int depthRejected,
			int pixelsDrawn)
		{
			DepthTests += depthTests;
			DepthRejected += depthRejected;
			PixelsDrawn += pixelsDrawn;
		}

		[Conditional("DEBUG")]
		public void UpdateStageTimes(
			float updateTime,
			float renderTime,
			float presentTime)
		{
			UpdateTime = updateTime;
			RenderTime = renderTime;
			PresentTime = presentTime;
		}

	}
}
