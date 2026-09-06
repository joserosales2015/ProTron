using ProTron.Math;

namespace ProTron.Components
{
	public class Transform
	{
		private Vector3f _position;
		private Vector3f _rotation;
		private Vector3f _scale;
		private Matrix4x4 _worldMatrix;
		private bool _isDirty = true;

		public Vector3f Position
		{
			get => _position;
			set
			{
				if (_position == value)
					return;

				_position = value;
				_isDirty = true;
			}
		}

		public Vector3f Rotation
		{
			get => _rotation;
			set
			{
				if (_rotation == value)
					return;

				_rotation = value;
				_isDirty = true;
			}
		}

		public Vector3f Scale
		{
			get => _scale;
			set
			{
				if (_scale == value)
					return;

				_scale = value;
				_isDirty = true;
			}
		}

		public Transform()
		{
			_position = Vector3f.Zero;
			_rotation = Vector3f.Zero;
			_scale = Vector3f.One;
			_worldMatrix = Matrix4x4.Identity();
		}

		public Matrix4x4 GetWorldMatrix()
		{
			if (!_isDirty)
				return _worldMatrix;

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

			_worldMatrix = translation * rotation * scale;
			_isDirty = false;

			return _worldMatrix;
		}
	}
}
