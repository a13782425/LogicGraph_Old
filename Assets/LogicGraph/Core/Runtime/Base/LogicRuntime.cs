using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Logic
{
    public class LogicRuntime : MonoBehaviour
    {
        /// <summary>
        /// �Ƿ�������
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
        /// ����ִ�е��߼�ͼ
        /// </summary>
        protected List<BaseLogicNode> _executeNodes = new List<BaseLogicNode>();
        /// <summary>
        /// �ȴ�ִ�е��߼�ͼ
        /// </summary>
        protected Queue<BaseLogicNode> _waitExecuteNodes = new Queue<BaseLogicNode>();
        /// <summary>
        /// ִ����ϵȴ�ɾ����node
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
                    Debug.LogError($"�ڵ�ֹͣʧ��,�ڵ���:{item.GetType().Name},������Ϣ:{ex.Message}");
                }
            }
            onComplete(true);
        }

        /// <summary>
        /// ִ��
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
                    Debug.LogError("�߼�ͼ:" + name + "��:" + logicNode.GetType().Name + "�ڵ�ִ��ʧ��");
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
                    Debug.LogError("�߼�ͼ:" + name + "��:" + item.GetType().Name + "�ڵ�ִ��ʧ��");
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
        /// ���
        /// </summary>
        /// <param name="isBreak">�Ƿ��ж����</param>
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
                    Debug.LogError($"�ڵ��˳�ʧ��,�ڵ���:{item.GetType().Name},������Ϣ:{ex.Message}");
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
                    Debug.LogError($"�߼�ͼ�����ص�ִ��ʧ��,������Ϣ:{ex.Message}");
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