using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class LogicRuntime : MonoBehaviour
    {
        /// <summary>
        /// 是否在运行
        /// </summary>
        public bool IsRun { get; protected set; }
        [SerializeField]
        protected BaseLogicGraph _logicGraph;
        public BaseLogicGraph LogicGraph
        {
            get
            {
                return _logicGraph;
            }
            protected set
            {
                _logicGraph = value;
            }
        }
        /// <summary>
        /// 正在执行的逻辑图
        /// </summary>
        protected List<BaseLogicNode> _executeNodes = new List<BaseLogicNode>();
        /// <summary>
        /// 等待执行的逻辑图
        /// </summary>
        protected Queue<BaseLogicNode> _waitExecuteNodes = new Queue<BaseLogicNode>();
        /// <summary>
        /// 执行完毕等待删除的node
        /// </summary>
        protected List<BaseLogicNode> _waitRemoveNodes = new List<BaseLogicNode>();
        protected Action _completeCallBack;

        public static LogicRuntime Create(BaseLogicGraph logicGraph, Transform parent = null)
        {
            GameObject obj = new GameObject("LogicRuntime");
            obj.transform.SetParent(parent);
            obj.transform.position = Vector3.zero;
            LogicRuntime runtime = obj.AddComponent<LogicRuntime>();
            runtime.LogicGraph = logicGraph;
            return runtime;
        }



        public virtual void Begin(Action callback)
        {
            _completeCallBack = callback;
            LogicGraph.Nodes.ForEach(n => n.Initialize(LogicGraph));
            IsRun = true;
            _executeNodes.Clear();
            _waitExecuteNodes.Clear();
            _waitRemoveNodes.Clear();
            LogicGraph.StartNodes.ForEach(n => _waitExecuteNodes.Enqueue(n));

        }

        public virtual void Stop()
        {
            foreach (var item in _executeNodes)
            {
                try
                {
                    item.OnStop();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点停止失败,节点名:{item.GetType().Name},错误信息:{ex.Message}");
                }
            }
            onComplete(true);
        }

        /// <summary>
        /// 执行
        /// </summary>
        protected virtual void onExecute()
        {
            while (_waitExecuteNodes.Count > 0)
            {
                BaseLogicNode logicNode = _waitExecuteNodes.Dequeue();
                try
                {
                    if (logicNode.OnExecute())
                    {
                        if (logicNode.IsComplete)
                        {
                            getNodeChild(logicNode);
                        }
                        else
                        {
                            _executeNodes.Add(logicNode);
                        }
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError("逻辑图:" + name + "中:" + logicNode.GetType().Name + "节点执行失败");
#endif
                    Debug.LogError(ex.Message);
                }
            }
            foreach (var item in _executeNodes)
            {
                try
                {
                    item.OnUpdate(Time.deltaTime);
                    if (item.IsComplete)
                    {
                        getNodeChild(item);
                        _waitRemoveNodes.Add(item);
                    }
                }
                catch (Exception ex)
                {
#if UNITY_EDITOR
                    Debug.LogError("逻辑图:" + name + "中:" + item.GetType().Name + "节点执行失败");
#endif
                    Debug.LogError(ex.Message);
                }
            }
            foreach (var item in _waitRemoveNodes)
            {
                _executeNodes.Remove(item);
            }
            _waitRemoveNodes.Clear();
            checkComplete();
        }

        /// <summary>
        /// 完成
        /// </summary>
        /// <param name="isBreak">是否中断完成</param>
        protected virtual void onComplete(bool isBreak)
        {
            this.IsRun = false;

            foreach (BaseLogicNode item in this.LogicGraph.Nodes)
            {
                try
                {
                    item.OnExit();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"节点退出失败,节点名:{item.GetType().Name},错误信息:{ex.Message}");
                }
            }
            if (!isBreak)
            {
                try
                {
                    _completeCallBack?.Invoke();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"逻辑图结束回调执行失败,错误信息:{ex.Message}");
                }
            }
        }

        private void checkComplete()
        {
            if (_executeNodes.Count == 0 && _waitExecuteNodes.Count == 0)
            {
                onComplete(false);
            }
        }

        private void getNodeChild(BaseLogicNode node)
        {
            if (!node.IsSkip)
            {
                node.GetChild().ForEach(n => _waitExecuteNodes.Enqueue(node));
            }
        }
        private void Awake()
        {
            IsRun = false;
        }
        protected virtual void Update()
        {
            if (!IsRun) return;
            onExecute();
        }



    }
}