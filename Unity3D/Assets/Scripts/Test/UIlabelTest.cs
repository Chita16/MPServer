﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class UIlabelTest : MonoBehaviour {

    public GameObject aa;
    AssetBundleManager assetBundleManager = new AssetBundleManager();
	// Use this for initialization
    void Start()
    {
        assetBundleManager = new AssetBundleManager();
        UILabel label = aa.AddComponent<UILabel>();
//        label.bitmapFont = StartCoroutine(assetBundleManager.LoadAtlas("TeamPanel/comic", typeof(Font)));
	}
	
	// Update is called once per frame
	void  Update () {
	
	}
}