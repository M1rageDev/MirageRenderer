using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL.GL;

namespace MirageDev.Mirage
{
	class Program
	{
		RendererSettings settings;
		MirageRenderer? renderer;

		Matrix4 view, projection;

		MirageObject ballObject;
		MirageObject worldObject;
		MirageObject waterObject;
		Scene scene = new();

		int Xsize = 640;
		int Zsize = 640;
		float resolution = 0.1f;
		double perlinFrequency = 50;
		int perlinOctaves = 4;
		Perlin perlin = new();

		Stopwatch timer = new();

		void Load()
		{
			// Load monkey
			Shader shader = new("../../../shaders/experiments/timeWarp.vert", "../../../shaders/experiments/uvmap.frag");
			ballObject = new(new ObjLoader("../../../models/Monkey.obj"), shader);
			ballObject.scale = new(1f, 1f, 1f);

			// Create world
			Mesh worldMesh = new();
			int i = 0;
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					float height = (float)perlin.NoiseOctaves(x / perlinFrequency, 0.5d, z / perlinFrequency, numOctaves: perlinOctaves);
					worldMesh.AddVertex(
						new(x * resolution, height * 2f, z * resolution), 
						new(x / (float)Xsize, z / (float)Zsize)
					);
					i++;
				}
			}

			int vert = 0;
			for (int g = 0; g < Zsize - 1; g++)
			{
				for (int t = 0; t < Xsize - 1; t++)
				{
					worldMesh.AddTriangle(new(vert, vert + 1, vert + Xsize));
					worldMesh.AddTriangle(new(vert + 1, vert + Xsize + 1, vert + Xsize));
					vert++;
				}
				vert++;
			}
			worldMesh.RecalculateNormals();

			Shader worldShader = new("../../../shaders/shader.vert", "../../../shaders/experiments/terrain.frag");
			worldShader.SetVector3("sand", new(1f, 1f, 0.5f));
			worldShader.SetVector3("grass", new(0.25f, 1f, 0.25f));
			worldObject = new(worldMesh, worldShader);

			// Load water
			Shader waterShader = new("../../../shaders/shader.vert", "../../../shaders/lit/solidColor.frag");
			waterShader.SetVector3("color", new(0.5f, 0.5f, 1f));
			waterObject = new(new ObjLoader("../../../models/Plane.obj"), waterShader);
			waterObject.scale = new(64f);
			waterObject.position = new(32f, 0f, 32f);

			// Setup scene
			scene.Add(ballObject);
			scene.Add(worldObject);
			scene.Add(waterObject);
			scene.SetDirectional(new()
			{
				direction = new(-0.2f, -1.0f, -0.3f),
				ambient = new(0.2f),
				diffuse = new(1f),
				specular = new(1f)
			});

			projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)renderer.Size.X / (float)renderer.Size.Y, 0.1f, 100.0f);

			timer.Start();
		}

		void FrameUpdate(MirageRenderer renderer, float dt)
		{
			KeyboardState input = renderer.KeyboardState;

			float t = (float)timer.Elapsed.TotalSeconds * 2f;
		}

		void FrameRender(MirageRenderer renderer)
		{
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			view = renderer.camera.CreateViewMatrix();
			
			ballObject.shader.SetVector3("viewPosition", renderer.camera.position);
			ballObject.shader.SetFloat("time", (float)timer.Elapsed.TotalSeconds * 3);

			scene.SetMVP(view, projection);
			scene.Render(renderer);
		}

		void Unload()
		{
			scene.Unload();
		}

		public void Start()
		{
			settings = new()
			{
				clearColor = new(151f / 255f, 223f / 255f, 252f / 255f, 1.0f),
				cursorState = CursorState.Grabbed,
				onLoad = () => Load(),
				onUnload = () => Unload(),
				onUpdate = (MirageRenderer renderer, float dt) => FrameUpdate(renderer, dt),
				onRender = (MirageRenderer renderer) => FrameRender(renderer),
			};
			
			renderer = new(800, 600, "Mirage rendering test", settings);

			renderer.Run();
		}
	}

	class ProgramStarter
	{
		static void Main(string[] args)
		{
			Program prog = new();
			prog.Start();
		}
	}
}
