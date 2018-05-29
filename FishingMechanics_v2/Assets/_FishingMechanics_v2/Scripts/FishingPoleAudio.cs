using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingPoleAudio : MonoBehaviour {
    public AudioSource ActiveSource;
    public AudioSource PassiveSource;

    public AudioClip[] lineBreakClips;

    private Random _rand;

    private void Awake()
    {
        ActiveSource = GetComponents<AudioSource>()[0];
        PassiveSource = GetComponents<AudioSource>()[1];

        _rand = new Random();
    }

    // Use this for initialization
    void Start () {

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void EnableLineStretchNoise()
    {
        if (!PassiveSource.isPlaying)
            PassiveSource.Play();
    }

    public void DisableLineStretchNoise()
    {
        if (PassiveSource.isPlaying)
            PassiveSource.Stop();
    }

    public void ModulateStretchSound(float stretchPercent)
    {
        PassiveSource.volume = stretchPercent;
        PassiveSource.pitch = 1 + (stretchPercent) * 2.5f;
    }

    public void PlaySnapSound()
    {
        int index = Random.Range(0, lineBreakClips.Length);
        AudioClip clip = lineBreakClips[index];
        ActiveSource.PlayOneShot(clip, 1f);
    }
}
