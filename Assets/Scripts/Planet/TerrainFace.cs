using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainFace
{
	ShapeGenerator shapeGenerator;
	Mesh mesh;
	int resolution;//61
	Vector3 localUp;
	Vector3 axisA;
	Vector3 axisB;

	int subfaceIndex = 0;// subfaces
	int subfaceNumber = 1;

	public TerrainFace(ShapeGenerator shapeGenerator, Mesh mesh, int resolution, Vector3 localUp, int subfaceIndex, int subfaceNumber)
	{
		this.shapeGenerator = shapeGenerator;
		this.mesh = mesh;
		this.resolution = resolution;
		this.localUp = localUp;

		this.subfaceIndex = subfaceIndex;//subfaces
		this.subfaceNumber = subfaceNumber;

		axisA = new Vector3(localUp.y, localUp.z, localUp.x);
		axisB = Vector3.Cross(localUp, axisA);
	}
	public void ConstructMesh(int levelOfDetail)
	{
		int simplificationIncriment = levelOfDetail;//levelOfDetail==0?1:levelOfDetail*2;
		int verticesPerLine = (resolution - 1) / simplificationIncriment + 1;
		//LOD
		MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
		int vertexIndex = 0;

		Vector3[] vertices = new Vector3[resolution*resolution];
		int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];
		Vector2[] uv = (mesh.uv.Length == vertices.Length)?mesh.uv:new Vector2[vertices.Length];

		for (int y = 0; y < resolution; y+= simplificationIncriment)
		{
			for (int x = 0; x < resolution; x+= simplificationIncriment)
			{
				//LOD
				//vertexIndex=x + y * resolution;

				Vector2 percent = new Vector2(x, y ) / (resolution - 1);
				Vector3 pointOnUnitCube = localUp + (percent.x - .5f*subfaceNumber + (subfaceIndex % subfaceNumber)) * (2f/subfaceNumber) * axisA + (percent.y -.5f*subfaceNumber  + (subfaceIndex / subfaceNumber)) * (2f / subfaceNumber) * axisB;
				Vector3 pointOnUnitSphere =  pointOnUnitCube.normalized; 
				float unscaledElevation = shapeGenerator.CalculateUnscaledElevation(pointOnUnitSphere) ;

				//LOD
				meshData.vertices[vertexIndex] = pointOnUnitSphere * shapeGenerator.GetScaledElevation(unscaledElevation);
				meshData.uvs[vertexIndex].y = unscaledElevation;
				if(x< resolution - 1&&y< resolution - 1)
				{
					meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
					meshData.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + verticesPerLine + 1);
				}
				vertexIndex++;
			}
		}
		mesh.Clear();
		mesh = meshData.CreateMesh(mesh);
	}

	public void UpdateUVs( ColorGenerator colorGenerator , int levelOfDetail)
	{
		int simplificationIncriment = levelOfDetail;//levelOfDetail==0?1:levelOfDetail*2;
		int verticesPerLine = (resolution - 1) / simplificationIncriment + 1;

		Vector2[] uv = mesh.uv;

		//if (resolution == 241)
		for (int y = 0; y < verticesPerLine; y++)
		{
			for (int x = 0; x < verticesPerLine; x++)
			{
				int i = x + y * verticesPerLine;
				Vector2 percent = new Vector2(x, y) / (verticesPerLine - 1);
				Vector3 pointOnUnitCube = localUp + (percent.x - .5f * subfaceNumber + (subfaceIndex % subfaceNumber)) * (2f / subfaceNumber) * axisA + (percent.y - .5f * subfaceNumber + (subfaceIndex / subfaceNumber)) * (2f / subfaceNumber) * axisB;
				Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;

				uv[i].x = colorGenerator.BiomePercentFromPoint(pointOnUnitSphere);
			}
		}
		mesh.uv = uv;
		
	}
}

public class MeshData
{
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triIndex=0;

	public MeshData(int meshWidth, int meshHeight)
	{
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth*meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1)*6];
	}
	public void AddTriangle(int a, int b, int c)
	{
		triangles[triIndex] = a;
		triangles[triIndex+1] = b;
		triangles[triIndex+2] = c;
		triIndex += 3;
	}

	public Mesh CreateMesh(Mesh mesh)
	{
		//meshFilters[i].sharedMesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}
}