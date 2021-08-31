using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public abstract class BaseLogicNodeView
    {

        private LGInfoCache graphCache;

        private NodeView _nodeView;

        protected LNInfoCache nodeCache { get; private set; }


        #region 公共方法

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize(LGInfoCache graphCache, LNInfoCache nodeCache)
        {
            this.graphCache = graphCache;
            this.nodeCache = nodeCache;
            _nodeView = new NodeView(this);
        }

        #endregion




        private class NodeView : Node
        {
            public BaseLogicNodeView LogicNodeView { get; private set; }

            /// <summary>
            /// 重新定义的内容容器
            /// </summary>
            private VisualElement m_content { get; set; }
            public NodeView(BaseLogicNodeView nodeView)
            {
                LogicNodeView = nodeView;
                userData = nodeView;
            }

            public NodeView()
            {
                //this.title = this.GetType().Name;
                //移除右上角折叠按钮
                this.titleContainer.Remove(titleButtonContainer);
                topContainer.style.height = 24;
                m_content = topContainer.parent;
                m_content.style.backgroundColor = new Color(0, 0, 0, 0.5f);
                m_checkTitle();

            }

            #region Title

            private TextField _titleEditor;
            private Label _titleItem;
            private bool _editTitleCancelled = false;
            private void m_checkTitle()
            {
                //找到Title对应的元素
                _titleItem = this.Q<Label>("title-label");
                _titleItem.style.flexGrow = 1;
                _titleItem.style.marginRight = 6;
                _titleItem.style.unityTextAlign = TextAnchor.MiddleCenter;
                _titleItem.RegisterCallback<MouseDownEvent>(m_onMouseDownEvent);
                _titleEditor = new TextField();
                _titleItem.parent.Add(_titleEditor);
                _titleEditor.style.flexGrow = 1;
                _titleEditor.style.marginRight = 6;
                _titleEditor.style.unityTextAlign = TextAnchor.MiddleCenter;
                _titleEditor.style.display = DisplayStyle.None;
                VisualElement visualElement2 = _titleEditor.Q(TextInputBaseField<string>.textInputUssName);
                visualElement2.RegisterCallback<FocusOutEvent>(m_onEditTitleFinished);
                visualElement2.RegisterCallback<KeyDownEvent>(m_onTitleEditorOnKeyDown);
            }
            private void m_onTitleEditorOnKeyDown(KeyDownEvent evt)
            {
                switch (evt.keyCode)
                {
                    case KeyCode.Escape:
                        _editTitleCancelled = true;
                        _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                        break;
                    case KeyCode.Return:
                        _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Blur();
                        break;
                }
            }

            private void m_onEditTitleFinished(FocusOutEvent evt)
            {
                _titleItem.style.display = DisplayStyle.Flex;
                _titleEditor.style.display = DisplayStyle.None;
                if (!_editTitleCancelled)
                {
                    this.title = _titleEditor.text;
                    this.LogicNodeView.nodeCache.Title = this.title;
                }
                _editTitleCancelled = true;
            }

            private void m_onMouseDownEvent(MouseDownEvent evt)
            {
                if (evt.clickCount == 2 && evt.button == 0)
                {
                    _titleItem.style.display = DisplayStyle.None;
                    _titleEditor.style.display = DisplayStyle.Flex;
                    _titleEditor.value = _titleItem.text;
                    _titleEditor.SelectAll();
                    _titleEditor.Q(TextInputBaseField<string>.textInputUssName).Focus();
                    _editTitleCancelled = false;
                }
            }

            #endregion
        }
    }


}