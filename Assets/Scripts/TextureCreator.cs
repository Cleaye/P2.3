using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureCreator : MonoBehaviour
{
    public NoiseMethodType type;

    [Range(1, 3)]
	public int dimensions = 3;

    [Range(2, 512)]
    public int resolution = 256;

    // Controls how busy the noise is
    public float frequency = 1f;
    [Range(1, 8)]
    // Controls how many layers should be added on top of eachother
	public int octaves = 1;
    // Controls the rate of change in frequency. Standard is double the frequency each octave
    [Range(1f, 4f)]
	public float lacunarity = 2f;
    // Controls how much incrementing octaves influence the octaves below it. Standard is half each octave
	[Range(0f, 1f)]
	public float persistence = 0.5f;
    
    private Texture2D texture;

    private void OnEnable() {
        if (texture == null) {
            texture = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
            texture.name = "Procedural Texture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Trilinear;
            texture.anisoLevel = 9;
            GetComponent<MeshRenderer>().material.mainTexture = texture;
        }
        FillTexture();
    }
    
    public void FillTexture() {
        if (texture.width != resolution) {
            texture.Resize(resolution, resolution);
        }

        // World coordinates
        Vector3 point00 = transform.TransformPoint(new Vector3(-0.5f,-0.5f));
		Vector3 point10 = transform.TransformPoint(new Vector3( 0.5f,-0.5f));
		Vector3 point01 = transform.TransformPoint(new Vector3(-0.5f, 0.5f));
		Vector3 point11 = transform.TransformPoint(new Vector3( 0.5f, 0.5f));

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        // Color has to be between 0-1 range
        float stepSize = 1f / resolution;
        // Apply color to every pixel in the texture
        for (int y = 0; y < resolution; y++) {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
			Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++) {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = Noise.Sum(method, point, frequency, octaves, lacunarity, persistence);
				if (type != NoiseMethodType.Value) {
					sample = sample * 0.5f + 0.5f;
				}
				texture.SetPixel(x, y, Color.white * sample);
            }
        }
        texture.Apply();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.hasChanged) {
            transform.hasChanged = false;
            FillTexture();
        }
    }
}
