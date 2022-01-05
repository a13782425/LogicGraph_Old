using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Logic.Editor
{
    public sealed class GameObjectField : VisualElement
    {

        private ObjectField _objField;
        public ObjectField objField => _objField;

        public bool allowSceneObjects { get => objField.allowSceneObjects; set => objField.allowSceneObjects = true; }
        public string label { get => _objField.label; set => _objField.label = value; }

        public GameObject value { get => (GameObject)_objField.value; set => _objField.value = value; }

        public event Action<GameObject> onValueChange;

        private Image _previewImage;

        private bool _previewObj = false;
        /// <summary>
        ///  «∑Ò‘§¿¿
        /// </summary>
        public bool previewObj
        {
            get => _previewObj;
            set
            {
                _previewObj = value;
                if (value)
                {
                    _previewImage.style.display = DisplayStyle.Flex;
                    m_showPreview(this.value);
                }
                else
                {
                    _previewImage.style.display = DisplayStyle.None;
                }
            }
        }

        public GameObjectField() : this(null) { }

        public GameObjectField(string label)
        {
            _objField = new ObjectField();
            _objField.objectType = typeof(GameObject);
            _objField.label = label;
            _objField.allowSceneObjects = false;
            _objField.RegisterCallback<ChangeEvent<Object>>(m_valueChanged);
            _previewImage = new Image();
            this.Add(_objField);
            this.Add(_previewImage);
        }

        private void m_valueChanged(ChangeEvent<Object> evt)
        {
            m_showPreview((GameObject)evt.newValue);
            onValueChange?.Invoke((GameObject)evt.newValue);
        }

        private void m_showPreview(GameObject obj)
        {
            if (_previewObj && obj != null)
            {
                _previewImage.image = AssetPreview.GetAssetPreview(obj) ?? AssetPreview.GetMiniThumbnail(obj);
            }
            else
            {
                _previewImage.image = null;
            }
        }

    }
}
