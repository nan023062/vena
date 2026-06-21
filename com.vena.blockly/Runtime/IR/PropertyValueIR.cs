using System;

namespace Vena.Blockly
{

    /// <summary>
    /// NodeIR.Properties[].value 的统一包装。
    /// 两种形态：
    ///   - 字面值：<see cref="Type"/> = "literal"、<see cref="Value"/> = bool/int/long/float/double/string/null（基本 JSON 标量）。
    ///   - 子节点引用：<see cref="Type"/> = "nodeRef"、<see cref="Value"/> = <see cref="System.Guid"/>（指向同图内 NodeIR.Guid）。
    /// AOT 不变量：字面值类型由 source 槽位静态锁定，不允许 object / 多态运行期分发。
    /// </summary>
    public sealed class PropertyValueIR
    {
        public const string TypeLiteral = "literal";
        public const string TypeNodeRef = "nodeRef";

        /// <summary>形态判别：见类注释。</summary>
        public string Type;

        /// <summary>字面值原始装箱 / Guid（按 Type 区分）。</summary>
        public object Value;

        public PropertyValueIR()
        {
            Type = TypeLiteral;
            Value = null;
        }

        public PropertyValueIR(string type, object value)
        {
            Type = type ?? TypeLiteral;
            Value = value;
        }

        /// <summary>构造字面值形态（基本 JSON 标量 / null）。</summary>
        public static PropertyValueIR Literal(object literal)
            => new PropertyValueIR(TypeLiteral, literal);

        /// <summary>构造子节点引用形态。</summary>
        public static PropertyValueIR NodeRef(Guid nodeGuid)
            => new PropertyValueIR(TypeNodeRef, nodeGuid);

        public bool IsLiteral => Type == TypeLiteral;
        public bool IsNodeRef => Type == TypeNodeRef;
    }
}
