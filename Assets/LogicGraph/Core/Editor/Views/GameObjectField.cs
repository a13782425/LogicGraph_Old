using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class GameObjectField : BaseField<GameObject>
    {
        /// <summary>
        ///   <para>USS class name of elements of this type.</para>
        /// </summary>
        public new static readonly string ussClassName = "unity-object-field";

        /// <summary>
        ///   <para>USS class name of labels in elements of this type.</para>
        /// </summary>
        public new static readonly string labelUssClassName = ussClassName + "__label";

        /// <summary>
        ///   <para>USS class name of input elements in elements of this type.</para>
        /// </summary>
        public new static readonly string inputUssClassName = ussClassName + "__input";

        /// <summary>
        ///   <para>USS class name of object elements in elements of this type.</para>
        /// </summary>
        public static readonly string objectUssClassName = ussClassName + "__object";

        /// <summary>
        ///   <para>USS class name of selector elements in elements of this type.</para>
        /// </summary>
        public static readonly string selectorUssClassName = ussClassName + "__selector";
        public GameObjectField() : this(null)
        {

        }

        public GameObjectField(string label) : base(label, (VisualElement)null)
        {
            //base.visualInput.focusable = false;
            //base.labelElement.focusable = false;
            //AddToClassList(ussClassName);
            //base.labelElement.AddToClassList(labelUssClassName);
            //allowSceneObjects = true;
            //m_ObjectFieldDisplay = new ObjectFieldDisplay(this)
            //{
            //    focusable = true
            //};
            //m_ObjectFieldDisplay.AddToClassList(objectUssClassName);
            //ObjectFieldSelector objectFieldSelector = new ObjectFieldSelector(this);
            //objectFieldSelector.AddToClassList(selectorUssClassName);
            //base.visualInput.AddToClassList(inputUssClassName);
            //base.visualInput.Add(m_ObjectFieldDisplay);
            //base.visualInput.Add(objectFieldSelector);
        }

    }
}
