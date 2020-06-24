using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Contains vertex and index data for a mesh.
/// </summary>
public class MeshData {
    public List<Vector3> Positions = new List<Vector3>();
    public List<Vector2> Texcoords = new List<Vector2>();
    public List<int>     Indices   = new List<int>();
}

/// <summary>
/// Represents a rectangular area on a grid, specified by a corner of the area
/// and the size of the area's sides.
/// </summary>
public class GridArea {
    
    public int X;
    public int Z;
    public int SizeX; 
    public int SizeZ;

    public GridArea(int x, int z, int sizeX, int sizeZ)
        => (X, Z, SizeX, SizeZ)
        =  (x, z, sizeX, sizeZ);
    
    /// <summary>
    /// Yields all points in the area.
    /// </summary>
    public IEnumerable<(int X, int Z)> Points
        => from x in Enumerable.Range(X, SizeX)
           from z in Enumerable.Range(Z, SizeZ)
           select (x, z);

}

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

    // The height for every (x, z)-coordinate on the terrain grid.
    private float[,] heightmap;
    // The child objects that hold the terrain meshes.
    private List<GameObject> meshObjects;
        
    // Start is called before the first frame update
    void Start() {
        Generate();
    }

    void Update() {
        if (continuousGeneration)
            Generate();
    }

    private void Generate() {
        // Disable this object's MeshRenderer, as the meshes will be in separate
        // child GameObjects (as a GameObject can have only one mesh)
        GetComponent<MeshRenderer>().enabled = false;
        SeaLevel = GameObject.Find("Ocean").transform.position.y;
        GenerateTerrain();
        GenerateMeshes();
    }

    private void GenerateTerrain() {
        InitializeMap();
        GenerateNoise();
        float[,] mask = GenerateMask();
        ApplyMask(mask);
        CreateMountains();
        for (int i = 0; i < 5; i++)
            UpdateShore();
    }

    /// <summary>
    /// Creates the terrain meshes.
    /// </summary>
    private void GenerateMeshes() {
        // Clear any existing meshes
        meshObjects = meshObjects ?? new List<GameObject>();
        foreach (var meshObject in meshObjects)
            GameObject.Destroy(meshObject);
        meshObjects.Clear();

        foreach (var (mx, mz) in MeshPoints) {
            MeshData data = new MeshData();
            // Find the part of the terrain that should be covered by this mesh
            GridArea area = AreaOf(mx, mz, MaxMeshSize);
            // Generate the mesh vertices
            for (int z = area.Z; z < area.Z + area.SizeZ; z++)
                for (int x = area.X; x < area.X + area.SizeX; x++) {
                    // Set position
                    Vector3 position = new Vector3(
                        x,
                        heightmap[x, z],
                        z
                    );
                    data.Positions.Add(position);
                    // Set texture coords
                    Vector2 texcoords = new Vector2(
                        (float) x / resolution,
                        (float) z / resolution
                    );
                    data.Texcoords.Add(texcoords);
                }
            // Set the mesh indices
            for (int z = 0, v = 0; z < area.SizeZ - 1; z++, v++)
                for (int x = 0; x < area.SizeX - 1; x++, v++) {
                    int o = x + area.SizeX * z;
                    data.Indices.Add(o);
                    data.Indices.Add(o + area.SizeX);
                    data.Indices.Add(o + area.SizeX + 1);
                    data.Indices.Add(o);
                    data.Indices.Add(o + area.SizeX + 1);
                    data.Indices.Add(o + 1);
                }
            // Add a new game object with the mesh
            meshObjects.Add(CreateMesh(data));
        }
    }

    /// <summary>
    /// Spawns a game object with a mesh for part of the terrain.
    /// Multiple game objects are needed because a game object can only have
    /// one mesh.
    /// </summary>
    private GameObject CreateMesh(MeshData data) {
        // Create a new object and make it a child of the terrain.
        GameObject @object = new GameObject("TerrainMesh");
        @object.transform.parent        = this.transform;
        // Reset the child's transform.
        @object.transform.localPosition = Vector3.zero;
        @object.transform.localRotation = Quaternion.identity;
        @object.transform.localScale    = Vector3.one;
        // Set the mesh of the child with the given data.
        MeshFilter   filter   = @object.AddComponent<MeshFilter>();
        MeshRenderer renderer = @object.AddComponent<MeshRenderer>();
        MeshRenderer source   = this.GetComponent<MeshRenderer>();
        renderer.materials = source.materials;
        filter.mesh.Clear();
        filter.mesh.vertices  = data.Positions.ToArray();
        filter.mesh.triangles = data.Indices.ToArray();
        filter.mesh.uv        = data.Texcoords.ToArray();
        filter.mesh.RecalculateNormals();
        return @object;
    }

    /// <summary>
    /// Initializes the heightmap.
    /// </summary>
    private void InitializeMap()
        => heightmap = new float[GridSize, GridSize];

    /// <summary>
    /// Generates heights for the heightmap using noise.
    /// </summary>
    private void GenerateNoise() {
        // Reset min and max terrain height values
        maxTerrainHeight = float.NegativeInfinity;
        minTerrainHeight = float.PositiveInfinity;
        // Get the noise method to be used
        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        // Map points have to be between 0-1 range
        float stepSize = 1f / resolution;
        // World coordinates
        Vector3
            point00 = transform.TransformPoint(new Vector3(-0.5f, -0.5f)),
            point10 = transform.TransformPoint(new Vector3(0.5f, -0.5f)),
            point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f)),
            point11 = transform.TransformPoint(new Vector3(0.5f, 0.5f));
        for (int z = 0; z <= resolution; z++) {
            Vector3
                point0 = Vector3.Lerp(point00, point01, (z + 0.5f) * stepSize),
                point1 = Vector3.Lerp(point10, point11, (z + 0.5f) * stepSize);
            for (int x = 0; x <= resolution; x++) {
                Vector3 point
                    = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float y
                    = terrainAmplifier
                    * Noise.Sum(method, point, terrainFrequency, terrainOctaves,
                        terrainLacunarity, terrainPersistence);
                if (y > maxTerrainHeight)
                    maxTerrainHeight = y;
                if (y < minTerrainHeight)
                    minTerrainHeight = y;
                heightmap[x, z] = y;
            }
        }
    }

    void CreateMountains() {
        for (int z = 0; z <= resolution; z++) {
            for (int x = 0; x <=resolution; x++) {
                if (IsLand(x,z) && !IsCoast(x,z)) {
                    heightmap[x, z] *= 10.0f;
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
                        heightmap[n.X, n.Z] = heightmap[x, z];
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generates a 2D mask to be applied to the terrain.
    /// </summary>
    public float[,] GenerateMask() {
        float[,] mask = new float[GridSize, GridSize];
        for (int j = 0; j < GridSize; j++)
            for (int i = 0; i < GridSize; i++)
                mask[i, j] = 0.0f;
        Vector3 center = new Vector2(0.5f * resolution, 0.5f * resolution);
        for (int z = 0; z < GridSize; z++)
            for (int x = 0; x < GridSize; x++) {
                float
                    distance = Vector2.Distance(center, new Vector2(x, z)),
                    value    = distance / resolution;
                mask[x, z] = value;
            }
        return mask;
    }

    /// <summary>
    /// Applies the given mask to the terrain.
    /// </summary>
    private void ApplyMask(float[,] mask) {
        for (int z = 0; z < GridSize; z++) {
            for (int x = 0; x < GridSize; x++) {
                heightmap[x, z] -= mask[x, z];
            }
        }
    }

    /// <summary>
    /// The height at the given (x, z) coordinates on the heightmap.
    /// </summary>
    public float this[int x, int z] {
        get => heightmap[x, z];
        set => heightmap[x, z] = value;
    }

    /// <summary>
    /// The number of vertices along one side of the heightmap.
    /// </summary>
    public int GridSize => this.resolution + 1;

    /// <summary>
    /// Height level of the sea.
    /// </summary>
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
    /// Returns the area containing specified by a starting point and the size
    /// (number of points along the side) of the area. The returned area may be
    /// not square, in case part of the area runs off of the terrain.
    /// </summary>
    public GridArea AreaOf(int x, int z, int size) {
        int sizeX = size;
        int sizeZ = size;
        if (x + sizeX > GridSize)
            sizeX = GridSize - x;
        if (z + sizeZ > GridSize)
            sizeZ = GridSize - z;
        return new GridArea(x, z, sizeX, sizeZ);
    }

    /// <summary>
    /// Maximum number of points along the size of a mesh. Based on the maximum
    /// number of indices Unity an handle.
    /// </summary>
    private const int MaxMeshSize = 256;

    /// <summary>
    /// Yields the (x, z)-coordinates of all grid points at which a mesh should
    /// be located.
    /// </summary>
    public IEnumerable<(int X, int Z)> MeshPoints {
        get {
            int x = 0, z = 0;
            while (z < GridSize) {
                while (x < GridSize) {
                    yield return (x, z);
                    x += MaxMeshSize - 1; // Overlap of 1 between meshes
                }
                x = 0;
                z += MaxMeshSize - 1; // Overlap of 1 between meshes
            }
        }
    }

    /// <summary>
    /// Returns true if and only if the terrain point at the given coordinates
    /// is at least at sea level.
    /// </summary>
    public bool IsLand(int x, int z)
        => heightmap[x, z] > SeaLevel;

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
        if (this.heightmap is null)
            return;
        for (int z = 0; z < GridSize; z++) {
            for (int x = 0; x < GridSize; x++) {
                if (IsCoast(x, z)) {
                    Vector3 point = new Vector3(x, heightmap[x, z], z);
                    Gizmos.DrawLine(point, point + 3.0f * Vector3.up);
                }
            }
        }
    }
    */

}