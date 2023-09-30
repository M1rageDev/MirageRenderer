using System;
using System.Globalization;

namespace MirageDev.Mirage
{
	class OBJLoaderOld
	{
		public float[] vertexTable;
		public uint[] triangleTable;

		public OBJLoaderOld(string filename)
		{
			// Parse an OBJ file for vertices and triangles (faces)
			string[] lines = File.ReadAllLines(filename);
			List<float> vertices = new();
			List<float> vt = new();
			List<uint> vtPerVertex = new();
			List<uint> faces = new();

			foreach (string line in lines)
			{
				if (line.StartsWith("v "))
				{
					// add a vertex
					string[] coords = line[2..].Split(' ');
										
					vertices.Add(float.Parse(coords[0], CultureInfo.InvariantCulture.NumberFormat));
					vertices.Add(float.Parse(coords[1], CultureInfo.InvariantCulture.NumberFormat));
					vertices.Add(float.Parse(coords[2], CultureInfo.InvariantCulture.NumberFormat));
				}
				else if (line.StartsWith("f "))
				{
					// add a face (only supports triangles now)
					string[] coords = line[2..].Split(' ');
					if (coords.Length > 3) throw new InvalidDataException("The OBJ file " + filename + "has some non-triangle faces. Please triangulate them.");
					uint x = uint.Parse(coords[0].Split("/")[0], CultureInfo.InvariantCulture.NumberFormat);
					uint y = uint.Parse(coords[1].Split("/")[0], CultureInfo.InvariantCulture.NumberFormat);
					uint z = uint.Parse(coords[2].Split("/")[0], CultureInfo.InvariantCulture.NumberFormat);
					faces.Add(x);
					faces.Add(y);
					faces.Add(z);

					uint tx0 = uint.Parse(coords[0].Split("/")[1], CultureInfo.InvariantCulture.NumberFormat);
					uint tx1 = uint.Parse(coords[1].Split("/")[1], CultureInfo.InvariantCulture.NumberFormat);
					uint tx2 = uint.Parse(coords[2].Split("/")[1], CultureInfo.InvariantCulture.NumberFormat);


				}
				else if (line.StartsWith("vt "))
				{
					// add a vertex texture coordinate
					string[] coords = line[3..].Split(' ');
					float x = float.Parse(coords[0].Split("/")[0], CultureInfo.InvariantCulture.NumberFormat);
					float y = float.Parse(coords[1].Split("/")[0], CultureInfo.InvariantCulture.NumberFormat);
					vt.Add(x);
					vt.Add(y);
				}
			}


			vertexTable = vertices.ToArray();
			triangleTable = faces.ToArray();
		}
	}
}
