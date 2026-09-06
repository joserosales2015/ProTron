using ProTron.Rendering;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Debugging
{
	public static class DebugOverlay
	{
		[Conditional("DEBUG")]
		public static void Draw(RendererStats stats)
		{
			int x = 10;
			int y = 10;
			int lineHeight = 20;

			Raylib.DrawText($"FPS: {stats.FPS:F0}", x, y, 20, Color.Yellow);
			y += lineHeight;

			Raylib.DrawText($"Frame: {stats.FrameTime:F2} ms", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Update: {stats.UpdateTime:F2} ms", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Render CPU: {stats.RenderTime:F2} ms", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Present: {stats.PresentTime:F2} ms", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Objects: {stats.Objects}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Objects Culled: {stats.ObjectsCulled}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Draw Calls: {stats.DrawCalls}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Vertices Transformed: {stats.VerticesTransformed}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Triangles Submitted: {stats.TrianglesSubmitted}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Triangles Rendered: {stats.TrianglesRendered}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Backface Culled: {stats.TrianglesCulled}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Triangles Clipped: {stats.TrianglesClipped}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Triangles Rejected: {stats.TrianglesRejected}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Depth Tests: {stats.DepthTests:N0}", x, y, 20, Color.White); 
			y += lineHeight;
			
			Raylib.DrawText($"Pixels Drawn: {stats.PixelsDrawn:N0}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Depth Rejects: {stats.DepthRejected:N0}", x, y, 20, Color.White);
			y += lineHeight;

			Raylib.DrawText($"Camera Z: {stats.CameraZ:F2}", x, y, 20, Color.White);
		}
	}
}
