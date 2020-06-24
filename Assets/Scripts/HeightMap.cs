using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HeightMap : MonoBehaviour
{
    [Header("Perlin Noise Settings")]
    public NoiseMethodType type;
    [Range(1, 3)]
	public int dimensions = 3;
    public Vector3 offset;

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
    private GradientColorKey[] colorKeys;
    private GradientAlphaKey[] alphaKeys;
    float minTerrainHeight;
    float maxTerrainHeight;

    Mesh terrainMesh;
    Vector3[] terrainVertices;
    Vector3[] normals;
    Vector2[] uvs;
    Color[] colors;
    int[] terrainTriangles;
        
    // Start is called before the first frame update
    void Start() {
        GenerateTerrain();
    }

    public void GenerateTerrain() {
        SeaLevel = GameObject.Find("Ocean").transform.position.y;
        terrainMesh = new Mesh();
        terrainMesh = GameObject.Find("Terrain").GetComponent<MeshFilter>().sharedMesh;
        GenerateCustomPerlinMap();
        UpdateMesh();
    }

    public void ResetTerrainValues() {
        dimensions = 3;
        offset = new Vector3(0, 0, 0);
        resolution = 250;
        terrainFrequency = 50f;
        terrainOctaves = 5;
        terrainLacunarity = 2f;
        terrainPersistence = 0.6f;
        terrainAmplifier = 2.3f;
        gradient = new Gradient();
        gradient.mode = GradientMode.Fixed;

        // Populate the color keys at the relative time 0 and 1 (0 and 100%)
        colorKeys = new GradientColorKey[4];
        colorKeys[0].color = new Color(1.0f, 0.5901458f, 0.1372549f);
        colorKeys[0].time = 0.497f;
        colorKeys[1].color = new Color(0.042245f, 0.3207547f, 0.004538973f);
        colorKeys[1].time = 0.547f;
        colorKeys[2].color = new Color(0.0f, 0.2358491f, 0.04003245f);
        colorKeys[2].time = 0.766f;
        colorKeys[3].color = new Color(0.4433962f, 0.4433962f, 0.4433962f);
        colorKeys[3].time = 1.0f;

        // Populate the alpha  keys at relative time 0 and 1  (0 and 100%)
        alphaKeys = new GradientAlphaKey[1];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 1.0f;

        gradient.SetKeys(colorKeys, alphaKeys);
        GenerateTerrain();
    }
    
    private void GenerateCustomPerlinMap() {
        terrainVertices = new Vector3[(resolution + 1) * (resolution + 1)];
        uvs = new Vector2[terrainVertices.Length];
        normals = new Vector3[terrainVertices.Length];
        colors = new Color[terrainVertices.Length];

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];

        // Map points have to be between 0-1 range
        float stepSize = 1f / resolution;

        // World coordinates
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f) + offset);
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f) + offset);
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f) + offset);
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f) + offset);
        
        // Calculate height for each point on the grid
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (z + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (z + 0.5f) * stepSize);
            for (int x = 0; x <= resolution; x++)
            {
                uvs[i] = new Vector2((float)x / resolution, (float)z / resolution);
                normals[i] = Vector3.up;

                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float y = Noise.Sum(method, point, terrainFrequency, terrainOctaves, terrainLacunarity, terrainPersistence);
                y *= terrainAmplifier;
                
                if (y > maxTerrainHeight)
                {
                    maxTerrainHeight = y;
                }
                if(y < minTerrainHeight)
                {
                    minTerrainHeight = y;
                }
                this[x,z] = y;
                i++;
            }
        }

        var mask = GenerateTexture();
        for (int i = 0, z = 0; z <= resolution; z++) {
            for (int x = 0; x <= resolution; x++) {   
                this[x,z] -= mask[i];

                float height = Mathf.InverseLerp(minTerrainHeight, maxTerrainHeight, terrainVertices[i].y);
                colors[i] = gradient.Evaluate(height);

                i++;
            }
        }

        CreateMountains();

        for (int i = 0; i < 5; i++) {
            UpdateShore();
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
    }

    void CreateMountains() {
        for (int z = 0; z <= resolution; z++) {
            for (int x = 0; x <=resolution; x++) {
                if (IsLand(x,z) && !IsCoast(x,z)) {
                    this[x,z] *= 10;
                }
            }
        }
    }

    void UpdateShore() {
        for (int z = 0; z <= resolution; z++) {
            for (int x = 0; x <= resolution; x++) {
                if (IsCoast(x,z)) {
                    var neighbors = NeighborsOf(x, z);
                    var seaNeighbors = neighbors.Where(n => IsSea(n.X, n.Z));
                    foreach (var n in seaNeighbors) {
                        this[n.X, n.Z] = this[x,z];
                    }
                }
            }
        }
    }

    void UpdateMesh()
    {
        terrainMesh.Clear();
        terrainMesh.vertices = terrainVertices;
        terrainMesh.triangles = terrainTriangles;
       // terrainMesh.uv = uvs;
        terrainMesh.normals = normals;
        terrainMesh.colors = colors;
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
            //Vector3 p = this.terrainVertices[x + GridSize * z];
            //Debug.Assert((int) p.x == x && (int) p.z == z);
            this.terrainVertices[x + GridSize * z]
                = new Vector3(x, value, z);
        }
    }

    /// <summary>
    /// The number of vertices along one side of the heightmap.
    /// </summary>
    public int GridSize => this.resolution + 1;

    [Header("Sea Settings")]
    public float SeaLevel = 0;

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