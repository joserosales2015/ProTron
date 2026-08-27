using ProTron.Components;
using ProTron.Geometry;
using ProTron.Math;

namespace ProTron.Objects
{
	public abstract class GameObject
	{
		public Transform Transform { get; }

		public Mesh Mesh { get; protected set; }

		public Material Material { get; set; }

		protected GameObject()
		{
			Transform = new Transform();
		}

		public virtual void Update(float deltaTime)
		{
		}
	}
}