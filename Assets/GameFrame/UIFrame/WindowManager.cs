﻿using System;
using System.Collections.Generic;
using GameFrame;
using GameFrame.UGUI;
using GameFrameDebuger;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace UIFrameWork
{
    public class WindowManager : Singleton<WindowManager>
    {
        private List<WindowBase> m_windows;
        private List<WindowBase> m_pooledWindows;
        private int m_windowSequence;
        private Dictionary<enWindowPriority, List<int>> m_exitWindowSequences;
        private GameObject m_root; 
        public OnWindowSorted OnWindowSorted;
        private EventSystem m_eventSystem;
        private Camera m_UICamera;
        private Queue<MessageBoxContent> messageBoxContexts = new Queue<MessageBoxContent>();
        private Queue<HintContent> hintContexts = new Queue<HintContent>();
        
        public Camera UICamera
        {
            get { return m_UICamera; }
        }

        public override void Init()
        {
            base.Init();
            m_windows = new List<WindowBase>();
            m_pooledWindows = new List<WindowBase>();
            m_windowSequence = 0;
            m_exitWindowSequences = new Dictionary<enWindowPriority, List<int>>();
            CreateUIRoot();
            CreateEventSystem();
            CreateCamera();
        }
      
        private void CreateUIRoot()
        {
            this.m_root = new GameObject("UIRoot");
            GameObject obj = GameObject.Find("BootUp");
            if (obj != null)
            {
                this.m_root.transform.SetParent(obj.transform);
                this.m_root.transform.localPosition = Vector3.zero;
            }
        }

        private void CreateEventSystem()
        {
            if (this.m_eventSystem == null)
            {
                GameObject obj = new GameObject("EventSystem");
                this.m_eventSystem = obj.AddComponent<EventSystem>();
                obj.AddComponent<StandaloneInputModule>();
            }
            this.m_eventSystem.gameObject.transform.SetParent(m_root.transform);
        }

        private void CreateCamera()
        {
            GameObject obj = new GameObject("UICamera");
            obj.transform.SetParent(this.m_root.transform,true);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localScale = Vector3.one;
            obj.transform.localRotation = Quaternion.identity;
            Camera camera = obj.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 50;
            camera.clearFlags = CameraClearFlags.Depth;
            camera.depth = 10;
            this.m_UICamera = camera;
        }

        public void UpdateSortingOrder()
        {
            this.m_windows.Sort();
            foreach (WindowBase t in this.m_windows)
            {
                int openorder = this.GetWindowOpenOrder(t.WindowInfo.Priority,t.GetSequence());
                t.SetDisplayOrder(openorder);
            }
            if (this.OnWindowSorted != null)
            {
                this.OnWindowSorted(this.m_windows);
            }
        }
        public void OnUpdate()
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                this.m_windows[i].CustomUpdate();
            }
        }
        public string GetWindowName(string path)
        {
            string[] arr = path.Split('/');
            return arr[arr.Length - 1];
        }
        public void OnLateUpdate()
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                this.m_windows[i].CustomLateUpdate();
            }
        }

        public void CloseWindow(bool isforce,WindowBase windowBase,WindowContext windowContext = null)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i] == windowBase)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce, windowContext);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce, windowContext);
                    }
                    break;
                }
            }
        }
        
        List<WindowBase> removeList = new List<WindowBase>();
        
        public void CloseWindow(bool isforce,string name, WindowContext windowContext = null)
        {
            removeList.Clear();
            bool isusepool = false;
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.Name == name)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        isusepool = true;
                    }
                    else
                    { 
                        isusepool = false;
                    }
                    removeList.Add(this.m_windows[i]);
                }
            }
            foreach (WindowBase windowBase in removeList)
            {
                if (isusepool)
                {
                    windowBase.Hide(isforce, windowContext);
                }
                else
                {
                    windowBase.Close(isforce, windowContext);
                }
            }
        }
        
        public void CloseWindow(bool isforce,int sque, WindowContext windowContext = null)
        {
            
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].GetSequence() == sque)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce, windowContext);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce, windowContext);
                    }
                    break;
                }
            }
        }

        public void CloseAllWindow(bool clearPool = true,bool isforce = false)
        {
            int k = 0;
            while (k<this.m_windows.Count)
            {
                if (this.m_windows[k].m_isUsePool)
                {
                    this.m_windows[k].Hide(isforce, null);
                }
                else
                {
                    this.m_windows[k].Close(isforce, null);
                }
            }
            if (clearPool)
            {
                ClearPool();
            }
        }

        public bool HasWindow()
        {
            return this.m_windows.Count > 0;
        }

        public WindowBase GetWindow(string name)
        {
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.Name == name)
                {
                    return this.m_windows[i];
                }
            }
            return null;
        }

        public WindowBase GetWindow(int sque)
        {
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (m_windows[i].GetSequence() == sque)
                {
                    return m_windows[i];
                }
            }
            return null;
        }

        public void CloseGroupWindow(int group,bool isforce)
        {
            if(group == 0) return;
            for (int i = 0; i < m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.Group == group)
                {
                    if (this.m_windows[i].m_isUsePool)
                    {
                        this.m_windows[i].Hide(isforce,null);
                    }
                    else
                    {
                        this.m_windows[i].Close(isforce,null);
                    }
                }
            }
        }

        public WindowBase GetTopWindow()
        {
            WindowBase windowBase = null;
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (!(this.m_windows[i] == null))
                {
                    if (windowBase == null)
                    {
                        windowBase = this.m_windows[i];
                    }
                    else if (this.m_windows[i].GetSortingOrder() > windowBase.GetSortingOrder())
                    {
                        windowBase = this.m_windows[i];
                    }
                }
            }
            return windowBase;
        }

        public void DisableInput()
        {
            if (this.m_eventSystem != null)
            {
                this.m_eventSystem.gameObject.SetActive(false);
            }
        }

        private WindowBase GetUnClosedWindow(string name)
        {
            
            for (int i = 0; i < this.m_windows.Count; i++)
            {
                if (this.m_windows[i].WindowInfo.Name.Equals(name))
                {
                    return this.m_windows[i];
                }
            }
            return null;
        }
      
        public void EnableInput()
        {
            if (this.m_eventSystem != null)
            {
                this.m_eventSystem.gameObject.SetActive(true);
            }
        }
        public void ClearPool()
        {
            for (int i = 0; i < m_pooledWindows.Count; i++)
            {
                Object.DestroyImmediate(m_pooledWindows[i].gameObject);
            }
            this.m_pooledWindows.Clear();
        }

        public void AddToExitSquenceList(enWindowPriority priority,int squence)
        {
            if (!m_exitWindowSequences.ContainsKey(priority))
            {
                List<int> list = new List<int>();
                m_exitWindowSequences[priority] = list;
            }
            m_exitWindowSequences[priority].Add(squence);
        }

        public void RemoveFromExitSquenceList(enWindowPriority priority,int squence)
        {
            if (m_exitWindowSequences.ContainsKey(priority))
            {
                m_exitWindowSequences[priority].Remove(squence);
            }
        }

        public int GetWindowOpenOrder(enWindowPriority priority, int squence)
        {
            int num = this.m_exitWindowSequences[priority].IndexOf(squence);
            if (num >= 0)
            {
                return (num + 1);
            }
            else
            {
                Debug.LogError("error 不应该出现不存在的序列号 请检查调试问题");
                return 0;
            }
        }

        public void RecycleWindow(enWindowPriority priority,WindowBase windowBase,bool isclose = false)
        {
            this.RemoveFromExitSquenceList(priority,windowBase.GetSequence());
            if (windowBase.m_isUsePool && !isclose)
            {
                this.m_pooledWindows.Add(windowBase);
            }
            if (m_windows.Contains(windowBase))
            {
                this.m_windows.Remove(windowBase);
            }
            if (isclose)
            {
                Object.DestroyImmediate(windowBase.CacheGameObject);
            }
        }

        public void OpenWindow(string name,bool isusePool,bool useCameraRender = true,WindowContext appear = null)
        {
            WindowBase windowBase = GetUnClosedWindow(name);
            if (windowBase != null && windowBase.WindowInfo.IsSinglen)
            {
                windowBase.IsPlayHide = false;windowBase.IsPlayOpen = false;//如果正在关闭动画直接打开
                this.RemoveFromExitSquenceList(windowBase.WindowInfo.Priority,windowBase.GetSequence());
                this.AddToExitSquenceList(windowBase.WindowInfo.Priority,this.m_windowSequence);
                int openorder = this.GetWindowOpenOrder(windowBase.WindowInfo.Priority,this.m_windowSequence);
                windowBase.Appear(this.m_windowSequence, openorder, appear);
                if (windowBase.WindowInfo.Group > 0)
                {
                    this.CloseGroupWindow(windowBase.WindowInfo.Group, false);
                }
                this.m_windowSequence++;
                return;
            }
            GameObject obj = null;
            if (isusePool)
            {
                for (int i = 0; i < m_pooledWindows.Count; i++)
                {
                    if (string.Equals(name, this.m_pooledWindows[i].WindowInfo.Name))
                    {
                        obj = this.m_pooledWindows[i].gameObject;
                        this.m_pooledWindows.RemoveAt(i);
                        break;
                    }
                }
            }
            if (obj == null)
            {
                GameObject res = null;
                Action<Object> callBack = delegate(Object loadobj)
                {
                    res = loadobj as GameObject;
                    if (res != null)
                    {
                        obj = Object.Instantiate(res);
                        windowBase = obj.GetComponent<WindowBase>();
                        if (windowBase != null)
                        {
                            windowBase.m_isUsePool = isusePool;
                        }
                        string uiname = GetWindowName(name);
                        obj.name = uiname;
                        if (obj.transform.parent != this.m_root.transform)
                        {
                            obj.transform.SetParent(m_root.transform);
                        }
                        if (windowBase != null)
                        {
                            if (!windowBase.IsInitialized())
                            {
                                AddCollider(windowBase); 
                                windowBase.Init(useCameraRender ? m_UICamera : null);
                            }
                            this.AddToExitSquenceList(windowBase.WindowInfo.Priority,this.m_windowSequence);
                            int openorder = GetWindowOpenOrder(windowBase.WindowInfo.Priority,this.m_windowSequence);
                            windowBase.Appear(this.m_windowSequence, openorder, appear);
                            if (windowBase.WindowInfo.Group > 0)
                            {
                                this.CloseGroupWindow(windowBase.WindowInfo.Group, false);
                            }
                            this.m_windows.Add(windowBase);
                        }
                        this.m_windowSequence++;
                    }
                };
                if (Platform.IsLoadFromBundle)
                {
                    Singleton<ResourceManager>.GetInstance().AddTask("assetbundles/" + name.ToLower() + ".assetbundle",
                        (loadobj) =>
                        {
                            callBack.Invoke(loadobj);
                        }, (int) AssetBundleLoadType.LoadBundleFromFile, (int) CachePriority.NoCache);

                }
                else
                {
                    Singleton<ResourceManager>.GetInstance().LoadResourceAsync<GameObject>(name, (loadobj) =>
                        {
                            callBack.Invoke(loadobj);
                        });
                }
            }
            else
            {
                windowBase = obj.GetComponent<WindowBase>();
                this.AddToExitSquenceList(windowBase.WindowInfo.Priority,this.m_windowSequence);
                int openorder = GetWindowOpenOrder(windowBase.WindowInfo.Priority,this.m_windowSequence);
                windowBase.Appear(this.m_windowSequence, openorder, appear);
                if (windowBase.WindowInfo.Group > 0)
                {
                    this.CloseGroupWindow(windowBase.WindowInfo.Group, false);
                }
                this.m_windows.Add(windowBase);
                this.m_windowSequence++;
            }
        }

        public void AddCollider(WindowBase windowBase)
        {
            Image image = null;
            Empty4Raycast empty4Raycast = null;
            GameObject go = null;
            switch (windowBase.WindowInfo.ColliderMode)
            {
                case enWindowColliderMode.Node:
                    break;
                case enWindowColliderMode.Dark:
                    go = new GameObject("DarkCollider", typeof(RectTransform), typeof(Image));
                    image = go.GetComponent<Image>();
                    image.color = new Color(0, 0, 0, 100 / 255f);
                    image.raycastTarget = true;
                    UIEventListener.Get(image.gameObject).onClick += windowBase.ColliderCallBack;
                    break;
                case enWindowColliderMode.Transparent:
                    go = new GameObject("AnsparencyCollider", typeof(RectTransform), typeof(Empty4Raycast));
                    empty4Raycast = go.GetComponent<Empty4Raycast>();
                    empty4Raycast.raycastTarget = true;
                    UIEventListener.Get(empty4Raycast.gameObject).onClick += windowBase.ColliderCallBack;
                    break;
            }
            if (go != null)
            {
                var rectTran = go.GetComponent<RectTransform>();
                rectTran.transform.SetParent(windowBase.CacheTransform);
                rectTran.transform.SetSiblingIndex(0);
                rectTran.localPosition = Vector3.zero;
                rectTran.anchorMin = new Vector2(0f, 0f);
                rectTran.anchorMax = new Vector2(1f, 1f);
                rectTran.pivot = new Vector2(0.5f, 0.5f);
                rectTran.sizeDelta = GameConfig.Resolution;
            }
        }

        public void ShowMessageBox(MessageBoxContent context)
        {
            var window = GetUnClosedWindow("MessageBox");
            if (window)
            {
                messageBoxContexts.Enqueue(context);
            }
            else
            {
                OpenWindow("MessageBox", true, false, context);
            }
        }

        public MessageBoxContent GetMessageBoxContext()
        {
            if (messageBoxContexts.Count > 0)
            {
                return messageBoxContexts.Dequeue();
            }
            return null;
        }

        public void ShowHintMessage(HintContent content)
        {
            var window = GetUnClosedWindow("Hint");
            if (window)
            {
                hintContexts.Enqueue(content);
            }
            else
            {
                OpenWindow("Hint",true,false,content);
            }
        }

        public HintContent GetHintContext()
        {
            if (hintContexts.Count > 0)
            {
                return hintContexts.Dequeue();
            }
            return null;
        }
        
        public virtual void UnInit()
        {
            m_windows = null;
            m_pooledWindows= null;
            m_windowSequence = 0;
            m_exitWindowSequences= null;
            m_root= null;
            OnWindowSorted= null;
            m_eventSystem= null;
            m_UICamera= null;
            messageBoxContexts = null;
            hintContexts = null;
        }
    }
}
