using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace MirageDev.Mirage
{
	public class Mesh
	{
		public List<float> vertices;
		public List<uint> tris;

		public List<int[]> m_Triangles = new();
		public List<Vector3> m_Vertices = new();

		public Mesh()
		{
			this.vertices = new();
			this.tris = new();
		}

		public Mesh(float[] vertices, uint[] tris)
		{
			this.vertices = vertices.ToList();
			this.tris = tris.ToList();
		}

		public void AddVertex(Vector3 pos)
		{
			vertices.Add(pos.X);
			vertices.Add(pos.Y);
			vertices.Add(pos.Z);
			vertices.Add(0);
			vertices.Add(0);
			vertices.Add(0);
			vertices.Add(0);
			vertices.Add(0);
			m_Vertices.Add(pos);
		}

		public void AddVertex(Vector3 pos, Vector2 uv)
		{
			vertices.Add(pos.X);
			vertices.Add(pos.Y);
			vertices.Add(pos.Z);
			vertices.Add(uv.X);
			vertices.Add(uv.Y);
			vertices.Add(0);
			vertices.Add(0);
			vertices.Add(0);
			m_Vertices.Add(pos);
		}

		public void AddVertex(Vector3 pos, Vector2 uv, Vector3 normal)
		{
			vertices.Add(pos.X);
			vertices.Add(pos.Y);
			vertices.Add(pos.Z);
			vertices.Add(uv.X);
			vertices.Add(uv.Y);
			vertices.Add(normal.X);
			vertices.Add(normal.Y);
			vertices.Add(normal.Z);
			m_Vertices.Add(pos);
		}

		public void AddTriangle(Vector3i triangle)
		{
			tris.Add((uint)triangle.X);
			tris.Add((uint)triangle.Y);
			tris.Add((uint)triangle.Z);
			m_Triangles.Add(new int[3] { triangle.X, triangle.Y, triangle.Z });
		}

		Vector3 ComputeFaceNormal(Vector3 p1, Vector3 p2, Vector3 p3)
		{
			var a = p3 - p2;
			var b = p1 - p2;
			return Vector3.Cross(a, b);
		}

		public void RecalculateNormals()
		{
			foreach (var tri in m_Triangles)
			{
				Vector3 A = m_Vertices[tri[0]];
				Vector3 B = m_Vertices[tri[1]];
				Vector3 C = m_Vertices[tri[2]];

				Vector3 normal = ComputeFaceNormal(A, B, C).Normalized();
				vertices[tri[0] * 8 + 5] = normal.X;
				vertices[tri[0] * 8 + 6] = normal.Y;
				vertices[tri[0] * 8 + 7] = normal.Z;
				vertices[tri[1] * 8 + 5] = normal.X;
				vertices[tri[1] * 8 + 6] = normal.Y;
				vertices[tri[1] * 8 + 7] = normal.Z;
				vertices[tri[2] * 8 + 5] = normal.X;
				vertices[tri[2] * 8 + 6] = normal.Y;
				vertices[tri[2] * 8 + 7] = normal.Z;
			}
		}
	}

	public class MirageObject
	{
		public Mesh mesh;
		public Shader shader;
		public Texture texture0;
		public Texture texture1;
		public Material material;

		public Vector3 position = new(0f, 0f, 0f);
		public Vector3 scale = new(1f,1f, 1f);
		public Vector3 rotation = new(0f, 0f, 0f);

		int VertexBufferObject;
		int ElementBufferObject;
		int VertexArrayObject;

		public MirageObject() { }

		public MirageObject(ObjLoader mesh, Shader shader, Texture texture, Material material)
		{
			this.mesh = mesh.mesh;
			this.shader = shader;
			texture0 = texture;
			this.material = material;
			material.Assign(this.shader);

			GenBuffers();
			shader.Use();
		}

		public MirageObject(Mesh mesh, Shader shader, Texture texture, Material material)
		{
			this.mesh = mesh;
			this.shader = shader;
			texture0 = texture;
			this.material = material;
			material.Assign(this.shader);

			GenBuffers();
			shader.Use();
		}

		public MirageObject(ObjLoader mesh, Shader shader)
		{
			this.mesh = mesh.mesh;
			this.shader = shader;

			GenBuffers();
			shader.Use();
		}

		public MirageObject(Mesh mesh, Shader shader)
		{
			this.mesh = mesh;
			this.shader = shader;

			GenBuffers();
			shader.Use();
		}

		public void GenVBO()
		{
			VertexBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
			GL.BufferData(BufferTarget.ArrayBuffer, mesh.vertices.Count * sizeof(float), mesh.vertices.ToArray(), BufferUsageHint.StreamDraw);
		}

		public void GenVAO()
		{
			VertexArrayObject = GL.GenVertexArray();
			GL.BindVertexArray(VertexArrayObject);
			GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 0);
			GL.EnableVertexAttribArray(0);
			GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, 8 * sizeof(float), 3 * sizeof(float));
			GL.EnableVertexAttribArray(1);
			GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 8 * sizeof(float), 5 * sizeof(float));
			GL.EnableVertexAttribArray(2);
		}

		public void GenEBO()
		{
			ElementBufferObject = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.BufferData(BufferTarget.ElementArrayBuffer, mesh.tris.Count * sizeof(uint), mesh.tris.ToArray(), BufferUsageHint.StreamDraw);
		}

		public void GenBuffers()
		{
			GenVBO();
			GenVAO();
			GenEBO();
		}

		public Matrix4 GenTransformMatrix()
		{
			return Matrix4.CreateScale(scale) *
				Matrix4.CreateRotationX(rotation.X) *
				Matrix4.CreateRotationY(rotation.Y) *
				Matrix4.CreateRotationZ(rotation.Z) *
				Matrix4.CreateTranslation(position);
		}

		public void Render()
		{
			texture0?.Use();
			shader.Use();

			GL.BindVertexArray(VertexArrayObject);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementBufferObject);
			GL.DrawElements(PrimitiveType.Triangles, mesh.tris.Count, DrawElementsType.UnsignedInt, 0);
		}
	}

	public class Scene
	{
		public List<MirageObject> objects = new();
		public List<Light> lights = new();
		public DirectionalLight directionalLight = new();

		public void Add(MirageObject x)
		{
			objects.Add(x);
		}

		public void AddLight(Light x)
		{
			lights.Add(x);
		}

		public void SetDirectional(DirectionalLight x)
		{
			directionalLight = x;
		}

		public void UpdateLight(int index, Light x)
		{
			lights[index] = x;
		}

		public void Render(MirageRenderer renderer)
		{
			int p = 0;
			int s = 0;
			foreach (Light light in lights)
			{
				if (light is PointLight)
				{
					p++;
				} else
				{
					s++;
				}
			}

			foreach (MirageObject e in objects)
			{
				e.shader.SetInt("actualPointLightLen", p);
				e.shader.SetInt("actualSpotLightLen", s);
				e.Render();
			}
			renderer.SwapBuffers();
		}

		public void SetMVP(Matrix4 view, Matrix4 projection)
		{
			foreach (MirageObject e in objects)
			{
				directionalLight.Assign(e.shader);
				int lI = 0;
				foreach (Light light in lights)
				{
					if (light is PointLight || light is SpotLight)
					{
						light.Assign(e.shader, lI);
					} else
					{
						throw new Exception("Invalid light type: Directional. Specify ONLY point and spot lights in the array.");
					}
					lI++;
				}
				e.shader.SetMatrix4("view", view);
				e.shader.SetMatrix4("projection", projection);
				e.shader.SetMatrix4("transform", e.GenTransformMatrix());
			}
		}

		public void Unload()
		{
			foreach (var obj in objects)
			{
				obj.shader.Dispose();
			}
		}
	}

	public abstract class Light
	{
		public Vector3 ambient;
		public Vector3 diffuse;
		public Vector3 specular;

		public abstract void Assign(Shader shader, int index=0);
	}

	public class DirectionalLight : Light
	{
		public Vector3 direction;

		public override void Assign(Shader shader, int index=0)
		{
			shader.SetVector3("directionalLight.direction", direction);
			shader.SetVector3("directionalLight.ambient", ambient);
			shader.SetVector3("directionalLight.diffuse", diffuse);
			shader.SetVector3("directionalLight.specular", specular);
		}
	}

	public class PointLight : Light
	{
		public Vector3 position;

		public float Kc;
		public float Kl;
		public float Kq;

		public override void Assign(Shader shader, int index)
		{
			shader.SetVector3(string.Format("pointLights[{0}].position", index), position);
			shader.SetVector3(string.Format("pointLights[{0}].ambient", index), ambient);
			shader.SetVector3(string.Format("pointLights[{0}].diffuse", index), diffuse);
			shader.SetVector3(string.Format("pointLights[{0}].specular", index), specular);
			shader.SetFloat(string.Format("pointLights[{0}].Kc", index), Kc);
			shader.SetFloat(string.Format("pointLights[{0}].Kl", index), Kl);
			shader.SetFloat(string.Format("pointLights[{0}].Kq", index), Kq);
		}
	}

	public class SpotLight : Light
	{
		public Vector3 position;
		public Vector3 direction;
		public float cutOff;
		public float outerCutOff;

		public float Kc;
		public float Kl;
		public float Kq;

		public override void Assign(Shader shader, int index)
		{
			shader.SetVector3(string.Format("spotLights[{0}].position", index), position);
			shader.SetVector3(string.Format("spotLights[{0}].direction", index), direction);
			shader.SetFloat(string.Format("spotLights[{0}].cutOff", index), cutOff);
			shader.SetFloat(string.Format("spotLights[{0}].outerCutOff", index), outerCutOff);
			shader.SetVector3(string.Format("spotLights[{0}].ambient", index), ambient);
			shader.SetVector3(string.Format("spotLights[{0}].diffuse", index), diffuse);
			shader.SetVector3(string.Format("spotLights[{0}].specular", index), specular);
			shader.SetFloat(string.Format("spotLights[{0}].Kc", index), Kc);
			shader.SetFloat(string.Format("spotLights[{0}].Kl", index), Kl);
			shader.SetFloat(string.Format("spotLights[{0}].Kq", index), Kq);
		}
	}

	public class Material
	{
		public int diffuseTex;
		public int specularTex;
		public float shininess;

		public void Assign(Shader shader)
		{
			shader.SetInt("material.diffuse", diffuseTex);
			shader.SetInt("material.specular", specularTex);
			shader.SetFloat("material.shininess", shininess);
		}
	}

	public struct RendererSettings
	{
		public Color4 clearColor;
		public CursorState cursorState;
		public Action onLoad;
		public Action<MirageRenderer, float> onUpdate;
		public Action<MirageRenderer> onRender;
		public Action onUnload;
	}

	public class MirageRenderer : GameWindow
	{
		public Camera camera = new();
		public RendererSettings settings;

		Vector2 lastPos;
		bool firstMove = true;
		float movementSpeed = 10f;
		float mouseSensitivity = 0.005f;

		public MirageRenderer(int width, int height, string title, RendererSettings settings) : 
			base(GameWindowSettings.Default, new NativeWindowSettings() 
			{ Size = (width, height), Title = title }) 
		{
			this.settings = settings;
		}

		protected override void OnResize(ResizeEventArgs e)
		{
			base.OnResize(e);

			GL.Viewport(0, 0, e.Width, e.Height);
		}

		protected override void OnLoad()
		{
			base.OnLoad();

			GL.Enable(EnableCap.DepthTest);

			CursorState = settings.cursorState;
			GL.ClearColor(settings.clearColor);
			GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

			settings.onLoad();
		}

		protected override void OnUpdateFrame(FrameEventArgs args)
		{
			base.OnUpdateFrame(args);

			KeyboardState input = KeyboardState;
			MouseState mouse = MouseState;

			if (input.IsKeyDown(Keys.Escape))
			{
				Close();
			}

			if (firstMove)
			{
				lastPos = new Vector2(mouse.X, mouse.Y);
				firstMove = false;
			}

			float deltaX = mouse.X - lastPos.X;
			float deltaY = mouse.Y - lastPos.Y;
			lastPos = new(mouse.X, mouse.Y);
			camera.yaw += deltaX * mouseSensitivity;
			camera.pitch += -deltaY * mouseSensitivity;

			if (input.IsKeyDown(Keys.W))
			{
				camera.position += camera.front * movementSpeed * (float)args.Time;
			}

			if (input.IsKeyDown(Keys.S))
			{
				camera.position -= camera.front * movementSpeed * (float)args.Time;
			}

			if (input.IsKeyDown(Keys.A))
			{
				camera.position -= camera.right * movementSpeed * (float)args.Time;
			}

			if (input.IsKeyDown(Keys.D))
			{
				camera.position += camera.right * movementSpeed * (float)args.Time;
			}

			if (input.IsKeyDown(Keys.Space))
			{
				camera.position += camera.up * movementSpeed * (float)args.Time;
			}

			if (input.IsKeyDown(Keys.LeftControl))
			{
				camera.position -= camera.up * movementSpeed * (float)args.Time;
			}

			camera.Update();

			settings.onUpdate(this, (float)args.Time);
		}

		protected override void OnRenderFrame(FrameEventArgs args)
		{
			base.OnRenderFrame(args);

			settings.onRender(this);
		}

		protected override void OnUnload()
		{
			base.OnUnload();

			settings.onUnload();
		}
	}
}
