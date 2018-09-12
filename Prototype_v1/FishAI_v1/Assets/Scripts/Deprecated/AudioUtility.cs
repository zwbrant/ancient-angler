using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioUtility : MonoBehaviour {
    public static bool lPaddleLoop;
    public static bool rPaddleLoop;
    public List<AudioClip> waterRushes;
    public List<AudioClip> waterSplashes;
    Random rand;

    // Use this for initialization
    void Start () {
        rand = new Random();
	}
	
	// Update is called once per frame
	void Update () {
	    
	}

    IEnumerator WaterRushLoop(AudioSource source)
    {
        float randIndex = Random.Range(0f, waterRushes.Count - 1f);
        randIndex = randIndex - (randIndex % 1);


        yield return new WaitForSeconds(1f);
    }
}
