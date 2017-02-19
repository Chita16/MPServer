﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class EventMaskSwitch
{
    public static GameObject lastPanel;
    public static GameObject openedPanel { get { return _openedPanel; } }
    private static GameObject _openedPanel;
    private static List<LayerMask> defalutLayerMask = new List<LayerMask>();
    private static List<LayerMask> prevLayerMask = new List<LayerMask>();
    #region -- EventMaskSwtich --
    // 位元移位運算子<< >> 2近位 左移、右移  0 << 1(左移1)  --->  00 = 0
    // 1 << 1(左移1)  -->  001 = 1
    // 1 >> 1(右移1)  -->  010 = 2
    // 1 >> 2(右移2)  -->  0100 = 4
    // 1 << 8(左移8)  -->  000000001
    /// <summary>
    /// 改變UICamera事件觸發遮罩
    /// </summary>
    /// <param name="obj">指定物件圖層</param>
    /// /// <param name="nextPanel">是否開啟下一個階層</param>
    public static void Switch(GameObject obj,bool nextPanel)
    {
        foreach (Camera c in Camera.allCameras)
        {
            if (!nextPanel)
                defalutLayerMask.Add(c.GetComponent<UICamera>().eventReceiverMask);
            prevLayerMask.Add(c.GetComponent<UICamera>().eventReceiverMask);
            c.GetComponent<UICamera>().eventReceiverMask = 1 << obj.layer;
        }
        _openedPanel = obj;
    }
    #endregion

    /// <summary>
    /// 回復上一動事件遮罩
    /// </summary>
    public static void Prev()
    {
        int i = prevLayerMask.Count - Camera.allCamerasCount;
        foreach (Camera c in Camera.allCameras)
        {
            LayerMask mask = c.GetComponent<UICamera>().eventReceiverMask;
            c.GetComponent<UICamera>().eventReceiverMask = prevLayerMask[i];
            prevLayerMask.Remove(prevLayerMask[i]);
        }
    }
    /// <summary>
    /// 回復初始事件遮罩
    /// </summary>
    public static void Resume()
    {
        int i = 0;
        foreach (Camera c in Camera.allCameras)
        {
            LayerMask mask = c.GetComponent<UICamera>().eventReceiverMask;
            c.GetComponent<UICamera>().eventReceiverMask = defalutLayerMask[i];
            i++;
        }
        defalutLayerMask.Clear();
    }
    private static GameObject GetPanel()
    {
        return openedPanel;
    }
}
