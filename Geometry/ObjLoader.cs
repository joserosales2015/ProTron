using ProTron.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

namespace ProTron.Geometry
{
	// Cargador mínimo de Wavefront .obj: posiciones (v), UVs (vt),
	// normales (vn) y caras (f). No soporta materiales (.mtl), grupos
	// ni curvas: para un motor de rasterización esto es lo único que
	// realmente hace falta.
	public static class ObjLoader
	{
		public static Mesh Load(string path)
		{
			if (!File.Exists(path))
				throw new FileNotFoundException(
					$"No se encontró el archivo OBJ: {path}", path);

			List<Vector3f> positions = new();
			List<Vector2> uvs = new();
			List<Vector3f> normals = new();

			Mesh mesh = new();

			// El formato OBJ indexa posición/UV/normal de forma independiente
			// (ej. "f 3/5/2"), pero nuestro Vertex es un único bloque. Este
			// diccionario evita duplicar vértices cuando la misma combinación
			// v/vt/vn aparece en más de una cara.
			Dictionary<(int, int, int), int> vertexCache = new();

			foreach (string rawLine in File.ReadLines(path))
			{
				ReadOnlySpan<char> line = rawLine.AsSpan().Trim();

				if (line.IsEmpty || line[0] == '#')
					continue;

				string[] tokens = rawLine.Split(
					(char[]?)null,
					StringSplitOptions.RemoveEmptyEntries);

				if (tokens.Length == 0)
					continue;

				switch (tokens[0])
				{
					case "v":
						positions.Add(new Vector3f(
							ParseFloat(tokens[1]),
							ParseFloat(tokens[2]),
							ParseFloat(tokens[3])));
						break;

					case "vt":
						uvs.Add(new Vector2(
							ParseFloat(tokens[1]),
							tokens.Length > 2 ? ParseFloat(tokens[2]) : 0f));
						break;

					case "vn":
						normals.Add(new Vector3f(
							ParseFloat(tokens[1]),
							ParseFloat(tokens[2]),
							ParseFloat(tokens[3])));
						break;

					case "f":
						AddFace(
							tokens,
							positions,
							uvs,
							normals,
							vertexCache,
							mesh);
						break;

					// "o", "g", "s", "usemtl", "mtllib", etc. se ignoran a propósito.
				}
			}

			if (mesh.Vertices.Count == 0 || mesh.Triangles.Count == 0)
				throw new InvalidDataException(
					$"El archivo OBJ no contiene geometría válida: {path}");

			return mesh;
		}

		private static void AddFace(
			string[] tokens,
			List<Vector3f> positions,
			List<Vector2> uvs,
			List<Vector3f> normals,
			Dictionary<(int, int, int), int> vertexCache,
			Mesh mesh)
		{
			int cornerCount = tokens.Length - 1;

			if (cornerCount < 3)
				return;

			Span<int> faceVertices = cornerCount <= 16
				? stackalloc int[cornerCount]
				: new int[cornerCount];

			for (int i = 0; i < cornerCount; i++)
			{
				faceVertices[i] = ResolveVertex(
					tokens[i + 1],
					positions,
					uvs,
					normals,
					vertexCache,
					mesh);
			}

			// Triangulación en abanico: válida para triángulos, quads y
			// n-gons convexos, que es todo lo que exportan Blender/etc.
			for (int i = 1; i < cornerCount - 1; i++)
			{
				mesh.Triangles.Add(new Triangle(
					faceVertices[0],
					faceVertices[i],
					faceVertices[i + 1]));
			}
		}

		private static int ResolveVertex(
			string token,
			List<Vector3f> positions,
			List<Vector2> uvs,
			List<Vector3f> normals,
			Dictionary<(int, int, int), int> vertexCache,
			Mesh mesh)
		{
			// Formatos posibles: "v", "v/vt", "v/vt/vn", "v//vn".
			string[] parts = token.Split('/');

			int positionIndex = ResolveIndex(parts[0], positions.Count);
			int uvIndex = parts.Length > 1 && parts[1].Length > 0
				? ResolveIndex(parts[1], uvs.Count)
				: -1;
			int normalIndex = parts.Length > 2 && parts[2].Length > 0
				? ResolveIndex(parts[2], normals.Count)
				: -1;

			var key = (positionIndex, uvIndex, normalIndex);

			if (vertexCache.TryGetValue(key, out int existingIndex))
				return existingIndex;

			Vector3f position = positions[positionIndex];
			Vector2 uv = uvIndex >= 0 ? uvs[uvIndex] : Vector2.Zero;
			Vector3f normal = normalIndex >= 0 ? normals[normalIndex] : Vector3f.Zero;

			int newIndex = mesh.Vertices.Count;
			mesh.Vertices.Add(new Vertex(position, normal, uv));
			vertexCache[key] = newIndex;

			return newIndex;
		}

		private static int ResolveIndex(string token, int count)
		{
			int index = int.Parse(token, CultureInfo.InvariantCulture);

			// OBJ permite índices negativos: cuentan desde el final de la
			// lista acumulada hasta ese punto del archivo.
			return index > 0 ? index - 1 : count + index;
		}

		private static float ParseFloat(string token)
		{
			return float.Parse(token, CultureInfo.InvariantCulture);
		}
	}
}
