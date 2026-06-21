using UnityEngine;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// UGC codegen 白名单 + 输出根目录配置。
    /// 资产路径约定：项目内 `Assets/.../UgcCodegenConfig.asset` 或包内 `Editor/Config/UgcCodegenConfig.asset`。
    /// </summary>
    [CreateAssetMenu(fileName = "UgcCodegenConfig", menuName = "Vena/Blockly/UGC Codegen Config", order = 0)]
    public sealed class UgcCodegenConfig : ScriptableObject
    {
        [Tooltip("被扫描的程序集名清单（asmdef name）。空 = 不扫描。")]
        public string[] AssemblyWhitelist;

        [Tooltip("被扫描的类型全名清单（namespace.Type）。空 = 接受程序集内全部带 [UgcSource] 的类。")]
        public string[] TypeWhitelist;

        [Tooltip("生成产物输出根目录（相对项目根 / Assets 的 Unity 路径）。默认 Packages/com.vena.blockly/Runtime/Generated。")]
        public string OutputRoot = "Packages/com.vena.blockly/Runtime/Generated";
    }
}
