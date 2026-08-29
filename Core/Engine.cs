using ProTron.Buffer;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;

namespace ProTron.Core
{
	public class Engine
	{
		private readonly Viewport _viewport;
		private Renderer _renderer;
		private Scene _scene;
		private readonly Display _display;
		private readonly FrameBuffer _frameBuffer;
		private readonly DepthBuffer _depthBuffer;
		private readonly Rasterizer _rasterizer;

		public Engine(Viewport viewport)
		{
			_viewport = viewport;
			_frameBuffer = new FrameBuffer(viewport.Width, viewport.Height);
			_display = new Display(_frameBuffer);
			_depthBuffer = new DepthBuffer(viewport.Width, viewport.Height);
			_rasterizer = new Rasterizer(_frameBuffer, _depthBuffer);
		}

		public void Run()
		{
			//Raylib.SetConfigFlags(ConfigFlags.FullscreenMode);
			Raylib.InitWindow(_viewport.Width, _viewport.Height, "ProTron");
			Raylib.SetTargetFPS(150);
			_display.Initialize();

			while (!Raylib.WindowShouldClose())
			{
				Update();
				Draw();
			}
			
			_display.Unload();
			Raylib.CloseWindow();
		}

		private void Update()
		{
			float deltaTime = Raylib.GetFrameTime();

			_scene.Camera.Update(deltaTime);

			// Solución para CS1612: Asignar la estructura completa, modificar y reasignar
			var rotation = _scene.Camera.Transform.Rotation;

			if (Raylib.IsKeyDown(KeyboardKey.Left))
				rotation.Y -= 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Right))
				rotation.Y += 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Up))
				rotation.X -= 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Down))
				rotation.X += 90 * deltaTime;

			_scene.Camera.Transform.Rotation = rotation;

			foreach (GameObject obj in _scene.Objects)
			{
				obj.Update(deltaTime);
			}

			_scene.SortFrontToBack(_scene.Camera);
		}

		private void Draw()
		{
			Raylib.BeginDrawing();

			_frameBuffer.Clear(0xFF000000);
			_depthBuffer.Clear();
			_renderer.Stats.BeginFrame();
			_renderer.BeginFrame();

			foreach (var obj in _scene.Objects)
			{
				_renderer.Draw(obj);
			}

			_renderer.Stats.EndFrame();
			
			_display.Present();
			Debugging.DebugOverlay.Draw(_renderer.Stats);

			Raylib.EndDrawing();
		}

		public void LoadScene(Scene scene)
		{
			_scene = scene;
			_renderer = new Renderer(_viewport, _scene, _rasterizer);
		}
	}
}