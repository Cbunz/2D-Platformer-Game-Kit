using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(Slider))]
public class MixerSliderLink : MonoBehaviour
{
    public AudioMixer mixer;
    public string mixerParameter;

    public float maxAttenuation = 0.0f;
    public float minAttenuation = -80.0f;

    protected Slider slider;

    private void Awake()
    {
        slider = GetComponent<Slider>();

        float value;
        mixer.GetFloat(mixerParameter, out value);

        slider.value = (value - minAttenuation) / (maxAttenuation - minAttenuation);

        slider.onValueChanged.AddListener(SliderValueChange);
    }

    void SliderValueChange(float value)
    {
        mixer.SetFloat(mixerParameter, minAttenuation + value * (maxAttenuation - minAttenuation));
    }
}
