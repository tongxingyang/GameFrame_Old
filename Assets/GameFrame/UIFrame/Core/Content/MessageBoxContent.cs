﻿using System;
using UIFrameWork;
using UnityEngine;

public class MessageBoxContent : WindowContext
{
    /// <summary>
    /// 1 只有一个按钮 2 有两个按钮
    /// </summary>
    private int showtype = 2;
    private string titlestring = string.Empty;
    private string contentstring = string.Empty;
    private Action<GameObject, object, object[]> twoButtonAction = null;

    public MessageBoxContent(int type, string title, string content, 
        Action<GameObject, object, object[]> two = null)
    {
        showtype = type;
        titlestring = title;
        contentstring = content;
        twoButtonAction = two;
    }

    public int GetShowType()
    {
        return showtype;
    }

    public string GetTitle()
    {
        return titlestring;
    }

    public string GetContent()
    {
        return contentstring;
    }

    public Action<GameObject, object, object[]> GetTwoAction()
    {
        return twoButtonAction;
    }
}
