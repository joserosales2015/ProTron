using ProTron.Math;
using ProTron.Objects;

namespace ProTron.Core
{
	public class Scene
	{
		private readonly List<GameObject> _objects = new();

		public Camera Camera { get; }

		public IReadOnlyList<GameObject> Objects => _objects;

		public Scene()
		{
			_objects = new List<GameObject>();

			Camera = new Camera();
			Camera.Transform.Position = new Math.Vector3f(0, 0, 0);
		}

		public void Add(GameObject obj)
		{
			_objects.Add(obj);
		}

		public void AddRange(IEnumerable<GameObject> objects)
		{
			_objects.AddRange(objects);
		}

		public void Remove(GameObject obj)
		{
			_objects.Remove(obj);
		}

		public void Clear()
		{
			_objects.Clear();
		}

		public bool Contains(GameObject obj)
		{
			return _objects.Contains(obj);
		}

		public int Count => _objects.Count;

		public void SortFrontToBack(Camera camera)
		{
			_objects.Sort((a, b) =>
			{
				float da = (a.Transform.Position - camera.Transform.Position).LengthSquared;
					
				float db = (b.Transform.Position - camera.Transform.Position).LengthSquared;

				return da.CompareTo(db);
			});
		}
	}
}