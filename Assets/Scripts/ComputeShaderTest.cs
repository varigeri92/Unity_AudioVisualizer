using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.UI;
using Assets.WasapiAudio.Scripts.Unity;

public class ComputeShaderTest : AudioVisualizationEffect
{

    [SerializeField] RawImage _rawImage;

    const int ARRAY_LENGTH = 128;

    [SerializeField] float power;
    public float SampleRate;
    Mesh mesh;
    [SerializeField] ComputeShader colorCalculatorShader;
    
    float[] spectrum;
    float[] previousSpectrum;
    float[] previousSpectrum_2;

    Color[] SampledpixelsCurrentFrame;
    Color[] SampledPixelsPreviousFrame;
    Color[] SmothedPixels;


    [SerializeField] Texture2D sampledSound;

    ComputeBuffer uvBuffer;
    ComputeBuffer uvBuffer_Fliped;
    ComputeBuffer colorBuffer;

    Color[] _vColors;
    RenderTexture renderTexture;
    RenderTexture capturedSoundData_texture;

    // Start is called before the first frame update


    [SerializeField] AnimationCurve _curve;
    [SerializeField] MaterialController _materialController;
    int chanel = 0;

    float noiseOffset = 0;
    float lastValidAVG = 0;
    [SerializeField] float noisePower;



