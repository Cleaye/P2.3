using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class HeightMap : MonoBehaviour
{
    Mesh terrainMesh;
    Vector3[] vertices;
    int[] triangles;

    // Heightmap dimensions
    public int HeightMapWidth;
    public int HeightMapHeight; 

    public int MaxHeight;
    
    private float[,] _heightMap;
    
    // Start is called before the first frame update
    void Start()
    {
        _heightMap = new float[HeightMapHeight, HeightMapWidth];
        //CreateRandomHeightMapParallel(1,1f,1f,1);
        terrainMesh = new Mesh();
        GetComponent<MeshFilter>().mesh = terrainMesh;
        CreateShape();
        UpdateMesh();
    }

    // public static float Noise(int x) {
    //     x = (x << 13) ^ x;
    //     return (1.0f - ((x * (x * x * 15731) + 1376312589) & 0x7fffffff) / 1073741824.0f);
    // }

    // public static float CosInterpolate(float v1, float v2, float a) {
    //     var angle = a * Math.PI;
    //     var prc = (1.0f - (float) Math.Cos(angle)) * 0.5f;
    //     return v1 * (1.0f - prc) + v2 * prc;
    // }

    // public static float PerlinNoise2D(int seed, float persistence, int octave, float x, float y) {
    //     var freq = (float) Math.Pow(2.0f, octave);
    //     var amp = (float)Math.Pow(persistence, octave);
    //     var tx = x * freq;
    //     var ty = y * freq;
    //     var txi = (int)tx;
    //     var tyi = (int)ty;
    //     var fracX = tx - txi;
    //     var fracY = ty - tyi;

    //     var v1 = Noise(txi + tyi * 57 + seed);
    //     var v2 = Noise(txi + 1 + tyi * 57 + seed);
    //     var v3 = Noise(txi + (tyi + 1) * 57 + seed);
    //     var v4 = Noise(txi + 1 + (tyi + 1) * 57 + seed);

    //     var i1 = CosInterpolate(v1, v2, fracX);
    //     var i2 = CosInterpolate(v3, v4, fracX);
    //     var f = CosInterpolate(i1, i2, fracY) * amp;
    //     return f;
    // }

    // public void CreateRandomHeightMapParallel(int seed, float noiseSize, float persistence, int octaves) {

    //     for (var y = 0; y < HeightMapHeight; y++) {
    //         for (var x = 0; x < HeightMapWidth; x++) {
    //             var xf = (x / (float) HeightMapWidth) * noiseSize;
    //             var yf = (y / (float) HeightMapHeight) * noiseSize;

    //             var total = 0.0f;
    //             for (var i = 0; i < octaves; i++) {
    //                 var f = PerlinNoise2D(seed, persistence, i, xf, yf);
    //                 total += f;
    //             }
    //             var b = (int)(128 + total * 128.0f);
    //             if (b < 0) b = 0;
    //             if (b > 255) b = 255;

    //             _heightMap[y, x] = (b / 255.0f) * MaxHeight;
    //         }
    //     }
    // }

    void CreateShape()
    {
        vertices = new Vector3[(HeightMapWidth + 1) * (HeightMapHeight + 1)];

        for (int i = 0, z =0; z<= HeightMapHeight; z++)
        {
            for (int x = 0; x<=HeightMapWidth; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }

        triangles = new int[HeightMapWidth * HeightMapHeight * 6];

        int vert = 0;
        int tris = 0;

        // Add triangles in order to draw the mesh later
        for (int z = 0; z < HeightMapHeight; z++)
        {
            for (int x = 0; x < HeightMapWidth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + HeightMapWidth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + HeightMapWidth + 1;
                triangles[tris + 5] = vert + HeightMapWidth + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

    }

    void UpdateMesh()
    {
        terrainMesh.Clear();
        terrainMesh.vertices = vertices;
        terrainMesh.triangles = triangles;
        terrainMesh.RecalculateNormals();
    }

}