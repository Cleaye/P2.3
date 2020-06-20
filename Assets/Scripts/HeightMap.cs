using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HeightMap : MonoBehaviour
{
    [Header("Perlin Noise Settings")]
    public NoiseMethodType type;
    [Range(1, 3)]
	public int dimensions = 3;

    [Range(2, 512)]
    public int resolution = 256;

    [Header("Terrain Settings")]
    // Controls how busy the noise is
    public float terrainFrequency = 50f;
    [Range(1, 8)]
    // Controls how many layers should be added on top of eachother
	public int terrainOctaves = 5;
    // Controls the rate of change in frequency. Standard is double the frequency each octave
    [Range(1f, 4f)]
	public float terrainLacunarity = 2f;
    // Controls how much incrementing octaves influence the octaves below it. Standard is half each octave
	[Range(0f, 1f)]
	public float terrainPersistence = 0.5f;
    // Controls the difference in heights between points
    [Range(0f, 5f)]
    public float terrainAmplifier = 1.0f;

    public Gradient gradient;
    float minTerrainHeight;
    float maxTerrainHeight;

    Mesh terrainMesh;
    Vector3[] terrainVertices;
    Color[] colors;
    int[] terrainTriangles;
        
    // Start is called before the first frame update
    void Update()
    {
        terrainMesh = new Mesh();
        terrainMesh = GameObject.Find("Terrain").GetComponent<MeshFilter>().mesh;
        GenerateCustomPerlinMap();
        UpdateMesh();
    }

    private void GenerateCustomPerlinMap() {

        terrainVertices = new Vector3[(resolution + 1) * (resolution + 1)];
        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];

        // Map points have to be between 0-1 range
        float stepSize = 1f / resolution;

        // World coordinates
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));
        
        // Calculate height for each point on the grid
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (z + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (z + 0.5f) * stepSize);
            for (int x = 0; x <=resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                // Vector3 point = new Vector3(x * .3f, z * .3f);
                float y = Noise.Sum(method, point, terrainFrequency, terrainOctaves, terrainLacunarity, terrainPersistence);
                if (type != NoiseMethodType.Value) {
					y = y * terrainAmplifier;
				}
                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if(y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }
                terrainVertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        var mask = GenerateTexture();
        //print(mask);
        for (int i = 0; i < terrainVertices.Length; i++) {
            terrainVertices[i].y += mask[i];
        }

        terrainTriangles = new int[resolution * resolution * 6];

        int vert = 0;
        int tris = 0;

        // Add triangles in order to draw the mesh later
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                terrainTriangles[tris + 0] = vert + 0;
                terrainTriangles[tris + 1] = vert + resolution + 1;
                terrainTriangles[tris + 2] = vert + 1;
                terrainTriangles[tris + 3] = vert + 1;
                terrainTriangles[tris + 4] = vert + resolution + 1;
                terrainTriangles[tris + 5] = vert + resolution + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        colors = new Color[terrainVertices.Length];
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <=resolution; x++)
            {
                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, terrainVertices[i].y);
                colors[i] = gradient.Evaluate(height);
                i++;
            }
        }
    }

    void UpdateMesh()
    {
        terrainMesh.Clear();
        terrainMesh.vertices = terrainVertices;
        terrainMesh.triangles = terrainTriangles;
        terrainMesh.colors = colors;
        terrainMesh.RecalculateNormals();
    }

     public float[] GenerateTexture(){
 
        float[] mask = new float[(resolution + 1) * (resolution + 1)];
        var maskCenter = new Vector2(resolution * 0.5f, resolution * 0.5f);
            
        for (int y = 0, i = 0; y < resolution; y++) {
            for(var x = 0; x < resolution; x++){
    
                var distFromCenter  = Vector2.Distance(maskCenter, new Vector2(x, y));
                var maskPixel  = (0.5f - (distFromCenter / resolution)) * 1f;
                mask[i] = maskPixel * 4;
                i++;
            }
        }
        return mask;
    }
}