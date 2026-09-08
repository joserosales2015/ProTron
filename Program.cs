using ProTron.Core;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;
using Model = ProTron.Objects.Model;

namespace ProTron
{
    internal class Program
    {
		public const int InternalWidth = 960;//720;//480;//
		public const int InternalHeight = 540;//405;//270;//

		static void Main(string[] args)
        {
			Viewport viewport = new Viewport(InternalWidth, InternalHeight);//Viewport(480, 270); //Viewport(720, 405); //
			Engine engine = new Engine(viewport);
			Scene scene = new Scene();

			scene.Camera.FieldOfView = 60f;
			scene.Camera.NearPlane = 0.5f;
			scene.AmbientLight = 0.18f;
			scene.DirectionalLight.Intensity = 0.82f;
			scene.DirectionalLight.WorldDirection = new Vector3f(0.0f, -1.0f, -1.0f).Normalized();

			// Un checkerboard procedural es la mejor forma de verificar que
			// las UV de una malla cargada desde archivo están bien: si se ve
			// distorsionado o con costuras raras, el problema es la UV, no el
			// renderer.
			Texture uvCheck = Texture.CreateCheckerboard(
				256, 256, 8,
				ColorUtils.PackRgba(230, 230, 230),
				ColorUtils.PackRgba(40, 110, 200));

			List<GameObject> models = new()
			{
				new Model("Assets/Models/torus.obj", new Vector3f(15f, 25f, 0f))
				{
					Transform = { Position = new Vector3f(-3.5f, 0, 10) },
					Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255))
					{
						Texture = uvCheck
					}
				},
				new Model("Assets/Models/mobius.obj", new Vector3f(0f, 30f, 0f))
				{
					Transform = { Position = new Vector3f(0f, 0, 10) },
					Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255))
					{
						Texture = uvCheck
					}
				},
				new Model("Assets/Models/face.obj", new Vector3f(0f, 20f, 0f))
				{
					Transform = { Position = new Vector3f(3.5f, 0, 10) },
					Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255))
					{
						Texture = uvCheck
					}
				},
			};


			scene.AddRange(models);
			engine.LoadScene(scene);
			engine.Run();
		}
    }
}
