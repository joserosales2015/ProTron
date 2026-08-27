using ProTron.Math;

namespace ProTron.Components
{
	public class Transform
	{
		public Vector3f Position { get; set; }

		public Vector3f Rotation { get; set; }

		public Vector3f Scale { get; set; }

		public Transform()
		{
			Position = new Vector3f(0, 0, 0);
			Rotation = new Vector3f(0, 0, 0);
			Scale = new Vector3f(1, 1, 1);
		}

		public Matrix4x4 GetWorldMatrix()
		{
			Matrix4x4 scale =
				Matrix4x4.CreateScale(
					Scale.X,
					Scale.Y,
					Scale.Z);

			Matrix4x4 rotation =
				Matrix4x4.CreateRotationY(Rotation.Y) *
				Matrix4x4.CreateRotationX(Rotation.X) *
				Matrix4x4.CreateRotationZ(Rotation.Z);

			Matrix4x4 translation =
				Matrix4x4.CreateTranslation(
					Position.X,
					Position.Y,
					Position.Z);

			return translation * rotation * scale;
		}
	}
}