using ProTron.Core;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;

namespace ProTron
{
    internal class Program
    {
		public const int InternalWidth = 960;
		public const int InternalHeight = 540;

		static void Main(string[] args)
        {
			Viewport viewport = new Viewport(InternalWidth, InternalHeight);//Viewport(480, 270); //Viewport(720, 405); //
			Engine engine = new Engine(viewport);
			Scene scene = new Scene();

			List<GameObject> cubes = new()
			{
				new Cube(2) { Transform = { Position = new Vector3f(-2, 0, 8) }, Material = new ProTron.Objects.Material(0xFF000000) },
				new Cube(2) { Transform = { Position = new Vector3f(2, 0, 8) }, Material = new ProTron.Objects.Material(0xFF000000) },
				new Cube(2) { Transform = { Position = new Vector3f(0, 0, 8) }, Material = new ProTron.Objects.Material(0xFF000000) },
				new Cube(2) { Transform = { Position = new Vector3f(0, -2, 8) }, Material = new ProTron.Objects.Material(0xFF000000) },
				new Cube(2) { Transform = { Position = new Vector3f(0, 2, 8) }, Material = new ProTron.Objects.Material(0xFF000000) },
				
			};

			scene.AddRange(cubes);
			engine.LoadScene(scene);
			engine.Run();
		}
    }
}
