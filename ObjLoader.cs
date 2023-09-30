using OpenTK.Mathematics;
using System.Globalization;

namespace MirageDev.Mirage
{
	public class ObjLoader
	{
		public List<float> vertices;
		public List<uint> tris;
		public Mesh mesh;

		public ObjLoader(string filePath)
		{
			vertices = new List<float>();
			tris = new List<uint>();

			List<Vector3> positions = new List<Vector3>();
			List<Vector2> uvs = new List<Vector2>();
			List<Vector3> normals = new List<Vector3>();

			using (StreamReader reader = new StreamReader(filePath))
			{
				while (!reader.EndOfStream)
				{
					string line = reader.ReadLine();
					if (string.IsNullOrWhiteSpace(line)) continue;
					string[] parts = line.Trim().Split(' ');
					if (parts[0] == "v")
					{
						float x = float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
						float y = float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat);
						float z = float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat);
						positions.Add(new Vector3(x, y, z));
					}
					else if (parts[0] == "vt")
					{
						float u = float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
						float v = float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat);
						uvs.Add(new Vector2(u, v));
					}
					else if (parts[0] == "vn")
					{
						float x = float.Parse(parts[1], CultureInfo.InvariantCulture.NumberFormat);
						float y = float.Parse(parts[2], CultureInfo.InvariantCulture.NumberFormat);
						float z = float.Parse(parts[3], CultureInfo.InvariantCulture.NumberFormat);
						normals.Add(new Vector3(x, y, z));
					}
					else if (parts[0] == "f")
					{
						for (int i = 1; i <= 3; i++)
						{
							string[] indices = parts[i].Split('/');
							int posIndex = int.Parse(indices[0], CultureInfo.InvariantCulture.NumberFormat) - 1;
							int uvIndex = int.Parse(indices[1], CultureInfo.InvariantCulture.NumberFormat) - 1;
							int normalIndex = int.Parse(indices[2], CultureInfo.InvariantCulture.NumberFormat) - 1;
							vertices.Add(positions[posIndex].X);
							vertices.Add(positions[posIndex].Y);
							vertices.Add(positions[posIndex].Z);
							vertices.Add(uvs[uvIndex].X);
							vertices.Add(uvs[uvIndex].Y);
							vertices.Add(normals[normalIndex].X);
							vertices.Add(normals[normalIndex].Y);
							vertices.Add(normals[normalIndex].Z);
							tris.Add((uint)(vertices.Count / 8 - 1));
						}
					}
				}
			}

			this.mesh = new(vertices.ToArray(), tris.ToArray());
		}
	}
}
