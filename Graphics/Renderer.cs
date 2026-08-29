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
		private readonly Scene _scene;
		private Vector3f _lightDirectionView;
		private Matrix4x4 _view;
		private Vertex[] _transformedVertices = Array.Empty<Vertex>();

		public RendererStats Stats { get; } = new RendererStats();

		public Renderer(Viewport viewport, Scene scene, Rasterizer rasterizer)
		{
			_scene = scene;
			_camera = scene.Camera;
			_projection = new Projection(viewport);
			_rasterizer = rasterizer;
			_rasterizer.Stats = Stats;
		}

		public void BeginFrame()
		{
			_view = _camera.GetViewMatrix();

			Vector4f light = _view * _scene.DirectionalLight.WorldDirection.ToVector4(0f);

			_lightDirectionView = new Vector3f(
				light.X,
				light.Y,
				light.Z).Normalized();
		}

		public void Draw(GameObject obj)
		{
			Stats.IncrementDrawCalls();
			Stats.IncrementObjects();
			Stats.UpdateCameraZ(_camera.Transform.Position.Z);
			Geometry.Mesh mesh = obj.Mesh;
			Matrix4x4 world = obj.Transform.GetWorldMatrix();
			Matrix4x4 worldView = _view * world;

			EnsureVertexCapacity(mesh.Vertices.Count);

			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				_transformedVertices[i] = TransformVertex(mesh.Vertices[i], worldView);
				Stats.IncrementVerticesTransformed();
			}

			foreach (Triangle triangle in mesh.Triangles)
			{
				Vertex a = _transformedVertices[triangle.A];
				Vertex b = _transformedVertices[triangle.B];
				Vertex c = _transformedVertices[triangle.C];

				Stats.IncrementTrianglesSubmitted();

				if (IsBackFace(a.Position, b.Position, c.Position))
				{
					Stats.IncrementTrianglesCulled();
					continue;
				}

				uint shadedColor = CalculateLitColor(
					a.Position,
					b.Position,
					c.Position,
					obj.Material.Color);

				ClipResult clip = Clipper.ClipTriangle(a, b, c, _camera.NearPlane);

				if (clip.Count == 2)
					Stats.IncrementTrianglesClipped();

				if (clip.Count >= 1)
					DrawTriangle(
						clip.Triangle1,
						triangle,
						shadedColor);

				if (clip.Count == 2)
					DrawTriangle(
						clip.Triangle2,
						triangle,
						shadedColor);
			}
		}

		private uint CalculateLitColor(Vector3f a, Vector3f b, Vector3f c, uint baseColor)
		{
			Vector3f edge1 = b - a;
			Vector3f edge2 = c - a;

			Vector3f normal = Vector3f.Cross(edge1, edge2).Normalized();

			// La normal apunta desde la cara; la luz viaja hacia la escena.
			float diffuse = MathF.Max(
				0f,
				Vector3f.Dot(normal, -_lightDirectionView));

			float intensity = _scene.AmbientLight +
				diffuse * _scene.DirectionalLight.Intensity;

			return ColorUtils.Shade(baseColor, intensity);
		}

		private void DrawTriangle(ClippedTriangle triangle, Triangle original, uint color)
		{
			Vector3f edge1 = triangle.B.Position - triangle.A.Position;
			Vector3f edge2 = triangle.C.Position - triangle.A.Position;
			
			Vector3f normal = Vector3f.Cross(edge1, edge2).Normalized();

			VertexOut o1 = ProjectVertex(triangle.A);
			VertexOut o2 = ProjectVertex(triangle.B);
			VertexOut o3 = ProjectVertex(triangle.C);

			_rasterizer.DrawFilledTriangle(o1, o2, o3, color);
			Stats.IncrementTrianglesRendered();
			//_rasterizer.DrawTriangleWireframe(o1, o2, o3, original, 0xff00ff00);
		}

		private void EnsureVertexCapacity(int count)
		{
			if (_transformedVertices.Length < count)
				Array.Resize(ref _transformedVertices, count);
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
