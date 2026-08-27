using ProTron.Math;
using Raylib_cs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProTron.Core
{
	public class Camera
	{
		public Components.Transform Transform { get; }

		public float NearPlane { get; set; } = 0.5f;

		public float FarPlane { get; set; } = 1000f;

		public float FieldOfView { get; set; } = 60f;

		public Camera()
		{
			Transform = new Components.Transform();
		}

		public Matrix4x4 GetViewMatrix()
		{
			Matrix4x4 rotation =
				Matrix4x4.CreateRotationX(-Transform.Rotation.X)
			  * Matrix4x4.CreateRotationY(-Transform.Rotation.Y)
			  * Matrix4x4.CreateRotationZ(-Transform.Rotation.Z);

			Matrix4x4 translation =
				Matrix4x4.CreateTranslation(
					-Transform.Position.X,
					-Transform.Position.Y,
					-Transform.Position.Z);

			return rotation * translation;
		}

		public void Update(float deltaTime)
		{
			float speed = 5f;

			var position = Transform.Position;

			if (Raylib.IsKeyDown(KeyboardKey.W))
				position.Z += speed * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.S))
				position.Z -= speed * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.A))
				position.X -= speed * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.D))
				position.X += speed * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Q))
				position.Y -= speed * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.E))
				position.Y += speed * deltaTime;

			Transform.Position = position;
		}
	}
}
