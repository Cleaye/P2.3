using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EditorScript : MonoBehaviour
{
    public HeightMap heightMap;
    public float xOffset = 0;
    public float yOffset = 0;
    public float zOffset = 0;
    public float maskStrength = 3.5f;
    public TextMeshProUGUI maskValue;

    public float terrainFrequency = 50f;
	public int terrainOctaves = 5;
    public TextMeshProUGUI terrainOctavesValue;
	public float terrainLacunarity = 2.5f;
    public TextMeshProUGUI terrainLacunarityValue;
	public float terrainPersistence = 0.5f;
    public TextMeshProUGUI terrainPersistenceValue;
    public float terrainAmplifier = 2f;  
    public TextMeshProUGUI terrainAmplifierValue;  
    public float terrainStrength = 10f;
    public TextMeshProUGUI terrainStrengthValue;


   
    public float sandFrequency = 1f;
	public int sandOctaves = 8;
    public TextMeshProUGUI sandOctavesValue;
	public float sandLacunarity = 2f;
    public TextMeshProUGUI sandLacunarityValue;
	public float sandPersistence = 0.5f;
    public TextMeshProUGUI sandPersistenceValue;
    public float sandAmplifier = 0.4f;
    public TextMeshProUGUI sandAmplifierValue;


    public float grassFrequency = 200f;
	public int grassOctaves = 1;
    public TextMeshProUGUI grassOctavesValue;
	public float grassLacunarity = 3.2f;
    public TextMeshProUGUI grassLacunarityValue;
	public float grassPersistence = 0.6f;
    public TextMeshProUGUI grassPersistenceValue;
    public float grassAmplifier = 0.05f;
    public TextMeshProUGUI grassAmplifierValue;


   

    public float mountainFrequency = 50f;
	public int mountainOctaves = 3;
    public TextMeshProUGUI mountainOctavesValue;
	public float mountainLacunarity = 2f;
    public TextMeshProUGUI mountainLacunarityValue;
	public float mountainPersistence = 0.6f;
    public TextMeshProUGUI mountainPersistenceValue;
    public float mountainAmplifier = 0.3f;
    public TextMeshProUGUI mountainAmplifierValue;

    public void GenerateNewTerrain() {
        Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
        List<float> terrainValues = new List<float>(); 
        terrainValues.Add(terrainFrequency);
        terrainValues.Add(terrainOctaves);
        terrainValues.Add(terrainLacunarity);
        terrainValues.Add(terrainPersistence);
        terrainValues.Add(terrainAmplifier);
        terrainValues.Add(terrainStrength);
        List<float> sandValues = new List<float>();
        sandValues.Add(sandFrequency);
        sandValues.Add(sandOctaves);
        sandValues.Add(sandLacunarity);
        sandValues.Add(sandPersistence);
        sandValues.Add(sandAmplifier);
        List<float> grassValues = new List<float>();
        grassValues.Add(grassFrequency);
        grassValues.Add(grassOctaves);
        grassValues.Add(grassLacunarity);
        grassValues.Add(grassPersistence);
        grassValues.Add(grassAmplifier);
        List<float> mountainValues = new List<float>();
        mountainValues.Add(mountainFrequency);
        mountainValues.Add(mountainOctaves);
        mountainValues.Add(mountainLacunarity);
        mountainValues.Add(mountainPersistence);
        mountainValues.Add(mountainAmplifier);

        heightMap.GenerateCustomTerrainValues(offset, maskStrength, terrainValues, sandValues, grassValues, mountainValues);
    }

    public void SetOffsetX(string x) {
        xOffset = float.Parse(x);
    }

    public void SetOffsetY(string y) {
        yOffset = float.Parse(y);
    }

    public void SetOffsetZ(string z) {
        zOffset = float.Parse(z);
    }

    public void SetMaskStrength(float strength) {
        maskStrength = strength;
        maskValue.text = System.Math.Round(strength, 2).ToString();
    }

    public void SetTerrainFrequency(float frequency) {
        terrainFrequency = frequency;
    }

    public void SetTerrainOctaves(float octaves) {
        terrainOctaves = (int)octaves;
        terrainOctavesValue.text = octaves.ToString();
    }

    public void SetTerrainLacunarity(float lacunarity) {
        terrainLacunarity = lacunarity;
        terrainLacunarityValue.text = System.Math.Round(lacunarity, 2).ToString();
    }

    public void SetTerrainPersistence(float persistence) {
        terrainPersistence = persistence;
        terrainPersistenceValue.text = System.Math.Round(persistence, 2).ToString();
    }

    public void SetTerrainAmplifier(float amplifier) {
        terrainAmplifier = amplifier;
        terrainAmplifierValue.text = System.Math.Round(amplifier, 2).ToString();
    }

    public void SetTerrainStrength(float strength) {
        terrainStrength = strength;
        terrainStrengthValue.text = System.Math.Round(strength, 2).ToString();
    }

    public void SetSandFrequency(float frequency) {
        sandFrequency = frequency;
    }

    public void SetSandOctaves(float octaves) {
        sandOctaves = (int)octaves;
        sandOctavesValue.text = octaves.ToString();
    }

    public void SetSandLacunarity(float lacunarity) {
        sandLacunarity = lacunarity;
        sandLacunarityValue.text = System.Math.Round(lacunarity, 2).ToString();
    }

    public void SetSandPersistence(float persistence) {
        sandPersistence = persistence;
        sandPersistenceValue.text = System.Math.Round(persistence, 2).ToString();
    }

    public void SetSandAmplifier(float amplifier) {
        sandAmplifier = amplifier;
        sandAmplifierValue.text = System.Math.Round(amplifier, 2).ToString();
    }

    public void SetGrassFrequency(float frequency) {
        grassFrequency = frequency;
    }

    public void SetGrassOctaves(float octaves) {
        grassOctaves = (int)octaves;
        grassOctavesValue.text = octaves.ToString();
    }

    public void SetGrassLacunarity(float lacunarity) {
        grassLacunarity = lacunarity;
        grassLacunarityValue.text = System.Math.Round(lacunarity, 2).ToString();
    }

    public void SetGrassPersistence(float persistence) {
        grassPersistence = persistence;
        grassPersistenceValue.text = System.Math.Round(persistence, 2).ToString();
    }

    public void SetGrassAmplifier(float amplifier) {
        grassAmplifier = amplifier;
        grassAmplifierValue.text = System.Math.Round(amplifier, 2).ToString();
    }

    public void SetMountainFrequency(float frequency) {
        mountainFrequency = frequency;
    }

    public void SetMountainOctaves(float octaves) {
        mountainOctaves = (int)octaves;
        mountainOctavesValue.text = octaves.ToString();
    }

    public void SetMountainLacunarity(float lacunarity) {
        mountainLacunarity = lacunarity;
        mountainLacunarityValue.text = System.Math.Round(lacunarity, 2).ToString();
    }

    public void SetMountainPersistence(float persistence) {
        mountainPersistence = persistence;
        mountainPersistenceValue.text = System.Math.Round(persistence, 2).ToString();
    }

    public void SetMountainAmplifier(float amplifier) {
        mountainAmplifier = amplifier;
        mountainAmplifierValue.text = System.Math.Round(amplifier, 2).ToString();
    }
}
