using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Logic.Editor
{
    public sealed class NodePort : Port, INodeElement
    {
        /// <summary>
        /// 端口朝向
        /// </summary>
        public PortDirEnum PortDir { get; private set; }
        /// <summary>
        /// 端口形状
        /// </summary>
        public PortShapeEnum PortShape { get; private set; }
        /// <summary>
        /// 端口类型
        /// </summary>
        public PortTypeEnum PortType { get; private set; }

        /// <summary>
        /// 是否接受进入端口的连接(被动)
        /// 参数:输出端口的Element
        /// 返回值:是否被连接
        /// </summary>
        public event Func<NodePort, bool> onCanLink;

        /// <summary>
        /// 是否接受输出端口的连接(主动)
        /// 参数:输入端口的Element
        /// 返回值:是否可以连接
        /// </summary>
        public event Func<NodePort, bool> onCanLinkTo;

        /// <summary>
        /// 当有Port端口进入
        /// this=出端口
        /// param=入端口
        /// </summary>
        public event Action<NodePort, NodePort> onAddPort;
        /// <summary>
        /// 当有Port端口断开
        /// this=出端口
        /// param=入端口
        /// </summary>
        public event Action<NodePort, NodePort> onDelPort;


        private NodePortAttribute _portAttr;

        private List<Type> varTypes = new List<Type>();

        public BaseNodeView nodeView { get; private set; }

        public FieldInfo fieldInfo { get; private set; }

        private bool _isList = false;

        /// <summary>
        /// 只有当_isList时才有值
        /// </summary>
        private MethodInfo _listAddFunc = null;
        /// <summary>
        /// 只有当_isList时才有值
        /// </summary>
        private MethodInfo _listRemoveFunc = null;
        private static Type _nodeBaseType = null;
        /// <summary>
        /// 只有当_isList时才有值
        /// </summary>
        private object value = null;

        static NodePort()
        {
            _nodeBaseType = typeof(BaseLogicNode);

        }

        public NodePort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(portOrientation, portDirection, portCapacity, type)
        {
        }

        public void Init(BaseNodeView nodeView, FieldInfo fieldInfo)
        {
            this.nodeView = nodeView;
            if (fieldInfo != null)
            {
                value = fieldInfo.GetValue(nodeView.target);
            }
            if (_isList && value != null)
            {
                _listAddFunc = value.GetType().GetMethod("Add", BindingFlags.Public | BindingFlags.Instance);
                _listRemoveFunc = value.GetType().GetMethod("Remove", BindingFlags.Public | BindingFlags.Instance);
            }
            if (this.nodeView.target is VariableNode varNode)
            {
                if (varNode != null)
                {
                    this.varTypes.Add(varNode.variable.GetValueType());
                }
            }
            switch (this.PortShape)
            {
                case PortShapeEnum.Cube:
                    this.AddToClassList(LogicUtils.PORT_CUBE);
                    this.visualClass = "Port_Cube" + direction;
                    break;
                default:
                    this.visualClass = "Port_" + direction;
                    break;
            }
        }
        public void AddPort(NodePort input)
        {
            if (onAddPort != null)
            {
                onAddPort.Invoke(this, input);
                return;
            }
            if (this.PortType == PortTypeEnum.Default)
            {
                if (input.PortType == PortTypeEnum.Variable)
                {
                    //this是变量节点
                    if (input._isList)
                        input._listAddFunc.Invoke(input.value, new object[] { this.nodeView.target });
                    else
                        input.fieldInfo.SetValue(input.nodeView.target, this.nodeView.target);
                }
                else if (input.PortType == PortTypeEnum.Default)
                {
                    this.nodeView.target.Childs.Add(input.nodeView.target);
                }
                else if (input.PortType == PortTypeEnum.Custom)
                {
                    if (this._isList)
                        this._listAddFunc.Invoke(this.value, new object[] { input.nodeView.target });
                    else
                        this.fieldInfo.SetValue(this.nodeView.target, input.nodeView.target);
                }
            }
            else if (this.PortType == PortTypeEnum.Variable)
            {
                if (input.PortType == PortTypeEnum.Default)
                {
                    //当前端口是变量,目标端口是默认的
                    if (this._isList)
                        this._listAddFunc.Invoke(this.value, new object[] { input.nodeView.target });
                    else
                        this.fieldInfo.SetValue(this.nodeView.target, input.nodeView.target);
                }
            }
            else if (this.PortType == PortTypeEnum.Custom)
            {
                if (this._isList)
                    this._listAddFunc.Invoke(this.value, new object[] { input.nodeView.target });
                else
                    this.fieldInfo.SetValue(this.nodeView.target, input.nodeView.target);
            }
        }
        public void DelPort(NodePort input)
        {
            if (onDelPort != null)
            {
                onDelPort.Invoke(this, input);
                return;
            }
            if (this.PortType == PortTypeEnum.Default)
            {
                if (input.PortType == PortTypeEnum.Variable)
                {
                    //this是变量节点
                    if (input._isList)
                        input._listRemoveFunc.Invoke(input.value, new object[] { this.nodeView.target });
                    else
                        input.fieldInfo.SetValue(input.nodeView.target, null);
                }
                else if (input.PortType == PortTypeEnum.Default)
                {
                    this.nodeView.target.Childs.Remove(input.nodeView.target);
                }
                else if (input.PortType == PortTypeEnum.Custom)
                {
                    if (this._isList)
                        this._listRemoveFunc.Invoke(this.value, new object[] { input.nodeView.target });
                    else
                        this.fieldInfo.SetValue(this.nodeView.target, null);
                }
            }
            else if (this.PortType == PortTypeEnum.Variable)
            {
                if (input.PortType == PortTypeEnum.Default)
                {
                    //当前端口是变量,目标端口是默认的
                    if (this._isList)
                        this._listRemoveFunc.Invoke(this.value, new object[] { input.nodeView.target });
                    else
                        this.fieldInfo.SetValue(this.nodeView.target, null);
                }
            }
            else if (this.PortType == PortTypeEnum.Custom)
            {
                if (this._isList)
                    this._listRemoveFunc.Invoke(this.value, new object[] { input.nodeView.target });
                else
                    this.fieldInfo.SetValue(this.nodeView.target, null);
            }
        }
        /// <summary>
        /// 是否接受进入端口的连接(被动)
        /// 参数:输出端口的Element
        /// 返回值:是否被连接
        /// </summary>
        /// <param name="inPort">等待进入的端口</param>
        public bool CanLink(NodePort inPort)
        {
            bool isResult = true;
            if (!_isList)
                isResult = this.connections.Count() == 0;
            if (!isResult)
                return isResult;
            switch (PortType)
            {
                case PortTypeEnum.Default:
                    isResult = checkDefaultCanLink(inPort);
                    break;
                case PortTypeEnum.Variable:
                    isResult = checkVariableCanLink(inPort);
                    break;
                case PortTypeEnum.Custom:
                default:
                    isResult = checkCustomCanLink(inPort);
                    break;
            }
            if (onCanLink != null)
            {
                isResult = onCanLink(inPort);
            }
            return isResult;
        }

        /// 是否接受输出端口的连接(主动)
        /// 参数:输入端口的Element
        /// 返回值:是否可以连接
        /// </summary>
        /// <param name="toPort">等待被链接的端口</param>
        public bool CanLinkTo(NodePort toPort)
        {
            bool isResult = true;
            if (!_isList)
                isResult = this.connections.Count() == 0;
            if (!isResult)
                return isResult;
            switch (PortType)
            {
                case PortTypeEnum.Default:
                    isResult = checkDefaultCanLinkTo(toPort);
                    break;
                case PortTypeEnum.Variable:
                    isResult = checkVariableCanLinkTo(toPort);
                    break;
                case PortTypeEnum.Custom:
                default:
                    isResult = checkCustomCanLinkTo(toPort);
                    break;
            }
            if (onCanLinkTo != null)
            {
                isResult = onCanLinkTo(toPort);
            }
            return isResult;
        }
        public bool DrawLink()
        {
            if (fieldInfo == null)
            {
                return true;
            }
            if (this.PortDir == PortDirEnum.In)
            {
                if (this.PortType == PortTypeEnum.Variable)
                {
                    if (this._isList)
                    {
                        IEnumerable obj = value as IEnumerable;
                        var varNodes = obj.OfType<BaseLogicNode>().ToList();
                        foreach (var item in varNodes)
                        {
                            var nodeView = this.nodeView.owner.GetNodeView(item);
                            nodeView.OutPut.DrawLink(this);
                        }
                    }
                    else
                    {
                        BaseLogicNode node = value as BaseLogicNode;
                        if (node != null)
                        {
                            var nodeView = this.nodeView.owner.GetNodeView(node);
                            nodeView.OutPut.DrawLink(this);
                        }
                    }
                }
            }
            else
            {
                List<BaseLogicNode> nodes = new List<BaseLogicNode>();
                if (this._isList)
                {
                    IEnumerable obj = value as IEnumerable;
                    nodes.AddRange(obj.OfType<BaseLogicNode>());
                }
                else
                {
                    nodes.Add(value as BaseLogicNode);
                }
                //break;
                //switch (this.PortType)
                //{
                //    case PortTypeEnum.Default:
                //    case PortTypeEnum.Variable:
                       
                //    case PortTypeEnum.Custom:
                //    default:
                //        break;
                //}
                foreach (var item in nodes)
                {
                    if (item != null)
                    {
                        var nodeView = this.nodeView.owner.GetNodeView(item);
                        this.DrawLink(nodeView.Input);
                    }
                }
            }
            return true;
        }
        public bool DrawLink(NodePort inPort)
        {
            EdgeView edge = new EdgeView();
            edge.input = inPort;
            edge.output = this;
            inPort.Connect(edge);
            this.Connect(edge);
            this.nodeView.owner.AddElement(edge);
            this.nodeView.owner.schedule.Execute(() =>
            {
                edge.UpdateEdgeControl();
            }).ExecuteLater(1);
            return true;
        }

        private bool hasType(Type checkType)
        {
            bool result = true;
            if (PortType == PortTypeEnum.Variable)
            {
                if (varTypes.Count > 0)
                {
                    result = varTypes.Contains(checkType);
                }
            }
            return result;
        }
        /// <summary>
        /// 检测自定义端口接受某端口的连接
        /// </summary>
        /// <param name="toPort"></param>
        /// <returns></returns>
        private bool checkCustomCanLink(NodePort inPort)
        {
            if (inPort.PortType == PortTypeEnum.Variable)
                return false;
            var inNode = inPort.nodeView.target as VariableNode;
            if (inPort.PortType == PortTypeEnum.Default && inNode == null)
                return true;
            return false;
        }

        /// <summary>
        /// 检测变量端口接受某端口的连接
        /// </summary>
        /// <param name="inPort">等待进入的端口</param>
        /// <returns></returns>
        private bool checkVariableCanLink(NodePort inPort)
        {
            var inNode = inPort.nodeView.target as VariableNode;
            if (inPort.PortType != PortTypeEnum.Variable && inNode == null)
            {
                return false;
            }
            var curNode = this.nodeView.target as VariableNode;
            if (curNode != null)
            {
                return inPort.hasType(curNode.variable.GetValueType());
            }
            else if (inNode != null)
            {
                return this.hasType(inNode.variable.GetValueType());
            }
            return false;
        }

        /// <summary>
        /// 检测默认端口接受某端口的连接
        /// </summary>
        /// <param name="toPort"></param>
        /// <returns></returns>
        private bool checkDefaultCanLink(NodePort inPort)
        {
            var curNode = this.nodeView.target as VariableNode;
            if (inPort.PortType == PortTypeEnum.Variable && curNode != null)
                return inPort.hasType(curNode.variable.GetValueType());
            if (inPort.PortType == PortTypeEnum.Variable)
                return false;
            return true;
        }

        /// <summary>
        /// 检测自定义端口主动连接到某端口
        /// </summary>
        /// <param name="toPort"></param>
        /// <returns></returns>
        private bool checkCustomCanLinkTo(NodePort toPort)
        {
            if (toPort.PortType == PortTypeEnum.Variable)
                return false;
            var toNode = toPort.nodeView.target as VariableNode;
            if (toPort.PortType == PortTypeEnum.Default && toNode == null)
                return true;
            return false;
        }

        /// <summary>
        /// 检测变量端口主动连接到某端口
        /// </summary>
        /// <param name="toPort"></param>
        /// <returns></returns>
        private bool checkVariableCanLinkTo(NodePort toPort)
        {
            VariableNode toNode = toPort.nodeView.target as VariableNode;
            if (toNode == null)
            {
                //当目标节点不是VariableNode时返回False
                return false;
            }
            return this.hasType(toNode.variable.GetValueType());
        }

        /// <summary>
        /// 检测默认端口主动连接到某端口
        /// </summary>
        /// <param name="toPort"></param>
        /// <returns></returns>
        private bool checkDefaultCanLinkTo(NodePort toPort)
        {
            if (toPort.nodeView.target is VariableNode)
            {
                //目标节点为变量的时候不能被default连接
                return false;
            }
            if (this.nodeView.target is VariableNode curVarNode)
            {
                //当前节点为变量
                //只能连接目标节点为变量的端口
                return toPort.PortType == PortTypeEnum.Variable;
            }
            if (toPort.PortType == PortTypeEnum.Default)
                return true;
            return false;
        }

        public static NodePort CreatePort(string labelName, PortDirEnum dir, PortTypeEnum portType, EdgeConnectorListener edgeConnectorListener)
        {
            var port = new NodePort(Orientation.Horizontal, dir == PortDirEnum.In ? Direction.Input : Direction.Output, Capacity.Multi, null);
            port.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            port.AddManipulator(port.m_EdgeConnector);
            port._isList = true;
            port.portName = labelName;
            port.PortDir = dir;
            port.PortType = PortTypeEnum.Default;
            port.PortShape = PortShapeEnum.Circle;
            port.styleSheets.Add(LogicUtils.GetPortStyle());
            return port;
        }

        public static NodePort CreatePort(FieldInfo fieldInfo, EdgeConnectorListener edgeConnectorListener)
        {
            NodePortAttribute portAttr = fieldInfo.GetCustomAttribute<NodePortAttribute>();
            if (portAttr.Dir == PortDirEnum.All)
            {
                throw new ArgumentException("字段特性里的PortDir不能为All");
            }
            var port = new NodePort(Orientation.Horizontal, portAttr.Dir == PortDirEnum.In ? Direction.Input : Direction.Output, Capacity.Multi, null);
            port.varTypes.AddRange(portAttr.VarTypes);
            port.m_EdgeConnector = new BaseEdgeConnector(edgeConnectorListener);
            port.AddManipulator(port.m_EdgeConnector);
            port.portName = portAttr.Title;
            port.fieldInfo = fieldInfo;
            port.PortDir = portAttr.Dir;
            port.PortShape = portAttr.Shape;
            port.styleSheets.Add(LogicUtils.GetPortStyle());
            checkField(port);
            return port;
        }

        private static void checkField(NodePort port)
        {
            if (port.fieldInfo != null)
            {

                Type fieldType = port.fieldInfo.FieldType;
                Type memberType = fieldType;
                if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    if (port.fieldInfo.FieldType.GetGenericArguments().Length > 0)
                    {
                        memberType = port.fieldInfo.FieldType.GetGenericArguments()[0];
                        port._isList = true;
                    }
                    else
                    {
                        port._isList = false;
                    }
                }
                else
                {
                    port._isList = false;
                }
                if (!_nodeBaseType.IsAssignableFrom(memberType))
                {
                    UnityEngine.Debug.LogError(port.nodeView.target.GetType().Name + ":" + port.fieldInfo.Name);
                    throw new Exception("NodePort 只能应用于BaseLogicNode的变量上");
                }
                if (memberType == typeof(VariableNode))
                {
                    port.PortType = PortTypeEnum.Variable;
                }
                else
                {
                    port.PortType = PortTypeEnum.Custom;
                }
            }
        }
    }
}
