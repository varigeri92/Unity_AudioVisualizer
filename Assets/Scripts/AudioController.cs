using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    [SerializeField] bool useMicrophone;
    AudioMixerGroup master;
    [SerializeField] AudioSource source;
    [SerializeField] Slider volumeSlider;
    [SerializeField] GameObject debugInfoPanel;


    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.H))
        {
            debugInfoPanel.SetActive(!debugInfoPanel.activeSelf);
        }
        source.volume = volumeSlider.value;
    }


}
