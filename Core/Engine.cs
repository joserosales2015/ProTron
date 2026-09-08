using ProTron.Buffer;
using ProTron.Graphics;
using ProTron.Math;
using ProTron.Objects;
using Raylib_cs;
using System.Diagnostics;

namespace ProTron.Core
{
	public class Engine
	{
		private readonly Viewport _viewport;
		private Renderer? _renderer;
		private Scene? _scene;
		private readonly Display _display;
		private readonly FrameBuffer _frameBuffer;
		private readonly DepthBuffer _depthBuffer;
		private readonly Rasterizer _rasterizer;
		private readonly RenderQueue _renderQueue = new();
		#if DEBUG
		private float _lastUpdateTime;
		#endif

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
			Scene scene = _scene ?? throw new InvalidOperationException(
				"Debe cargar una escena antes de iniciar el motor.");
			_renderer ??= new Renderer(_viewport, scene, _rasterizer);

			//Raylib.SetConfigFlags(ConfigFlags.FullscreenMode);
			Raylib.InitWindow(_viewport.Width, _viewport.Height, "ProTron");
			bool displayInitialized = false;

			try
			{
				LoadSceneTextures(scene);
				Raylib.SetTargetFPS(150);
				_display.Initialize();
				displayInitialized = true;

				while (!Raylib.WindowShouldClose())
				{
					#if DEBUG
					long updateStart = Stopwatch.GetTimestamp();
					#endif
					Update();
					#if DEBUG
					_lastUpdateTime = (float)Stopwatch.GetElapsedTime(updateStart).TotalMilliseconds;
					#endif
					Draw();
				}
			}
			finally
			{
				if (displayInitialized)
					_display.Unload();

				Raylib.CloseWindow();
			}
		}

		private void Draw()
		{
			Renderer renderer = _renderer ?? throw new InvalidOperationException(
				"El renderizador no está inicializado.");

			Raylib.BeginDrawing();
			Raylib.ClearBackground(Color.Black);
			#if DEBUG
			long renderStart = Stopwatch.GetTimestamp();
			#endif

			_frameBuffer.Clear(ColorUtils.PackRgba(0, 0, 0));
			_depthBuffer.Clear();
			renderer.Stats.BeginFrame();
			renderer.BeginFrame();

			foreach (GameObject obj in _renderQueue.OpaqueObjects)
			{
				renderer.Draw(obj);
			}

			renderer.FlushOpaqueTriangles();

			foreach (GameObject obj in _renderQueue.TransparentObjects)
			{
				renderer.Draw(obj);
			}

			#if DEBUG
			float renderTime = (float)Stopwatch.GetElapsedTime(renderStart).TotalMilliseconds;
			long presentStart = Stopwatch.GetTimestamp();
			#endif
			_display.Present();
			#if DEBUG
			float presentTime = (float)Stopwatch.GetElapsedTime(presentStart).TotalMilliseconds;
			renderer.Stats.UpdateStageTimes(_lastUpdateTime, renderTime, presentTime);
			#endif
			renderer.Stats.EndFrame();
			Debugging.DebugOverlay.Draw(renderer.Stats);

			Raylib.EndDrawing();
		}

		public void LoadScene(Scene scene)
		{
			ArgumentNullException.ThrowIfNull(scene);
			_scene = scene;
			_renderer = new Renderer(_viewport, _scene, _rasterizer);
		}

		private static void LoadSceneTextures(Scene scene)
		{
			Dictionary<string, Texture> loadedTextures = new();

			foreach (GameObject obj in scene.Objects)
			{
				string? path = obj.Material.TexturePath;

				if (string.IsNullOrWhiteSpace(path))
					continue;

				if (!loadedTextures.TryGetValue(path, out Texture? texture))
				{
					texture = Texture.LoadFromFile(path);
					loadedTextures.Add(path, texture);
				}

				obj.Material.Texture = texture;
			}
		}

		private void Update()
		{
			Scene scene = _scene ?? throw new InvalidOperationException(
				"No hay una escena cargada.");
			float deltaTime = Raylib.GetFrameTime();

			scene.Camera.Update(deltaTime);

			// Solución para CS1612: Asignar la estructura completa, modificar y reasignar
			var rotation = scene.Camera.Transform.Rotation;

			if (Raylib.IsKeyDown(KeyboardKey.Left))
				rotation.Y -= 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Right))
				rotation.Y += 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Up))
				rotation.X -= 90 * deltaTime;

			if (Raylib.IsKeyDown(KeyboardKey.Down))
				rotation.X += 90 * deltaTime;

			scene.Camera.Transform.Rotation = rotation;

			foreach (GameObject obj in scene.Objects)
			{
				obj.Update(deltaTime);
			}

			_renderQueue.Build(scene.Objects, scene.Camera);
		}
	}
}
