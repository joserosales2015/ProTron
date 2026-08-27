using ProTron.Buffer;
using ProTron.Core;
using ProTron.Geometry;
using ProTron.Math;
using ProTron.Objects;
using ProTron.Rendering;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static ProTron.Geometry.Triangle;
using static ProTron.Graphics.Clipper;
using Matrix4x4 = ProTron.Math.Matrix4x4;

namespace ProTron.Graphics
{
	public class Renderer
	{
		private readonly Projection _projection;
		private readonly Camera _camera;
		private readonly Rasterizer _rasterizer;
		private Matrix4x4 _view;

		public RendererStats Stats { get; } = new RendererStats();

		public Renderer(Viewport viewport, Camera camera, Rasterizer rasterizer)
		{
			_camera = camera;
			_projection = new Projection(viewport);
			_rasterizer = rasterizer;
			_rasterizer.Stats = Stats;
		}

		public void BeginFrame()
		{
			_view = _camera.GetViewMatrix();
		}

		public void Draw(GameObject obj)
		{
			Stats.DrawCalls++;
			Stats.Objects++;
			Stats.CameraZ = _camera.Transform.Position.Z;
			Geometry.Mesh mesh = obj.Mesh;
			Matrix4x4 world = obj.Transform.GetWorldMatrix();
			Matrix4x4 worldView = _view * world;
			Vertex[] transformedVertices = new Vertex[mesh.Vertices.Count];

			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				transformedVertices[i] = TransformVertex(mesh.Vertices[i], worldView);
				Stats.VerticesTransformed++;
			}

			foreach (Triangle triangle in mesh.Triangles)
			{
				Vertex a = transformedVertices[triangle.A];
				Vertex b = transformedVertices[triangle.B];
				Vertex c = transformedVertices[triangle.C];

				Stats.TrianglesSubmitted++;

				if (IsBackFace(a.Position, b.Position, c.Position))
				{
					Stats.TrianglesCulled++;
					continue;
				}

				ClipResult clip = Clipper.ClipTriangle(a, b, c, _camera.NearPlane);

				if (clip.Count == 2)
					Stats.TrianglesClipped++;

				if (clip.Count >= 1)
					DrawTriangle(
						clip.Triangle1,
						triangle,
						obj.Material.Color);

				if (clip.Count == 2)
					DrawTriangle(
						clip.Triangle2,
						triangle,
						obj.Material.Color);
			}
		}

		private void DrawTriangle(ClippedTriangle triangle, Triangle original, uint color)
		{
			VertexOut o1 = ProjectVertex(triangle.A);
			VertexOut o2 = ProjectVertex(triangle.B);
			VertexOut o3 = ProjectVertex(triangle.C);

			_rasterizer.DrawFilledTriangle(o1, o2, o3, color);
			Stats.TrianglesRendered++;
			_rasterizer.DrawTriangleWireframe(o1, o2, o3, original, 0xff00ff00);
		}

		private VertexOut ProjectVertex(Vertex v)
		{
			return new VertexOut(
				_projection.Project(v.Position),
				1.0f / v.Position.Z);
		}

		private bool IsBackFace(Vector3f v1, Vector3f v2, Vector3f v3)
		{
			Vector3f edge1 = v2 - v1;
			Vector3f edge2 = v3 - v1;

			Vector3f normal = Vector3f.Cross(edge1, edge2);

			Vector3f viewDir = -v1;

			float dot = Vector3f.Dot(normal, viewDir);

			return dot >= 0;
		}

		private Vertex TransformVertex(Vertex vertex, Matrix4x4 worldView)
		{
			Vector4f p = worldView * vertex.Position.ToVector4();

			return new Vertex(new Vector3f(p.X, p.Y, p.Z));
		}
	}
}
