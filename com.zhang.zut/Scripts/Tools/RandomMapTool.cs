//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//namespace MyTool.Tools
//{
//	public static class RandomMapTool
//	{
//		public enum NormalizeMode { Local, Global };

//		public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, NormalizeMode normalizeMode)
//		{
//			float[,] noiseMap = new float[mapWidth, mapHeight];

//			System.Random prng = new System.Random(seed);
//			Vector2[] octaveOffsets = new Vector2[octaves];

//			float maxPossibleHeight = 0;
//			float amplitude = 1;
//			float frequency = 1;

//			for (int i = 0; i < octaves; i++)
//			{
//				float offsetX = prng.Next(-100000, 100000) + offset.x;
//				float offsetY = prng.Next(-100000, 100000) - offset.y;
//				octaveOffsets[i] = new Vector2(offsetX, offsetY);

//				maxPossibleHeight += amplitude;
//				amplitude *= persistance;
//			}

//			if (scale <= 0)
//			{
//				scale = 0.0001f;
//			}

//			float maxLocalNoiseHeight = float.MinValue;
//			float minLocalNoiseHeight = float.MaxValue;

//			float halfWidth = mapWidth / 2f;
//			float halfHeight = mapHeight / 2f;


//			for (int y = 0; y < mapHeight; y++)
//			{
//				for (int x = 0; x < mapWidth; x++)
//				{

//					amplitude = 1;
//					frequency = 1;
//					float noiseHeight = 0;

//					for (int i = 0; i < octaves; i++)
//					{
//						float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
//						float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

//						float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
//						noiseHeight += perlinValue * amplitude;

//						amplitude *= persistance;
//						frequency *= lacunarity;
//					}

//					if (noiseHeight > maxLocalNoiseHeight)
//					{
//						maxLocalNoiseHeight = noiseHeight;
//					}
//					else if (noiseHeight < minLocalNoiseHeight)
//					{
//						minLocalNoiseHeight = noiseHeight;
//					}
//					noiseMap[x, y] = noiseHeight;
//				}
//			}

//			for (int y = 0; y < mapHeight; y++)
//			{
//				for (int x = 0; x < mapWidth; x++)
//				{
//					if (normalizeMode == NormalizeMode.Local)
//					{
//						noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
//					}
//					else
//					{
//						float normalizedHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight / 0.9f);
//						noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
//					}
//				}
//			}

//			return noiseMap;
//		}

//		public static float[,] GenerateFalloffMap(int size)
//		{
//			float[,] map = new float[size, size];

//			for (int i = 0; i < size; i++)
//			{
//				for (int j = 0; j < size; j++)
//				{
//					float x = i / (float)size * 2 - 1;
//					float y = j / (float)size * 2 - 1;

//					float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
//					map[i, j] = Evaluate(value);
//				}
//			}

//			return map;
//		}

//		static float Evaluate(float value)
//		{
//			float a = 3;
//			float b = 2.2f;

//			return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
//		}


//		public static Texture2D TextureFromColourMap(Color[] colourMap, int width, int height)
//		{
//			Texture2D texture = new Texture2D(width, height);
//			texture.filterMode = FilterMode.Point;
//			texture.wrapMode = TextureWrapMode.Clamp;
//			texture.SetPixels(colourMap);
//			texture.Apply();
//			return texture;
//		}


//		public static Texture2D TextureFromHeightMap(float[,] heightMap)
//		{
//			int width = heightMap.GetLength(0);
//			int height = heightMap.GetLength(1);

//			Color[] colourMap = new Color[width * height];
//			for (int y = 0; y < height; y++)
//			{
//				for (int x = 0; x < width; x++)
//				{
//					colourMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
//				}
//			}

//			return TextureFromColourMap(colourMap, width, height);
//		}

//		public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
//		{
//			AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

//			int width = heightMap.GetLength(0);
//			int height = heightMap.GetLength(1);
//			float topLeftX = (width - 1) / -2f;
//			float topLeftZ = (height - 1) / 2f;

//			int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
//			int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

//			MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
//			int vertexIndex = 0;

//			for (int y = 0; y < height; y += meshSimplificationIncrement)
//			{
//				for (int x = 0; x < width; x += meshSimplificationIncrement)
//				{
//					meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
//					meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

//					if (x < width - 1 && y < height - 1)
//					{
//						meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
//						meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
//					}

//					vertexIndex++;
//				}
//			}

//			return meshData;

//		}
//		public class MeshData
//		{
//			public Vector3[] vertices;
//			public int[] triangles;
//			public Vector2[] uvs;
//			int triangleIndex;

//			public MeshData(int meshWidth, int meshHeight)
//			{
//				vertices = new Vector3[meshWidth * meshHeight];
//				uvs = new Vector2[meshWidth * meshHeight];
//				triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
//			}

//			public void AddTriangle(int a, int b, int c)
//			{
//				triangles[triangleIndex] = a;
//				triangles[triangleIndex + 1] = b;
//				triangles[triangleIndex + 2] = c;
//				triangleIndex += 3;
//			}

//			public Mesh CreateMesh()
//			{
//				Mesh mesh = new Mesh();
//				mesh.vertices = vertices;
//				mesh.triangles = triangles;
//				mesh.uv = uvs;
//				mesh.RecalculateNormals();
//				return mesh;
//			}
//		}

//		[System.Serializable]
//		public struct TerrainType
//		{
//			public string name;
//			public float height;
//			public Color colour;
//			public bool grass;
//			public TerrainType(string name,float height,Color color,bool grass) 
//			{
//				this.name = name;
//				this.height = height;
//				this.colour = color;
//				this.grass = grass;
//			}
//		}

//		public struct MapData
//		{
//			public readonly float[,] heightMap;
//			public readonly Color[] colourMap;


//			public MapData(float[,] heightMap, Color[] colourMap)
//			{
//				this.heightMap = heightMap;
//				this.colourMap = colourMap;
//			}
//		}
//	}
//}
