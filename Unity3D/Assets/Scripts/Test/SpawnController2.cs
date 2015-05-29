﻿using UnityEngine;
using System.Collections;

public class SpawnController2 : MonoBehaviour {

    MiceSpawner miceSpawner;
    IEnumerator coroutine;
    public SpawnMode spawnMode = SpawnMode.Random;

    public enum SpawnMode{
        Random,
        EasyMode,
        NormalMode,
        HardMode,
        CarzyMode,
        MissionMode,
        EndTimeMode,
        HelpMode
    }

	// Use this for initialization
	void Start () {
        miceSpawner = GetComponent<MiceSpawner>();
        miceSpawner.StartCoroutine(coroutine);
	}
	
	// Update is called once per frame
	void Update () {

        float time = Time.time;


        if (time > 1f)
        {
            miceSpawner.StopCoroutine(coroutine);
        }
	}
}