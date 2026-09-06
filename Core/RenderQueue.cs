using ProTron.Math;
using ProTron.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Core
{
	public sealed class RenderQueue
	{
		private readonly List<GameObject> _opaqueObjects = new();
		private readonly List<GameObject> _transparentObjects = new();
		private readonly DistanceComparer _frontToBack = new(descending: false);
		private readonly DistanceComparer _backToFront = new(descending: true);

		public IReadOnlyList<GameObject> OpaqueObjects => _opaqueObjects;
		public IReadOnlyList<GameObject> TransparentObjects => _transparentObjects;

		public void Build(IEnumerable<GameObject> objects, Camera camera)
		{
			ArgumentNullException.ThrowIfNull(objects);
			ArgumentNullException.ThrowIfNull(camera);

			_opaqueObjects.Clear();
			_transparentObjects.Clear();

			foreach (GameObject obj in objects)
			{
				if (obj.Material.BlendMode == MaterialBlendMode.AlphaBlend)
					_transparentObjects.Add(obj);
				else
					_opaqueObjects.Add(obj);
			}

			_frontToBack.CameraPosition = camera.Transform.Position;
			_backToFront.CameraPosition = camera.Transform.Position;
			_opaqueObjects.Sort(_frontToBack);
			_transparentObjects.Sort(_backToFront);
		}

		private sealed class DistanceComparer : IComparer<GameObject>
		{
			private readonly bool _descending;

			public Vector3f CameraPosition { get; set; }

			public DistanceComparer(bool descending)
			{
				_descending = descending;
			}

			public int Compare(GameObject? first, GameObject? second)
			{
				if (ReferenceEquals(first, second))
					return 0;

				if (first is null)
					return 1;

				if (second is null)
					return -1;

				float firstDistance =
					(first.Transform.Position - CameraPosition).LengthSquared;
				float secondDistance =
					(second.Transform.Position - CameraPosition).LengthSquared;
				int comparison = firstDistance.CompareTo(secondDistance);

				return _descending ? -comparison : comparison;
			}
		}
	}
}
