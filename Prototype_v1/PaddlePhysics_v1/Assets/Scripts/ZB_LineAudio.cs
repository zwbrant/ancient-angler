using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Catz
/// </summary>
public class ZB_LineAudio : MonoBehaviour {
    internal AudioSource passiveSrc;
    internal AudioSource activeSrc;

    public AudioClip[] lineBreakClips;

    private Random rand;
    // Use this for initialization
    void Start () {
        passiveSrc = GetComponents<AudioSource>()[0];
        activeSrc = GetComponents<AudioSource>()[1];

        rand = new Random();
    }
	
	// Update is called once per frame
	void Update () {
	}

    public void ModulateStretchSound(float stretchLength, float stretchLimit)
    {
        passiveSrc.volume = stretchLength / stretchLimit;
        passiveSrc.pitch = 1 + (stretchLength / stretchLimit) * .6f;
    }

    public void PlaySnapSound()
    {
        int index = Random.Range(0, lineBreakClips.Length);
        AudioClip clip = lineBreakClips[index];
        activeSrc.PlayOneShot(clip, 1f);
    }
}
