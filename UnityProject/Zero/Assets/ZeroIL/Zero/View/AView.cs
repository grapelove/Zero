﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Zero;

namespace ZeroIL.Zero
{
    /// <summary>
    /// 视图对象
    /// </summary>
    public abstract class AView
    {
        /// <summary>
        /// 创建AView对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="abName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T Create<T>(string viewName, string abName, AView parent, object data = null) where T:AView
        {
            GameObject prefab = ResMgr.Ins.Load<GameObject>(abName, viewName);
            AView view = Create<T>(prefab, parent, data);
            return view as T;
        }

        /// <summary>
        /// 通过Prefab创建AView对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="abName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static T Create<T>(GameObject prefab, AView parent, object data = null) where T : AView
        {
            AView view = AViewMgr.CreateViewFromPrefab(prefab, parent.gameObject.transform, prefab.name, typeof(T));
            parent.AddChild(view);            
            if(data != null)
            {
                view.SetData(data);
            }
            return view as T;
        }

        /// <summary>
        /// 异步创建AView对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="viewName"></param>
        /// <param name="abName"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static void CreateAsync<T>(string viewName, string abName, AView parent, object data = null, Action<T> onCreated = null, Action<float> onProgress = null) where T : AView
        {
            ResMgr.Ins.LoadAsync(abName, viewName,
            (UnityEngine.Object asset) =>
            {
                GameObject prefab = asset as GameObject;
                T view = Create<T>(prefab, parent, data);
                onCreated(view);
            },
            onProgress);
        }

        /// <summary>
        /// 销毁委托事件
        /// </summary>
        public Action<AView> onDestroyHandler;

        /// <summary>
        /// Unity中的GameObject对象
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// 是否销毁了
        /// </summary>
        public bool IsDestroyed
        {
            get { return gameObject == null ? true : false; }
        }        

        /// <summary>
        /// 对象名称
        /// </summary>
        public string Name
        {
            get { return gameObject.name; }
            set { gameObject.name = value; }
        }

        /// <summary>
        /// 对象对应的视图（Prefab）名称
        /// </summary>
        public string ViewName { get; internal set; }

        /// <summary>
        /// 子视图对象的列表
        /// </summary>
        List<AView> _childViewList = new List<AView>();                    

        internal void SetGameObject(GameObject gameObject)
        {
            this.gameObject = gameObject;

            //AViewGameObject com = GetComplent<AViewGameObject>();
            //if (null == com)
            //{
            //    com = this.gameObject.AddComponent<AViewGameObject>();
            //}
            //Debug.Log(gameObject.name + ":开始注册事件");
            //com.onEnable += OnEnable;
            //com.onDisable += OnDisable;
            //com.onDestroy += OnDestroy;

            OnInit();
            if (this.gameObject.activeInHierarchy)
            {
                OnEnable();
            }
        }

        internal void SetData(object data)
        {
            if(null == data)
            {
                return;
            }
            OnData(data);
        }

        /// <summary>
        /// 设置激活
        /// </summary>
        /// <param name="isActive"></param>
        public void SetActive(bool isActive)
        {
            if(isActive)
            {
                if(false == gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(true);
                    WhenEnable();
                }
            }
            else
            {
                if(gameObject.activeInHierarchy)
                {
                    gameObject.SetActive(false);
                    WhenDisable();
                }
            }
        }

        /// <summary>
        /// 得到组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetComplent<T>()
        {
            return gameObject.GetComponent<T>();
        }

        /// <summary>
        /// 得到子对象上的组件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="childName"></param>
        /// <returns></returns>
        public T GetChildComplent<T>(string childName)
        {
            var child = gameObject.transform.Find(childName);
            if (null == child)
            {
                return default(T);
            }
            return child.GetComponent<T>();
        }

        /// <summary>
        /// 得到子对象
        /// </summary>
        /// <param name="childName"></param>
        /// <returns></returns>
        public Transform GetChild(string childName)
        {
            return gameObject.transform.Find(childName);
        }    
        
        /// <summary>
        /// 得到子对象
        /// </summary>
        /// <param name="childName"></param>
        /// <returns></returns>
        public GameObject GetChildGameObject(string childName)
        {
            var child = GetChild(childName);
            if(null != child)
            {
                return child.gameObject;
            }
            return null;
        }


