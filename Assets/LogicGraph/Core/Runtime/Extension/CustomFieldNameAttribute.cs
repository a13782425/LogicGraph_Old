using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Logic
{
    /// <summary>
    /// 自定义Inspector名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class CustomFieldNameAttribute : PropertyAttribute
    {
        public string Name { get; private set; }

        public CustomFieldNameAttribute(string name)
        {
            Name = name;
        }
    }
}
