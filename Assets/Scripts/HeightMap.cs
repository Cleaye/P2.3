using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HeightMap : MonoBehaviour
{
    [Header("Generator")]
    public bool continuousGeneration = false;

    [Header("Perlin Noise Settings")]
    public NoiseMethodType type;
    [Range(1, 3)]
	public int dimensions = 2;

    [Range(2, 512)]
    public int resolution = 250;

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
	public float terrainPersistence = 0.6f;
    // Controls the difference in heights between points
    [Range(0f, 5f)]
    public float terrainAmplifier = 2.3f;

    public Gradient gradient;
    float minTerrainHeight;
    float maxTerrainHeight;

    Mesh terrainMesh;
    Vector3[] terrainVertices;
    Vector2[] uvs;
    int[] terrainTriangles;
        
    // Start is called before the first frame update
    void Start() {
        terrainMesh = new Mesh();
        terrainMesh = GameObject.Find("Terrain").GetComponent<MeshFilter>().mesh;
        GenerateCustomPerlinMap();
        UpdateMesh();
    }

    void Update() {
        if (continuousGeneration) {
            terrainMesh = new Mesh();
            terrainMesh = GameObject.Find("Terrain").GetComponent<MeshFilter>().mesh;
            GenerateCustomPerlinMap();
            UpdateMesh();
        }
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
                y = y * terrainAmplifier;
                
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
            terrainVertices[i].y -= mask[i];
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

        uvs = new Vector2[terrainVertices.Length];
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <=resolution; x++)
            {
                uvs[i] = new Vector2((float)x / resolution, (float)z / resolution);
                i++;
            }
        }
    }
    void UpdateMesh()
    {
        terrainMesh.Clear();
        terrainMesh.vertices = terrainVertices;
        terrainMesh.triangles = terrainTriangles;
        terrainMesh.uv = uvs;
        terrainMesh.RecalculateNormals();
    }
     public float[] GenerateTexture(){
 
        float[] mask = new float[GridSize * GridSize];
        Array.Clear(mask, 0, mask.Length);
        var maskCenter = new Vector2(resolution * 0.5f, resolution * 0.5f);

        for (int y = 0, i = 0; y <  GridSize; y++) {
            for(var x = 0; x < GridSize; x++) {
                var distFromCenter = Vector2.Distance(maskCenter, new Vector2(terrainVertices[i].x, terrainVertices[i].z));
                var maskPixel = (distFromCenter / resolution);
                mask[i] = maskPixel;
                i++;
            }
        }
        return mask;
    }

    /// <summary>
    /// The height at the given (x, z) coordinates on the heightmap.
    /// </summary>
    public float this[int x, int z] {
        get {
            Vector3 p = this.terrainVertices[x + GridSize * z];
            Debug.Assert((int) p.x == x && (int) p.z == z);
            return p.y;
        }
        set {
            Vector3 p = this.terrainVertices[x + GridSize * z];
            Debug.Assert((int) p.x == x && (int) p.z == z);
            this.terrainVertices[x + 1 * z] = new Vector3(p.x, value, p.y);
        }
    }

    /// <summary>
    /// The number of vertices along one side of the heightmap.
    /// </summary>
    public int GridSize => this.resolution + 1;

    public const float SeaLevel = 0.0f;

    /// <summary>
    /// Given the coordinates of a grid point, returns the coordinates of all
    /// neighboring points (including diagonally neighboring points).
    /// </summary>
    public IEnumerable<(int X, int Z)> NeighborsOf(int x, int z)
        => from nx in Enumerable.Range(x - 1, 3)
           from nz in Enumerable.Range(z - 1, 3)
           where nx >= 0 && nx < GridSize && nz >= 0 && nz < GridSize
           select (nx, nz);

    /// <summary>
    /// Returns true if and only if the terrain point at the given coordinates
    /// is at least at sea level.
    /// </summary>
    public bool IsLand(int x, int z)
        => this[x, z] > SeaLevel;

    /// <summary>
    /// Returns true if and only if the terrain point at the given coordinates
    /// is below sea level.
    /// </summary>
    public bool IsSea(int x, int z)
        => !IsLand(x, z);

    /// <summary>
    /// Returns true if and only if the terrain point at the given coordinates
    /// is land and is directly connected to sea.
    /// </summary>
    public bool IsCoast(int x, int z)
        => IsLand(x, z)
        && NeighborsOf(x, z).Any(n => IsSea(n.X, n.Z));

    // Uncomment to draws lines on each vertex that is considered coast.
    // Used for debugging.
    /*
    private void OnDrawGizmos() {
        if (this.terrainVertices is null)
            return;
        foreach (Vector3 vertex in this.terrainVertices) {
            if (IsCoast((int) vertex.x, (int) vertex.z)) {
                Gizmos.DrawLine(vertex, vertex + 3.0f * Vector3.up);
            }
        }   
    }
    */

}