        #region 获取绑定的数据

        /// <summary>
        /// 获取绑定的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public UnityEngine.Object[] GetBindingObject(string key)
        {
            var com = GetComplent<ObjectBindingData>();
            if(null == com)
            {
                return null;
            }

            var item = com.Find(key);
            if (null == item)
            {
                return null;
            }
            return item.Value.list;
        }

        /// <summary>
        /// 获取绑定的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public double[] GetBindingDouble(string key)
        {
            var com = GetComplent<DoubleBindingData>();
            if (null == com)
            {
                return null;
            }

            var item = com.Find(key);
            if (null == item)
            {
                return null;
            }
            return item.Value.list;
        }

        /// <summary>
        /// 获取绑定的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public float[] GetBindingFloat(string key)
        {
            var com = GetComplent<FloatBindingData>();
            if (null == com)
            {
                return null;
            }
            var item = com.Find(key);

            if (null == item)
            {
                return null;
            }
            return item.Value.list;
        }

        /// <summary>
        /// 获取绑定的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public int[] GetBindingInt(string key)
        {
            var com = GetComplent<IntBindingData>();
            if (null == com)
            {
                return null;
            }
            var item = com.Find(key);
            if (null == item)
            {
                return null;
            }
            return item.Value.list;
        }

        /// <summary>
        /// 获取绑定的数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string[] GetBindingString(string key)
        {
            var com = GetComplent<StringBindingData>();
            if (null == com)
            {
                return null;
            }
            var item = com.Find(key);
            if (null == item)
            {
                return null;
            }
            return item.Value.list;
        }

        #endregion



        /// <summary>
        /// 得到子视图对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="childName"></param>
        /// <returns></returns>
        public T CreateViewChlid<T>(string childName, object data = null) where T:AView
        {          
            var childGameObject = GetChildGameObject(childName);
            if(null == childGameObject)
            {
                return default(T);
            }

            T viewChild = Activator.CreateInstance(typeof(T)) as T;
            viewChild.SetGameObject(childGameObject);
            if(data != null)
            {
                viewChild.SetData(data);
            }

            AddChild(viewChild);

            return viewChild;
        }

        /// <summary>
        /// 添加子视图
        /// </summary>
        /// <param name="child"></param>
        private void AddChild(AView child)
        {
            _childViewList.Add(child);
        }

        /// <summary>
        /// 销毁对象
        /// </summary>
        public void Destroy()
        {
            if(IsDestroyed)
            {
                return;
            }

            WhenDisable();
            AViewMgr.DestroyView(this);
            gameObject = null;
            WhenDestroy();
        }

        internal void WhenEnable()
        {
            if(false == gameObject.activeSelf)
            {
                return;
            }


            OnEnable();
            foreach (var childView in _childViewList)
            {
                childView.WhenEnable();
            }
            
        }

        internal void WhenDisable()
        {
            if (false == gameObject.activeSelf)
            {
                return;
            }

            OnDisable();
            foreach (var childView in _childViewList)
            {                
                childView.WhenDisable();                
            }            
        }

        internal void WhenDestroy()
        {
            OnDestroy();
            if (onDestroyHandler != null)
            {
                onDestroyHandler.Invoke(this);
            }

            foreach (var childView in _childViewList)
            {
                childView.WhenDestroy();
            }
        }

        //internal void SetDestroy()
        //{            
        //    foreach(var childView in _childViewList)
        //    {
        //        childView.SetDestroy();
        //    }
        //    OnDestroy();
        //}


        #region 子类按需求重写实现的方法
        /// <summary>
        /// 初始化方法
        /// </summary>
        protected virtual void OnInit()
        {

        }
        


        /// <summary>
        /// 当显示对象被实例化后该方法被调用。在该方法中可以预备视图所需要的数据。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        protected virtual void OnData(object data)
        {

        }

        /// <summary>
        /// 激活时触发
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// 进入非激活状态时触发
        /// </summary>
        protected virtual void OnDisable()
        {

        }

        /// <summary>
        /// 当显示对象被销毁时调用。在该方法中进行内存回收工作或其它。
        /// </summary>
        protected virtual void OnDestroy()
        {

        }
        #endregion
    }
}
