using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Graphics.OpenGL4;
using System.Diagnostics;
using System;
using OpenTK.Windowing.GraphicsLibraryFramework;
using static OpenTK.Graphics.OpenGL.GL;
using System.Drawing;
using System.Reflection.Metadata;

namespace MirageDev.Mirage
{
	class Program
	{
		RendererSettings settings;
		MirageRenderer? renderer;

		Matrix4 view, projection;

		MirageFrameBuffer screenFB;
		Shader screenShader;
		MirageScreenQuad screenQuad;

		MirageObject worldObject;
		MirageObject waterObject;
		MirageObject sunObject;
		Scene scene = new();

		Texture sandTex;
		Texture grassTex;
		Texture waterTex;

		int Xsize = 640;
		int Zsize = 640;
		float resolution = 0.1f;
		double perlinFrequency = 50;
		double flatFrequency = 200;
		float perlinOffset = 0f;
		int perlinOctaves = 4;
		Perlin perlin = new();

		Stopwatch timer = new();

		Mesh CreateWorld()
		{
			// Create river map
			float[,] riverMap = new float[Xsize, Zsize];
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					riverMap[x, z] = Math.Abs((float)perlin.Noise(x / 100f + perlinOffset, 0.5d, z / 100f + perlinOffset)) < 0.02f ? 1f : 0f;
				}
			}

			// Create continent map
			float[,] biomeMap = new float[Xsize, Zsize];
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					biomeMap[x, z] = (float)perlin.Noise(x / flatFrequency + perlinOffset, 0.5d, z / flatFrequency + perlinOffset);
				}
			}

			// Create world
			Mesh worldMesh = new();
			int i = 0;
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					float height = (float)perlin.NoiseOctaves(x / perlinFrequency + perlinOffset, 0.5d, z / perlinFrequency + perlinOffset, numOctaves: perlinOctaves) * 1.2f;
					if (biomeMap[x, z] > 0)
					{
						// flatten
						height += biomeMap[x, z] * 2f;
					}

					worldMesh.AddVertex(
						new(x * resolution, height * 2f, z * resolution),
						new((float)x / (float)Xsize * 100f, (float)z / (float)Zsize * 100f)
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
			worldMesh.RecalculateTangents();

			return worldMesh;
		}

		void Load()
		{
			sandTex = new("../../../textures/sand.png");
			grassTex = new("../../../textures/grass.png");
			waterTex = new("../../../textures/water.jpg");

			// Load depth FBO and screen shader
			screenFB = new(800, 600);
			screenFB.BindBuffer(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment0);
			screenShader = new("../../../shaders/screen/basic.vert", "../../../shaders/screen/basic.frag");
			screenShader.SetInt("screenTexture", 0);
			screenQuad = new();

			// Load world
			Mesh worldMesh = CreateWorld();
			Shader worldShader = new("../../../shaders/shader.vert", "../../../shaders/experiments/terrain.frag");
			worldShader.SetInt("sandTex", 0);
			worldShader.SetInt("grassTex", 1);
			worldObject = new(worldMesh, worldShader);
			worldObject.textures = new Texture[2] { sandTex, grassTex };

			// Load water
			Shader waterShader = new("../../../shaders/experiments/water.vert", "../../../shaders/experiments/water.frag");
			waterShader.SetInt("tex", 0);
			waterShader.SetInt("depthTex", 3);
			waterObject = new(new ObjLoader("../../../models/Plane.obj"), waterShader);
			waterObject.textures = new Texture[1] { waterTex };
			waterObject.scale = new(32f, 1f, 32f);
			waterObject.position = new(32f, 0f, 32f);

			// Load sun
			Shader sunShader = new("../../../shaders/shader.vert", "../../../shaders/unlit/solidColor.frag");
			sunShader.SetVector3("color", new(1f));
			sunObject = new(new ObjLoader("../../../models/Sphere.obj"), sunShader);
			sunObject.scale = new(2f);

			// Setup scene
			scene.Add(worldObject);
			scene.Add(waterObject);
			scene.Add(sunObject);
			scene.SetDirectional(new()
			{
				direction = new(-0.2f, -1.0f, -0.3f),
				ambient = new(0.2f),
				diffuse = new(1f),
				specular = new(1f)
			});
			sunObject.position = new Vector3(32f, 0f, 32f) + -scene.directionalLight.direction * 10f;

			projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)renderer.Size.X / (float)renderer.Size.Y, 0.1f, 1000.0f);

			timer.Start();
		}

		void FrameUpdate(MirageRenderer renderer, float dt)
		{
			KeyboardState input = renderer.KeyboardState;

			float t = (float)timer.Elapsed.TotalSeconds;

			view = renderer.camera.CreateViewMatrix();
			waterObject.shader.SetFloat("time", t);
			waterObject.shader.SetVector3("viewPos", renderer.camera.position);
			scene.SetMVP(view, projection);
		}

		void FrameRender(MirageRenderer renderer)
		{
			// first pass (color/depth)
			GL.Enable(EnableCap.DepthTest);
			screenFB.Use();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			screenFB.UseDepthTexture(TextureUnit.Texture3);
			scene.Render(renderer);

			// second pass (screen quad)
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
			GL.Disable(EnableCap.DepthTest);
			GL.Clear(ClearBufferMask.ColorBufferBit);
			screenFB.UseTexture(TextureUnit.Texture0);
			
			screenShader.Use();
			screenQuad.Draw();

			renderer.SwapBuffers();
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
