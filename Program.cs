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
		Texture depthTex;
		Shader screenShader;
		MirageScreenQuad screenQuad;

		MirageObject worldObject;
		MirageObject waterObject;
		MirageObject[] trees;
		Scene scene = new();

		int Xsize = 512;
		float Ymult = 7;
		int Zsize = 512;
		float resolution = 0.5f;
		double perlinFrequency = 50;
		double flatFrequency = 200;
		float perlinOffset = 0f;
		int perlinOctaves = 4;
		Perlin perlin = new();

		int waterXsize = 1024;
		int waterZsize = 1024;
		float waterResolution = 0.25f;

		float[,] heightMap;

		Stopwatch timer = new();

		Mesh CreateWorld()
		{
			// Create continent map
			float[,] biomeMap = new float[Xsize, Zsize];
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					biomeMap[x, z] = (float)perlin.Noise(x / flatFrequency + perlinOffset, 0.5d, z / flatFrequency + perlinOffset);
				}
			}

			// Create heightmap
			heightMap = new float[Xsize, Zsize];
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					float height = (float)perlin.NoiseOctaves(x / perlinFrequency + perlinOffset, 0.5d, z / perlinFrequency + perlinOffset, numOctaves: perlinOctaves) * 1.2f;
					if (biomeMap[x, z] > 0)
					{
						// flatten
						height += biomeMap[x, z];
					}
					heightMap[x, z] = height * Ymult;
				}
			}

			// Create world
			Mesh worldMesh = new();
			int i = 0;
			for (int x = 0; x < Xsize; x++)
			{
				for (int z = 0; z < Zsize; z++)
				{
					float height = heightMap[x, z];

					worldMesh.AddVertex(
						new(x * resolution, height, z * resolution),
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
					worldMesh.AddTriangle(new(vert, vert + Zsize, vert + Zsize + 1));
					worldMesh.AddTriangle(new(vert + Zsize + 1, vert + 1, vert));
					vert++;
				}
				vert++;
			}
			worldMesh.RecalculateNormals(true);

			return worldMesh;
		}

		Mesh CreateWater()
		{
			// Create water
			Mesh waterMesh = new();
			int i = 0;
			for (int x = 0; x < waterXsize; x++)
			{
				for (int z = 0; z < waterZsize; z++)
				{
					waterMesh.AddVertex(
						new(x * waterResolution, 0f, z * waterResolution),
						new((float)x / (float)waterXsize * 100f, (float)z / (float)waterZsize * 100f)
					);
					i++;
				}
			}

			int vert = 0;
			for (int g = 0; g < waterZsize - 1; g++)
			{
				for (int t = 0; t < waterXsize - 1; t++)
				{
					waterMesh.AddTriangle(new(vert, vert + waterZsize, vert + waterZsize + 1));
					waterMesh.AddTriangle(new(vert + waterZsize + 1, vert + 1, vert));
					vert++;
				}
				vert++;
			}
			waterMesh.RecalculateNormals(true);

			return waterMesh;
		}

		void PlaceTrees(int probability=20)
		{
			List<MirageObject> treeList = new();
			Random random = new();

			for (int x = 0; x < Xsize / 4; x++)
			{
				for (int z = 0; z < Zsize / 4; z++)
				{
					if (heightMap[z * 4, x * 4] > 1f && heightMap[z * 4, x * 4] < 4f && random.Next(probability) == probability / 2)
					{
						// place tree
						Shader treeShader = new("../../../shaders/shader.vert", "../../../shaders/lit/textured.frag");
						treeShader.SetInt("tex", 0);
						MirageObject tree = new(new ObjLoader("../../../models/treeNew.obj"), treeShader);
						tree.mesh.RecalculateNormals(true);
						tree.position = new(z*2, heightMap[z*4, x*4], x*2);
						tree.scale = new(0.75f);
						tree.textures = new Texture[1] { new("../../../textures/texture_gradient.png") };
						treeList.Add(tree);
					}
				}
			}

			trees = treeList.ToArray();
		}

		void Load()
		{
			// Load screen/depth FBO and screen shader
			screenFB = new(800, 600);
			screenFB.BindBuffer(PixelInternalFormat.Rgba, PixelFormat.Rgba, PixelType.Byte, FramebufferAttachment.ColorAttachment0);
			screenShader = new("../../../shaders/screen/basic.vert", "../../../shaders/screen/basic.frag");
			screenShader.SetInt("screenTexture", 0);
			screenQuad = new();

			// Load world
			Mesh worldMesh = CreateWorld();
			Shader worldShader = new("../../../shaders/experiments/terrain.vert", "../../../shaders/experiments/terrain.frag");
			worldShader.SetArray("terrainColors", 
				new Vector3[4] { 
					new(201f / 255f, 178f / 255f, 99f / 255f), 
					new(0.350f, 0.9f, 0.350f),
					new(148f / 255f, 149f / 255f, 145f / 255f),
					new(0.9f, 0.9f, 0.9f)
				}
			);
			worldShader.SetArray("terrainHeights",
				new float[4] {
					-7f,
					0f,
					4f,
					5f
				}
			);
			worldObject = new(worldMesh, worldShader);

			// Load water
			Mesh waterMesh = CreateWater();
			Shader waterShader = new("../../../shaders/experiments/water.vert", "../../../shaders/experiments/water.frag");
			waterShader.SetInt("tex", 0);
			waterShader.SetInt("depthTex", 3);
			waterObject = new(waterMesh, waterShader);
			waterObject.onRenderAction = new(OnWaterRender);
			depthTex = new(new(), 800, 600, PixelInternalFormat.DepthComponent, PixelFormat.DepthComponent, PixelType.Float);

			// Place trees
			PlaceTrees();
			foreach (MirageObject tree in trees)
			{
				scene.Add(tree);
			}

			// Setup scene
			scene.Add(worldObject);
			scene.Add(waterObject);
			scene.SetDirectional(new()
			{
				direction = new(-0.2f, -1.0f, -0.3f),
				ambient = new(0.2f),
				diffuse = new(1f),
				specular = new(1f)
			});
			waterShader.SetVector3("lightDir", scene.directionalLight.direction);

			GL.ProvokingVertex(ProvokingVertexMode.FirstVertexConvention);

			projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60.0f), (float)renderer.Size.X / (float)renderer.Size.Y, 0.1f, 1000.0f);

			timer.Start();
		}

		void OnWaterRender(MirageObject water)
		{
			depthTex.Use(TextureUnit.Texture3);
			GL.CopyTexImage2D(TextureTarget.Texture2D, 0, InternalFormat.DepthComponent, 0, 0, 800, 600, 0);
		}

		void FrameUpdate(MirageRenderer renderer, float dt)
		{
			KeyboardState input = renderer.KeyboardState;

			float t = (float)timer.Elapsed.TotalSeconds * 0.5f;

			view = renderer.camera.CreateViewMatrix();
			waterObject.shader.SetFloat("time", t);
			waterObject.shader.SetVector3("viewPos", renderer.camera.position);
			scene.SetMVP(view, projection);
		}

		void FrameRender(MirageRenderer renderer)
		{
			// first pass (color)
			GL.Enable(EnableCap.DepthTest);
			screenFB.Use();
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			scene.Render(renderer);

			// second pass (screen quad)
			screenFB.Unbind();
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
				clearColor = Color.SkyBlue,
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
