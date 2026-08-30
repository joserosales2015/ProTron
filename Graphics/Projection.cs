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

		public float AspectRatio =>
			_viewport.Width / (float)_viewport.Height;

		public Projection(Viewport viewport)
		{
			_viewport = viewport;
		}

		public Vector2 Project(Vector3f point, float fieldOfView)
		{
			float fovRadians = fieldOfView * MathF.PI / 180f;
			float tanHalfFov = MathF.Tan(fovRadians * 0.5f);

			float aspectRatio = AspectRatio;

			// Coordenadas normalizadas de pantalla: -1 a 1.
			float ndcX = point.X / (point.Z * tanHalfFov * aspectRatio);
			float ndcY = point.Y / (point.Z * tanHalfFov);

			// De NDC a píxeles.
			float screenX = (ndcX + 1f) * 0.5f * _viewport.Width;
			float screenY = (1f - ndcY) * 0.5f * _viewport.Height;

			return new Vector2(screenX, screenY);
		}

	}
}
