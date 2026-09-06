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
		private float _scaleX;
		private float _scaleY;
		private float _configuredFieldOfView = float.NaN;
		private float _configuredAspectRatio = float.NaN;

		public float AspectRatio =>
			_viewport.Width / (float)_viewport.Height;

		public Projection(Viewport viewport)
		{
			_viewport = viewport ?? throw new ArgumentNullException(nameof(viewport));
		}

		public void Configure(float fieldOfView)
		{
			if (!float.IsFinite(fieldOfView) || fieldOfView <= 0f || fieldOfView >= 180f)
				throw new ArgumentOutOfRangeException(nameof(fieldOfView));

			float aspectRatio = AspectRatio;

			if (!float.IsFinite(aspectRatio) || aspectRatio <= 0f)
				throw new InvalidOperationException("El viewport debe tener dimensiones positivas.");

			if (fieldOfView == _configuredFieldOfView &&
				aspectRatio == _configuredAspectRatio)
				return;

			float halfFovRadians = fieldOfView * MathF.PI / 360f;
			_scaleY = 1f / MathF.Tan(halfFovRadians);
			_scaleX = _scaleY / aspectRatio;
			_configuredFieldOfView = fieldOfView;
			_configuredAspectRatio = aspectRatio;
		}

		public Vector2 Project(Vector3f point, float inverseDepth)
		{
			// Coordenadas normalizadas de pantalla: -1 a 1.
			float ndcX = point.X * _scaleX * inverseDepth;
			float ndcY = point.Y * _scaleY * inverseDepth;

			// De NDC a píxeles.
			float screenX = (ndcX + 1f) * 0.5f * _viewport.Width;
			float screenY = (1f - ndcY) * 0.5f * _viewport.Height;

			return new Vector2(screenX, screenY);
		}

	}
}
