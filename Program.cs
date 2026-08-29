using ProTron.Core;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;

namespace ProTron
{
    internal class Program
    {
		public const int InternalWidth = 720;//960;//480;//
		public const int InternalHeight = 405;//540;//270;//

		static void Main(string[] args)
        {
			Viewport viewport = new Viewport(InternalWidth, InternalHeight);//Viewport(480, 270); //Viewport(720, 405); //
			Engine engine = new Engine(viewport);
			Scene scene = new Scene();

			scene.AmbientLight = 0.18f;
			scene.DirectionalLight.Intensity = 0.82f;
			scene.DirectionalLight.WorldDirection = new Vector3f(-0.5f, -1f, 0.3f).Normalized();

			List<GameObject> cubes = new()
			{
				new Cube(2) { Transform = { Position = new Vector3f(-2, 0, 8) }, Material = new ProTron.Objects.Material(0xFF0000dd) },
				new Cube(2) { Transform = { Position = new Vector3f(2, 0, 8) }, Material = new ProTron.Objects.Material(0xFF00dd00) },
				new Cube(2) { Transform = { Position = new Vector3f(0, 0, 8) }, Material = new ProTron.Objects.Material(0xFFdd0000) },
				new Cube(2) { Transform = { Position = new Vector3f(0, -2, 8) }, Material = new ProTron.Objects.Material(0xFFdd00dd) },
				new Cube(2) { Transform = { Position = new Vector3f(0, 2, 8) }, Material = new ProTron.Objects.Material(0xFF00dddd) },
				
			};

			scene.AddRange(cubes);
			engine.LoadScene(scene);
			engine.Run();
		}
    }
}
