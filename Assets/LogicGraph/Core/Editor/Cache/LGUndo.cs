﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;

namespace Logic.Editor
{
    /// <summary>
    /// 逻辑图撤销
    /// </summary>
    public sealed class LGUndo
    {
        private const int MAX_STEP = 10;
        private Stack<LGUndoData> UndoStack = new Stack<LGUndoData>();
        private BaseGraphView _graphView;
        public BaseGraphView GraphView => _graphView;
        public LGUndo(BaseGraphView graph)
        {
            _graphView = graph;
        }


        public void PopUndo()
        {
            if (UndoStack.Count <= 0)
            {
                _graphView.Window.ShowNotification(new UnityEngine.GUIContent("最多撤销十步"));
                return;
            }
            LGUndoData undoData = UndoStack.Pop();
            undoData.Undo();
        }

        public void PushUndo(LGUndoData undoData)
        {
            if (UndoStack.Count >= MAX_STEP)
            {
                LGUndoData[] temps = UndoStack.ToArray();
                UndoStack.Clear();
                for (int i = 1; i < temps.Length; i++)
                {
                    UndoStack.Push(temps[i]);
                }
            }
            UndoStack.Push(undoData);
        }


    }

    /// <summary>
    /// 逻辑图撤销数据
    /// </summary>
    public sealed class LGUndoData
    {
        private List<BaseLogicNode> _undoNodeList = new List<BaseLogicNode>();
        private List<UndoGroupData> _undoGroupList = new List<UndoGroupData>();
        private List<BaseVariable> _undoVarList = new List<BaseVariable>();
        private List<NodeEdgeData> _undoEdgeList = new List<NodeEdgeData>();

        private BaseGraphView _graph;
        public LGUndoData(BaseGraphView graph)
        {
            _graph = graph;
        }

        public void AddStep(NodePort input, NodePort output)
        {
            NodeEdgeData nodeEdgeData = new NodeEdgeData();
            nodeEdgeData.Init(input, output);
          
            _undoEdgeList.Add(nodeEdgeData);
        }

        public void AddStep(BaseLogicNode target)
        {
            _undoNodeList.Add(target);
        }

        public void AddStep(BaseLogicGroup group)
        {
            _undoGroupList.Add(new UndoGroupData(group));
        }

        public void AddStep(BaseVariable param)
        {
            _undoVarList.Add(param);
        }

        public void Undo()
        {
            foreach (var item in _undoVarList)
            {
                _graph.AddVariable(item);
            }
            foreach (var item in _undoNodeList)
            {
                _graph.AddNode(item);
                _graph.AddNodeView(item);
            }
            foreach (var item in _undoGroupList)
            {
                foreach (var nodeId in item.nodes)
                {
                    item.group.Nodes.Add(nodeId);
                }
                _graph.Target.Groups.Add(item.group);
                _graph.AddGroupView(item.group);
            }
            foreach (var item in _undoEdgeList)
            {
                item.DrawLink(this._graph);
            }
        }


        private class UndoGroupData
        {
            public BaseLogicGroup group = null;
            public List<string> nodes = new List<string>();
            public UndoGroupData(BaseLogicGroup group)
            {
                this.group = group;
                foreach (var item in group.Nodes)
                {
                    nodes.Add(item);
                }
            }
        }

    }
}
