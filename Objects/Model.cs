using ProTron.Geometry;
using ProTron.Math;

namespace ProTron.Objects
{
	// GameObject de propósito general para cualquier malla cargada desde
	// disco (a diferencia de Cube, que construye su geometría a mano).
	public class Model : GameObject
	{
		public Vector3f RotationSpeed { get; set; } = Vector3f.Zero;

		public Model(string objPath)
		{
			Mesh = ObjLoader.Load(objPath);
		}

		public Model(string objPath, Vector3f rotationSpeed)
			: this(objPath)
		{
			RotationSpeed = rotationSpeed;
		}

		public override void Update(float deltaTime)
		{
			if (RotationSpeed == Vector3f.Zero)
				return;

			var rotation = Transform.Rotation;
			rotation += RotationSpeed * deltaTime;
			Transform.Rotation = rotation;
		}
	}
}
