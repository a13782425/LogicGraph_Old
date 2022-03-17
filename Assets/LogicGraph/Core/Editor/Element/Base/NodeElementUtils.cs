using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public static class NodeElementUtils
    {

        public static Dictionary<Type, Type> ElementMapping = new Dictionary<Type, Type>()
        {
            { typeof(string), typeof(NodeTextField) },
            { typeof(int), typeof(NodeIntegerField)},
            { typeof(bool), typeof(NodeToggle)},
            { typeof(double), typeof(NodeDoubleField)},
            { typeof(float), typeof(NodeFloatField)},
            { typeof(Enum), typeof(NodeEnumField)},
            { typeof(Vector2), typeof(NodeVector2Field)},
            { typeof(Vector3), typeof(NodeVector3Field)},
            { typeof(Vector4), typeof(NodeVector4Field)},
            { typeof(Color), typeof(NodeColorField)},
            { typeof(Gradient), typeof(NodeGradientField)},
            { typeof(AnimationCurve),typeof(NodeCurveField)},
            { typeof(Bounds),typeof(NodeBoundsField)},
        };

        /// <summary>
        /// 设置字段组件的默认样式
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="element"></param>
        public static void SetBaseFieldStyle<T>(BaseField<T> element)
        {
            element.style.minHeight = 24;
            element.style.marginTop = 2;
            element.style.marginRight = 2;
            element.style.marginLeft = 2;
            element.style.marginBottom = 2;
            element.style.unityTextAlign = TextAnchor.MiddleLeft;
            element.labelElement.style.minWidth = 50;
            element.labelElement.style.fontSize = 12;
        }

        public static string CheckTitle(this INodeElement node, string title)
        {
            return string.IsNullOrWhiteSpace(title) ? node.fieldInfo.Name : title;
        }

        public static void Show(this INodeElement node)
        {
            if (node is VisualElement visual)
            {
                visual.style.display = DisplayStyle.Flex;
            }
        }
        public static void Hide(this INodeElement node)
        {
            if (node is VisualElement visual)
            {
                visual.style.display = DisplayStyle.None;
            }
        }
    }
}
