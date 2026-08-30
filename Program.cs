using ProTron.Core;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;

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

			List<GameObject> cubes = new()
			{
				new Cube(2) { Transform = { Position = new Vector3f(-2, 0, 8) }, Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255)) { TexturePath = "Assets/696.png" } },
				new Cube(2) { Transform = { Position = new Vector3f(2, 0, 8) }, Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255)) { TexturePath = "Assets/696.png" } },
				new Cube(2) { Transform = { Position = new Vector3f(0, 0, 8) }, Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255)) { TexturePath = "Assets/696.png" } },
				new Cube(2) { Transform = { Position = new Vector3f(0, -2, 8) }, Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255)) { TexturePath = "Assets/696.png" } },
				new Cube(2) { Transform = { Position = new Vector3f(0, 2, 8) }, Material = new ProTron.Objects.Material(ColorUtils.PackRgba(255, 255, 255)) { TexturePath = "Assets/696.png" } },
			};

			scene.AddRange(cubes);
			engine.LoadScene(scene);
			engine.Run();
		}
    }
}
