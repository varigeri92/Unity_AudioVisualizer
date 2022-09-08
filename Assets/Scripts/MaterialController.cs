using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MaterialController : MonoBehaviour
{
    AudioSource _source;
    [SerializeField] Material _material;
    public List<ColorPreset> presets;
    int _current = 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGradient(Color color_from, Color color_to)
    {
        _material.SetColor("_Color_From", color_from);
        _material.SetColor("_Color_To", color_to);
    }

    public void NextColor()
    {
        _current++;
        if (_current >= presets.Count)
        {
            _current = 0;
        }
        SetGradient(presets[_current].color_from, presets[_current].color_to);

    }

    public void PreviousColor()
    {
        _current--;
        if (_current < 0)
        {
            _current = presets.Count-1;
        }
        SetGradient(presets[_current].color_from, presets[_current].color_to);
    }

    public void OffsetNoise(float ammount)
    {
        _material.SetFloat("_NoiseOffset",  ammount);
    }

    public float GetOffset()
    {
        return _material.GetFloat("_NoiseOffset");
    }

    public void SetNoisePower(float ammount)
    {
        _material.SetFloat("_NoisePow", ammount);
    }

}
[System.Serializable]
public class ColorPreset
{
    [ColorUsage(true, true)]
    public Color color_from;
    [ColorUsage(true, true)]
    public Color color_to;
}
