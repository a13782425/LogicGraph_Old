using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public enum UITypeEnum
    {
        参数,
        变量,
        节点
    }
    public sealed class CreateNodeFieldView : VisualElement
    {
        private List<FieldInfo> _fieldInfos = new List<FieldInfo>();

        private Button _delBtn;
        private VisualElement _content;
        private VisualElement _fieldContent;
        private EnumField _uiTypeEnumField;
        private Type _curType;
        public CreateNodeFieldView(Type type)
        {
            _curType = type;
            _content = new VisualElement();
            _content.name = "content";
            this.Add(_content);
            _delBtn = new Button(m_onDelClick);
            _delBtn.name = "del_btn";
            _delBtn.text = "移除";
            this.Add(_delBtn);

            m_addContent();

            this.AddToClassList("node_field");
        }

        private void m_addContent()
        {
            _uiTypeEnumField = new EnumField("UI类型:", UITypeEnum.参数);
            _uiTypeEnumField.RegisterCallback<ChangeEvent<Enum>>(m_onUITypeChange);
            _content.Add(_uiTypeEnumField);
            _fieldContent = new VisualElement();
            _fieldContent.name = "field_content";
            _content.Add(_fieldContent);
            m_setUIType(UITypeEnum.参数);
        }

        private void m_onUITypeChange(ChangeEvent<Enum> evt)
        {
            m_setUIType((UITypeEnum)evt.newValue);
        }

        private void m_setUIType(UITypeEnum newValue)
        {
            switch (newValue)
            {
                case UITypeEnum.参数:
                    m_showParam();
                    break;
                case UITypeEnum.变量:
                    break;
                case UITypeEnum.节点:
                    break;
                default:
                    break;
            }

        }

        /// <summary>
        /// 显示参数
        /// </summary>
        private void m_showParam()
        {
            _fieldContent.Clear();
            _fieldInfos.Clear();
            FieldInfo[] fields = _curType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            Type nodeType = typeof(BaseLogicNode);
            _fieldInfos.AddRange(fields.Where(a => a.FieldType != nodeType && !a.FieldType.IsSubclassOf(nodeType)));
            if (_fieldInfos.Count <= 0)
            {
                _fieldContent.Add(new Label("该节点没有变量"));
            }
            else
                _fieldContent.Add(new PopupField<FieldInfo>("节点选择:", _fieldInfos, 0));
        }

        private void m_onDelClick()
        {
            this.RemoveFromHierarchy();
        }
    }
}
