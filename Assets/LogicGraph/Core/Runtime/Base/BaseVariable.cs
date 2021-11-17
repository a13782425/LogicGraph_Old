using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngine.UIElements;
using UnityEditor.UIElements;
#endif

namespace Logic
{
    [Serializable]
    public abstract class BaseVariable
    {

        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;

        [SerializeField]
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
#if UNITY_EDITOR
                onModifyParam?.Invoke();
#endif
            }
        }

        /// <summary>
        /// 是否存在默认值
        /// </summary>
        public virtual bool HasDefaultValue => false;
        public virtual object Value { get; set; }
        /// <summary>
        /// 获取值的类型
        /// </summary>
        /// <returns></returns>
        public virtual Type GetValueType() => Value?.GetType();
        public BaseVariable()
        {
            _onlyId = Guid.NewGuid().ToString();
        }

#if UNITY_EDITOR

        /// <summary>
        /// 可以修改变量名
        /// </summary>
        [SerializeField]
        public bool CanRename = true;
        /// <summary>
        /// 可以删除
        /// </summary>
        [SerializeField]
        public bool CanDel = true;
        /// <summary>
        /// 描述
        /// </summary>
        [SerializeField]
        private string _describe = "";
        public string Describe
        {
            get => _describe;
            set
            {
                _describe = value;
                onModifyParam?.Invoke();
            }
        }

        /// <summary>
        /// 节点信息发生修改
        /// </summary>
        public event Action onModifyParam;
        public virtual Color GetColor()
        {
            return new Color32(0, 128, 255, 255);
        }

        public virtual VisualElement GetUI()
        {
            return new Label("BaseParameter");
        }
#endif

    }
    [Serializable]
    public class ColorVariable : BaseVariable
    {
        [SerializeField]
        private Color val = default;
        public override object Value { get => val; set => val = (Color)value; }
        public override Type GetValueType() => typeof(Color);

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color(1, 0, 1, 1);
        }
        public override VisualElement GetUI()
        {
            ColorField field = new ColorField();
            field.labelElement.style.minWidth = 50;
            field.value = val;
            field.RegisterCallback<ChangeEvent<Color>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [Serializable]
    public class FloatVariable : BaseVariable
    {
        [SerializeField]
        private float val = default;
        public override object Value { get => val; set => val = (float)value; }
        public override Type GetValueType() => typeof(float);

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color32(32, 128, 255, 255);
        }
        public override VisualElement GetUI()
        {
            FloatField field = new FloatField();
            field.value = val;
            field.RegisterCallback<ChangeEvent<float>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [Serializable]
    public class IntVariable : BaseVariable
    {
        [SerializeField]
        private int val = default;
        public override object Value { get => val; set => val = (int)value; }
        public override Type GetValueType() => typeof(int);

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color32(50, 93, 255, 255);
        }
        public override VisualElement GetUI()
        {
            IntegerField field = new IntegerField();
            field.value = val;
            field.RegisterCallback<ChangeEvent<int>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [Serializable]
    public class StringVariable : BaseVariable
    {
        [SerializeField]
        private string val = "";
        public override object Value { get => val; set => val = (string)value; }
        public override Type GetValueType() => typeof(string);

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color32(128, 255, 128, 255);
        }
        public override VisualElement GetUI()
        {
            TextField field = new TextField();
            field.value = val;
            field.multiline = true;
            field.RegisterCallback<ChangeEvent<string>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [Serializable]
    public class Vector2Variable : BaseVariable
    {
        [SerializeField]
        private Vector2 val = default;
        public override object Value { get => val; set => val = (Vector2)value; }
        public override Type GetValueType() => typeof(Vector2);

#if UNITY_EDITOR
        public override VisualElement GetUI()
        {
            Vector2Field field = new Vector2Field();
            field.value = val;
            field.RegisterCallback<ChangeEvent<Vector2>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [Serializable]
    public class Vector3Variable : BaseVariable
    {
        [SerializeField]
        private Vector3 val = default;
        public override object Value { get => val; set => val = (Vector3)value; }
        public override Type GetValueType() => typeof(Vector3);

#if UNITY_EDITOR
        public override VisualElement GetUI()
        {
            Vector3Field field = new Vector3Field();
            field.value = val;
            field.RegisterCallback<ChangeEvent<Vector3>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }
    [Serializable]
    public class BoolVariable : BaseVariable
    {
        [SerializeField]
        private bool val = default;
        public override object Value { get => val; set => val = (bool)value; }
        public override Type GetValueType() => typeof(bool);

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color32(48, 255, 255, 255);
        }
        public override VisualElement GetUI()
        {
            Toggle field = new Toggle();
            field.value = val;
            field.RegisterCallback<ChangeEvent<bool>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }
}
