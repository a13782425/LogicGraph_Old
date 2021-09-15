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
    public abstract class BaseParameter
    {

        [SerializeField]
        private string _onlyId = "";
        public string OnlyId => _onlyId;

        [SerializeField]
        public string Name;

        public virtual object Value { get; set; }

        public BaseParameter()
        {
            _onlyId = Guid.NewGuid().ToString();
        }

#if UNITY_EDITOR

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
    [System.Serializable]
    public partial class ColorParameter : BaseParameter
    {
        [SerializeField]
        private Color val = default;
        public override object Value { get => val; set => val = (Color)value; }

#if UNITY_EDITOR
        public override Color GetColor()
        {
            return new Color(1, 0, 1, 1);
        }
        public override VisualElement GetUI()
        {
            ColorField field = new ColorField();
            field.value = val;
            field.RegisterCallback<ChangeEvent<Color>>((a => this.val = a.newValue));
            return field;
        }
#endif
    }

    [System.Serializable]
    public partial class FloatParameter : BaseParameter
    {
        [SerializeField]
        private float val = default;
        public override object Value { get => val; set => val = (float)value; }
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

    [System.Serializable]
    public partial class IntParameter : BaseParameter
    {
        [SerializeField]
        private int val = default;
        public override object Value { get => val; set => val = (int)value; }
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

    [System.Serializable]
    public partial class StringParameter : BaseParameter
    {
        [SerializeField]
        private string val = "";
        public override object Value { get => val; set => val = (string)value; }
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

    [System.Serializable]
    public partial class Vector2Parameter : BaseParameter
    {
        [SerializeField]
        private Vector2 val = default;
        public override object Value { get => val; set => val = (Vector2)value; }
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

    [System.Serializable]
    public partial class Vector3Parameter : BaseParameter
    {
        [SerializeField]
        private Vector3 val = default;
        public override object Value { get => val; set => val = (Vector3)value; }
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
    [System.Serializable]
    public partial class BoolParameter : BaseParameter
    {
        [SerializeField]
        private bool val = default;
        public override object Value { get => val; set => val = (bool)value; }
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
