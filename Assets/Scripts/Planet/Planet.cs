using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
	[Range(1,6)]
	public int levelOfDetail;
	[Range(0,4)]
	public int subfaceLevel = 0;
	int numberOfTerrainFaces = 6;

	[Range(2, 256)]
	public int resolution = 10;//61
	public bool autoUpdate = true;
	public enum FaceRenderMask {All, Top, Bottom, Left, Right, Front, Back };
	public FaceRenderMask faceRenderMask;

	public ShapeSettings shapeSettings;
	public ColorSettings colorSettings;

	[HideInInspector]
	public bool shapeSettingsFoldout;
	[HideInInspector]
	public bool colorSettingsFoldout;

	ShapeGenerator shapeGenerator = new ShapeGenerator();
	ColorGenerator colorGenerator = new ColorGenerator();

	[SerializeField,HideInInspector]
	MeshFilter[] meshFilters;
	TerrainFace[] terrainFaces;

	private void Start()
	{
		GeneratePlanet();
	}

	void Initialize()
	{
		int subfaceCount = (int)Mathf.Pow(2, subfaceLevel);
		numberOfTerrainFaces = 6 * (subfaceCount*subfaceCount);
		shapeGenerator.UpdateSettings(shapeSettings);
		colorGenerator.UpdateSettings(colorSettings);
		if (meshFilters != null)
		{
			if (meshFilters.Length != numberOfTerrainFaces)
			{
				for (int i = 0; i < meshFilters.Length; i++)
				{
					if (meshFilters[i] != null)
					{
						DestroyImmediate(meshFilters[i].sharedMesh);
						DestroyImmediate(meshFilters[i]);
					}
				}
				meshFilters = new MeshFilter[numberOfTerrainFaces];
			}
		}
		if (meshFilters == null || meshFilters.Length == 0)
		{
			meshFilters = new MeshFilter[numberOfTerrainFaces];
		}
		terrainFaces = new TerrainFace[numberOfTerrainFaces];

		Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back };

		for (int i = 0; i < numberOfTerrainFaces; i++)
		{
			if (meshFilters[i] == null)
			{ 
				GameObject meshObj = new GameObject("mesh");
				meshObj.transform.parent = transform;

				meshObj.AddComponent<MeshRenderer>();
				meshFilters[i] = meshObj.AddComponent<MeshFilter>();
				meshFilters[i].sharedMesh = new Mesh();
			}
			meshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = colorSettings.planetMaterial;

			int subfaceIndex = i % (subfaceCount * subfaceCount); // subfaces

			terrainFaces[i] = new TerrainFace(shapeGenerator, meshFilters[i].sharedMesh, resolution, directions[i/ (subfaceCount * subfaceCount)], subfaceIndex , subfaceCount);
			bool renderFace = faceRenderMask == FaceRenderMask.All || (int)faceRenderMask - 1 == i / (subfaceCount * subfaceCount);
			meshFilters[i].gameObject.SetActive(renderFace);
		}
	}
	public void GeneratePlanet()
	{
		Initialize();
		GenerateMesh();
		GenerateColors();
	}

	public void OnShapeSettingsUpdated()
	{
		if (autoUpdate)
		{
			Initialize();
			GenerateMesh();
		}
	}

	public void OnColorSettingsUpdated()
	{
		if (autoUpdate)
		{
			Initialize();
			GenerateColors();
		}
	}

	void GenerateMesh()
	{
		for(int i = 0;i < numberOfTerrainFaces; i++)
		{
			if (meshFilters[i].gameObject.activeSelf)
			{
				terrainFaces[i].ConstructMesh(levelOfDetail);
			}
		}
		colorGenerator.UpdateElevation(shapeGenerator.elevationMinMax);
	}

	void GenerateColors()
	{
		colorGenerator.UpdateColors();
		for (int i = 0; i < numberOfTerrainFaces; i++)
		{
			if (meshFilters[i].gameObject.activeSelf)
			{
				terrainFaces[i].UpdateUVs(colorGenerator,levelOfDetail);
			}
		}
	}
}