    // Initializing Buffers:
    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        _vColors = new Color[mesh.vertexCount];
        uvBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float)*2);
        uvBuffer_Fliped = new ComputeBuffer(mesh.vertexCount, sizeof(float) * 2);
        colorBuffer = new ComputeBuffer(mesh.vertexCount, sizeof(float)*4);

        SpectrumSize = ARRAY_LENGTH;
        spectrum = new float[ARRAY_LENGTH*4];
        previousSpectrum = new float[spectrum.Length];
        previousSpectrum_2 = new float[spectrum.Length];

        SampledpixelsCurrentFrame = new Color[ARRAY_LENGTH];
        SampledPixelsPreviousFrame = new Color[ARRAY_LENGTH];
        SmothedPixels = new Color[ARRAY_LENGTH];


        sampledSound = new Texture2D(ARRAY_LENGTH, ARRAY_LENGTH);

        _rawImage.texture = sampledSound;
        renderTexture = new RenderTexture(ARRAY_LENGTH, ARRAY_LENGTH, 0);
        renderTexture.enableRandomWrite = true;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.wrapMode = TextureWrapMode.Clamp;
        capturedSoundData_texture = new RenderTexture(ARRAY_LENGTH, ARRAY_LENGTH, 0);
        capturedSoundData_texture.enableRandomWrite = true;

        SetShaderData();
        colorCalculatorShader.SetFloat("t", 0);


        StartCoroutine(SampleSoundOvertime_DiscMode());
    }



    // Update is called once per frame
    void Update()
    {
        /*
        float[] colorvalues = new float[4 * mesh.vertexCount];
        colorBuffer.GetData(colorvalues);
        int ind = 0;
        for(int i = 3; i < colorvalues.Length; i+=4)
        {
            Color vCol= new Color(colorvalues[i-3], colorvalues[i - 2], colorvalues[i - 1], colorvalues[i]);
            _vColors[ind] = vCol;
            ind++;
        }
        mesh.SetColors(_vColors);


        SampledpixelsCurrentFrame.CopyTo(SampledPixelsPreviousFrame, 0);
        */
    }


    private void SetShaderData()
    {

        colorCalculatorShader.SetBuffer(0, "uvs", uvBuffer);
        uvBuffer.SetData(mesh.uv);
        colorCalculatorShader.SetBuffer(0, "uvs_f", uvBuffer_Fliped);
        uvBuffer.SetData(mesh.uv3);
        colorCalculatorShader.SetBuffer(0, "colors", colorBuffer);
    }

    private void UpdateShaderdata()
    {
        colorCalculatorShader.SetTexture(0, "sampledSound", renderTexture);
        colorCalculatorShader.SetTexture(0, "blendTarget", capturedSoundData_texture);
        colorCalculatorShader.Dispatch(0, ARRAY_LENGTH, ARRAY_LENGTH, 1);
    }

    // Method For OTher Effect
    void SampleSoundToTexture()
    {
        float summ = 0;
        float previousvalue = 0;
        for (int h = 0; h < ARRAY_LENGTH; h++)
        {
            int index = (int)h / 4;

            float val = spectrum[index];
            val *= power;
            summ += val;
            previousvalue = val; 
            float smothVal = (val + previousvalue) * 0.5f;
            Color color = new Color();
            color.r = smothVal;
            color.g = smothVal;
            color.b = smothVal;
            color.a = 1;
            SampledpixelsCurrentFrame[h] = color;
        }

        float avg = summ / ARRAY_LENGTH;

        lastValidAVG = avg * noisePower * power;
        _materialController.SetNoisePower(lastValidAVG);
        noiseOffset += avg * 0.1f;
        _materialController.OffsetNoise(noiseOffset);
        

        sampledSound.SetPixels(SampledpixelsCurrentFrame);
        sampledSound.Apply();
        Graphics.Blit(sampledSound, renderTexture);
    }

    // Method For OTher Effect
    IEnumerator SampleSoundOvertime(float targetOffset = 0)
    {
        while(true)
        {
            float offset_t = 0;
            float offset = _materialController.GetOffset();
            for (int w = 0; w < ARRAY_LENGTH; w++)
            {
                float summ = 0;
                int previosInd = 0;
                for (int h = 0; h < ARRAY_LENGTH; h++)
                {
                    int index = (int)h / 4;
                    float t = (float)h / (float)ARRAY_LENGTH;
                    float val = spectrum[index];
                    val = Mathf.Lerp(spectrum[previosInd], val, 0.5f);
                    previosInd = (int)h / 4;

                    val = (val + previousSpectrum[index] + previousSpectrum_2[index]) / 3;
                    previousSpectrum_2[index] = previousSpectrum[index];
                    previousSpectrum[index] = spectrum[index];

                    val *= power * _curve.Evaluate(t);
                    summ += val;
                    Color color = new Color();
                    color.r = val;
                    color.g = val;
                    color.b = val;
                    color.a = 1;
                    sampledSound.SetPixel(w, h, color);

                }
                float avg = summ / ARRAY_LENGTH;

                lastValidAVG = avg * noisePower * power;
                _materialController.SetNoisePower(lastValidAVG);
                noiseOffset += avg * 0.1f;
                _materialController.OffsetNoise(noiseOffset);

                sampledSound.Apply();
                Graphics.Blit(sampledSound, renderTexture);
                UpdateShaderdata();
                if (targetOffset != 0)
                {
                    offset_t += Time.deltaTime * 0.05f;
                    targetOffset = offset + 0.5f;
                    float newOffset = Mathf.Lerp(offset, targetOffset, offset_t);
                    _materialController.OffsetNoise(newOffset);
                }
                yield return null; //new WaitForSeconds(SampleRate);
            }
        }
    }



    IEnumerator SampleSoundOvertime_DiscMode(float targetOffset = 0)
    {
        yield return null;

        Color[] StartColors = new Color[ARRAY_LENGTH*ARRAY_LENGTH];
        for (int i = 0; i < StartColors.Length; i++)
        {
            StartColors[i] = new Color(0,0,0,1);
        }
        
        sampledSound.SetPixels(StartColors);


        sampledSound.Apply();
        Graphics.Blit(sampledSound, renderTexture);
        UpdateShaderdata();

        while (true)
        {
            float offset_t = 0;
            float offset = _materialController.GetOffset();
            int currentW_Coord = 0;

            for (int w = 0; w < ARRAY_LENGTH; w++)
            {

                //spectrum from unity audio sorces:
                //AudioListener.GetSpectrumData(spectrum, chanel, FFTWindow.Triangle);

                //spectrum from outside audio sorces:
                spectrum = GetSpectrumData();
                float summ = 0;
                int previosInd = 0;

                Color[] oldColors = sampledSound.GetPixels(0, 0, (ARRAY_LENGTH), ARRAY_LENGTH);
                Color[] newColors = new Color[(ARRAY_LENGTH-1) * ARRAY_LENGTH];
                for (int i = 0, k = 0; i < ARRAY_LENGTH-1; i++)
                {
                    for (int j = 0; j < ARRAY_LENGTH; j++, k++)
                    {
                        //Spiral Solution (flickering)
                        //sampledSound.SetPixel(i+1,j, oldColors[k]);

                        //Straigth
                        sampledSound.SetPixel(j+1, i, oldColors[k]);
                    }
                }

                for (int h = 0; h < ARRAY_LENGTH; h++)
                {
                    int index = (int)h / 4;
                    float t = (float)h / (float)ARRAY_LENGTH;
                    float val = spectrum[index];

                    val *= power * _curve.Evaluate(t);
                    summ += val;
                    Color color = new Color();
                    color.r = val;
                    color.g = val;
                    color.b = val;
                    color.a = 1;
                    sampledSound.SetPixel(0, h, color);

                }
                float avg = summ / ARRAY_LENGTH;

                lastValidAVG = avg * noisePower * power;
                _materialController.SetNoisePower(lastValidAVG);
                noiseOffset += avg * 0.1f;
                _materialController.OffsetNoise(noiseOffset);

                sampledSound.Apply();
                Graphics.Blit(sampledSound, renderTexture);
                UpdateShaderdata();
                if (targetOffset != 0)
                {
                    offset_t += Time.deltaTime * 0.05f;
                    targetOffset = offset + 0.5f;
                    float newOffset = Mathf.Lerp(offset, targetOffset, offset_t);
                    _materialController.OffsetNoise(newOffset);
                }

                float[] colorvalues = new float[4 * mesh.vertexCount];
                colorBuffer.GetData(colorvalues);
                int ind = 0;
                for (int i = 3; i < colorvalues.Length; i += 4)
                {
                    Color vCol = new Color(colorvalues[i - 3], colorvalues[i - 2], colorvalues[i - 1], colorvalues[i]);
                    _vColors[ind] = vCol;
                    ind++;
                }
                mesh.SetColors(_vColors);


                SampledpixelsCurrentFrame.CopyTo(SampledPixelsPreviousFrame, 0);

                yield return null;//new WaitForSeconds(SampleRate);
            }
        }
    }



    public void SampleRealtime()
    {
        colorCalculatorShader.SetFloat("t", 0);
    }

    private void OnDestroy()
    {
        uvBuffer.Dispose();
        colorBuffer.Dispose();
        uvBuffer_Fliped.Dispose();
    }

}
