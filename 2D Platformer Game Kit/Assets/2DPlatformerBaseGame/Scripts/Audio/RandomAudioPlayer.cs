using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioPlayer : MonoBehaviour {

	[System.Serializable]
    public struct TileOverride
    {
        public TileBase tile;
        public AudioClip[] clips;
    }

    public AudioClip[] clips;

    public TileOverride[] overrides;

    public bool randomizePitch = false;
    public float pitchRange = 0.2f;

    protected AudioSource audioSource;
    protected Dictionary<TileBase, AudioClip[]> lookupOverride;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        lookupOverride = new Dictionary<TileBase, AudioClip[]>();

        for (int i = 0; i < overrides.Length; i++)
        {
            if (overrides[i].tile == null)
            {
                continue;
            }

            lookupOverride[overrides[i].tile] = overrides[i].clips;
        }
    }

    public void PlayRandomSound(TileBase surface = null)
    {
        AudioClip[] source = clips;

        AudioClip[] temp;
        if (surface != null && lookupOverride.TryGetValue(surface, out temp))
        {
            source = temp;
        }

        int choice = Random.Range(0, source.Length);

        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(1.0f - pitchRange, 1.0f + pitchRange);
        }

        audioSource.PlayOneShot(source[choice]);
    }

    public void Stop()
    {
        audioSource.Stop();
    }
}
