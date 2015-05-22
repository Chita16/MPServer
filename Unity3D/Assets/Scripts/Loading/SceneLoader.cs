﻿using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour
{

    AsyncOperation Loader;
    public GameObject ProgressLabel;
    /*
    private float fps = 10.0f;
    private float time;
    //一组动画的贴图，在编辑器中赋值。
    public Texture2D[] animations;
    private int nowFram;
    */

    private void Start()
    {
        Debug.Log(Global.loadScene);
        ProgressLabel.GetComponent<UILabel>().text = "0%";
        StartCoroutine(LoadLevel(Global.loadScene));

    }

    private IEnumerator LoadLevel(string Scene)
    {

        AsyncOperation op = Application.LoadLevelAsync(Scene);
        
        op.allowSceneActivation = false;
        while (op.progress < 0.90f && op.isDone==false)
        {
            Draw((int)op.progress * 100);
            yield return new WaitForEndOfFrame();
        }

        Draw(100);
        yield return new WaitForEndOfFrame();
        if (op.isDone)
        {
            op.allowSceneActivation = true;
        }

        /* 動畫版 Android會出錯
         * 
        int displayProgress = 0;
        int toProgress = 0;
        
        op.allowSceneActivation = false;
        while (op.progress < 0.90f)
        {
            toProgress = (int)op.progress * 100;
            while (displayProgress < toProgress)
            {
                displayProgress+=10;
                Draw(displayProgress);
                yield return new WaitForEndOfFrame();
            }
        }

        toProgress = 100;
        while (displayProgress < toProgress)
        {
            ++displayProgress;
            Draw(displayProgress);
            yield return new WaitForEndOfFrame();
        }
        op.allowSceneActivation = true;
        
         */
    }

    void Draw(int displayProgress)
    {
        //GUI.Label(new Rect(30, 160, 600, 20), "(1) " + progress.ToString() + "%"); // 顯示登入回傳
        ProgressLabel.GetComponent<UILabel>().text = displayProgress.ToString() + "%";
    }

    /*
    void DrawAnimation(Texture2D[] tex)
    {

        time += Time.deltaTime;

        if (time >= 1.0 / fps)
        {

            nowFram++;

            time = 0;

            if (nowFram >= tex.Length)
            {
                nowFram = 0;
            }
        }
        GUI.DrawTexture(new Rect(100, 100, 40, 60), tex[nowFram]);

        //在这里显示读取的进度。
        GUI.Label(new Rect(100, 180, 300, 60), "lOADING!!!!!" + progress);

    }
  */
}