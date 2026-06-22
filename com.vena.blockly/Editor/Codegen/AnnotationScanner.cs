// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// 程序集级注解扫描器。识别带 [Blockly]（multi-target，target ∈ {Class, Method, Property, Field}）的源类、
    /// 收集其内部 [Blockly] 成员，计算并返回每个成员对应的 codegen 输入项 <see cref="ScannedMember"/>。
    /// 不发射代码、不写盘 —— 仅产出可被 emitter 消费的 POCO 集合。
    /// </summary>
    internal static class AnnotationScanner
    {
        /// <summary>
        /// 扫描入口。按 <paramref name="config"/> 程序集白名单 + 类型白名单过滤、
        /// 跳过自身已带 [BlocklySource] 的类、按 [BlocklySourceProperty.Order] 升序固定顺序。
        /// 硬约束（Q1）：同一类上 [Blockly] 与 [BlocklySource] 不允许同存；[Blockly] 不允许打在 runtime 节点源根类继承链上。
        /// </summary>
        public static IReadOnlyList<ScannedSource> Scan(CodegenConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            var asmFilter = ToSet(config.AssemblyWhitelist);
            var typeFilter = ToSet(config.TypeWhitelist);

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var results = new List<ScannedSource>();

            foreach (var asm in assemblies)
            {
                if (asmFilter.Count > 0 && !asmFilter.Contains(asm.GetName().Name))
                {
                    continue;
                }

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types.Where(t => t != null).ToArray(); }

                foreach (var type in types)
                {
                    if (type == null || !type.IsClass || type.IsAbstract) continue;
                    if (typeFilter.Count > 0 && !typeFilter.Contains(type.FullName)) continue;

                    // 跳过自身就是 runtime 节点源（[BlocklySource]）。
                    if (type.GetCustomAttribute<BlocklySourceAttribute>() != null) continue;

                    var classAttr = type.GetCustomAttribute<BlocklyAttribute>();
                    if (classAttr == null) continue;

                    // Q1 硬约束 #1：[Blockly] 不允许与 [BlocklySource] 同存。
                    // （上一行 skip 已覆盖大多数情况，这里保留显式判断作为防御性兜底——上游若改 skip 逻辑，
                    //  本约束仍能保护语义不变。）
                    if (type.GetCustomAttribute<BlocklySourceAttribute>() != null)
                    {
                        throw new InvalidOperationException(
                            $"[Vena.Blockly] {type.FullName}: [Blockly] 不允许与 [BlocklySource] 同存"
                            + "（两族正交：[Blockly] 是 codegen 输入族；[BlocklySource] 是手写 runtime 节点源族）。");
                    }

                    // Q1 硬约束 #2：[Blockly] 不允许打在 runtime 节点源类上（继承自 Expression / BehaviorNodeSource /
                    // Block<> / BehaviorNode<,>）。
                    {
                        var cursor = type.BaseType;
                        while (cursor != null && cursor != typeof(object))
                        {
                            if (IsRuntimeNodeRootType(cursor))
                            {
                                throw new InvalidOperationException(
                                    $"[Vena.Blockly] {type.FullName}: [Blockly] 不允许打在 runtime 节点源类上"
                                    + "（继承自 Expression / BehaviorNodeSource / Block<> / BehaviorNode<,>）。"
                                    + "若意图是注册 runtime 节点源，请改用 [BlocklySource]。");
                            }
                            cursor = cursor.BaseType;
                        }
                    }

                    var members = CollectMembers(type, classAttr);
                    if (members.Count == 0) continue;

                    var scanned = new ScannedSource(type, classAttr.DisplayName, members);
                    scanned.SourceDirectory = ResolveSourceDirectory(type);
                    results.Add(scanned);
                }
            }

            return results;
        }

        /// <summary>
        /// 定位源类的物理目录：<c>Type.Assembly</c> → 同名 asmdef → asmdef 所在目录 →
        /// 该目录递归查找 <c>&lt;Type.Name&gt;.cs</c> 唯一文件 → 其 <see cref="Path.GetDirectoryName"/>。
        /// 找不到或多匹配均 fail-fast，让 codegen run 中止（异常冒泡到 <c>CodegenMenu.RunCodegen</c> 的 try/catch）。
        /// </summary>
        private static string ResolveSourceDirectory(Type type)
        {
            var asmName = type.Assembly.GetName().Name;

            // Unity AssetDatabase 通过 asmdef 的名字查找其资产路径。
            var asmdefGuids = AssetDatabase.FindAssets($"{asmName} t:asmdef");
            string asmdefAssetPath = null;
            foreach (var guid in asmdefGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;
                var fileName = Path.GetFileNameWithoutExtension(path);
                if (string.Equals(fileName, asmName, StringComparison.Ordinal))
                {
                    asmdefAssetPath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(asmdefAssetPath))
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] 无法定位源类物理目录: {type.FullName}（未找到 asmdef \"{asmName}\"）");
            }

            // AssetDatabase 路径是项目相对路径，需要转成磁盘绝对路径。
            var projectRoot = Path.GetDirectoryName(UnityEngine.Application.dataPath);
            var asmdefAbs = Path.GetFullPath(Path.Combine(projectRoot, asmdefAssetPath));
            var asmdefDir = Path.GetDirectoryName(asmdefAbs);

            var simpleName = type.Name;
            // 嵌套类不应作为 [Blockly] 源类，但若 Type.Name 含 backtick（泛型），剥掉之。
            var tickIdx = simpleName.IndexOf('`');
            if (tickIdx >= 0) simpleName = simpleName.Substring(0, tickIdx);

            var matches = Directory.GetFiles(asmdefDir, simpleName + ".cs", SearchOption.AllDirectories);
            if (matches.Length == 0)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] 无法定位源类物理目录: {type.FullName}"
                    + $"（在 {asmdefDir} 下未找到 {simpleName}.cs）");
            }
            if (matches.Length > 1)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] 无法定位源类物理目录: {type.FullName}"
                    + $"（在 {asmdefDir} 下找到多个 {simpleName}.cs："
                    + string.Join(", ", matches) + "）");
            }

            return Path.GetDirectoryName(matches[0]);
        }

        private static IReadOnlyList<ScannedMember> CollectMembers(Type type, BlocklyAttribute classAttr)
        {
            var list = new List<ScannedMember>();

            // 仅扫描 public instance/static 成员。private 不进入扫描集（注解仅作用于公共 API）。
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var method in type.GetMethods(flags))
            {
                var methodAttr = method.GetCustomAttribute<BlocklyAttribute>();
                if (methodAttr == null) continue;

                ValidateMethodSignature(type, method, methodAttr);
                list.Add(ScannedMember.FromMethod(type, method, methodAttr, classAttr.DisplayName));
            }

            foreach (var prop in type.GetProperties(flags))
            {
                var propAttr = prop.GetCustomAttribute<BlocklyAttribute>();
                if (propAttr == null) continue;

                if (prop.GetGetMethod(false) != null)
                {
                    list.Add(ScannedMember.FromPropertyGetter(type, prop, propAttr, classAttr.DisplayName));
                }
                if (prop.GetSetMethod(false) != null)
                {
                    list.Add(ScannedMember.FromPropertySetter(type, prop, propAttr, classAttr.DisplayName));
                }
            }

            foreach (var field in type.GetFields(flags))
            {
                var fieldAttr = field.GetCustomAttribute<BlocklyAttribute>();
                if (fieldAttr == null) continue;

                // field 与 property 在 C# 中 `instance.X` 字面同形，CodeWriter 不需要区分来源，
                // 因此复用 PropertyGetter / PropertySetter 的产物模板。readonly field 仅 getter。
                list.Add(ScannedMember.FromFieldGetter(type, field, fieldAttr, classAttr.DisplayName));
                if (!field.IsInitOnly)
                {
                    list.Add(ScannedMember.FromFieldSetter(type, field, fieldAttr, classAttr.DisplayName));
                }
            }

            return list;
        }

        private static void ValidateMethodSignature(Type type, MethodInfo method, BlocklyAttribute attr)
        {
            var paramInfos = method.GetParameters();
            var declaredNames = attr.ParameterNames ?? Array.Empty<string>();
            if (declaredNames.Length != paramInfos.Length)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [Blockly] parameterNames 数 ({declaredNames.Length}) " +
                    $"与方法签名参数数 ({paramInfos.Length}) 不匹配。");
            }
            if (attr.IsStatic && !method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [Blockly] isStatic=true 但方法非 static。");
            }
            if (!attr.IsStatic && method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [Blockly] isStatic=false 但方法是 static。");
            }
        }

        private static bool IsRuntimeNodeRootType(Type t)
        {
            if (t == typeof(Expression)) return true;
            if (t == typeof(BehaviorNodeSource)) return true;
            if (t.IsGenericType)
            {
                var def = t.GetGenericTypeDefinition();
                if (def.FullName == "Vena.Blockly.Block`1") return true;
                if (def.FullName == "Vena.Blockly.BehaviorNode`2") return true;
            }
            return false;
        }

        private static HashSet<string> ToSet(string[] arr)
        {
            return new HashSet<string>(arr ?? Array.Empty<string>(), StringComparer.Ordinal);
        }
    }

    /// <summary>扫描结果：一个 [Blockly] 源类 + 其下所有 codegen 成员。</summary>
    internal sealed class ScannedSource
    {
        public Type SourceType { get; }
        public string ClassDisplayName { get; }
        public IReadOnlyList<ScannedMember> Members { get; }
        /// <summary>源类物理目录（其 .cs 所在目录绝对路径）。emitter 用以将三件套产物落到 `<SourceDirectory>/Generated/`。</summary>
        public string SourceDirectory { get; set; }

        public ScannedSource(Type sourceType, string classDisplayName, IReadOnlyList<ScannedMember> members)
        {
            SourceType = sourceType;
            ClassDisplayName = classDisplayName;
            Members = members;
        }
    }

    internal enum ScannedMemberKind
    {
        Method,
        PropertyGetter,
        PropertySetter,
    }

    /// <summary>扫描结果项：单个 codegen 成员（一个三件套对应一项）。</summary>
    internal sealed class ScannedMember
    {
        public ScannedMemberKind Kind { get; }
        public Type DeclaringType { get; }
        /// <summary>方法对应 method.Name；属性 getter / setter 对应 property.Name；字段 getter / setter 对应 field.Name。</summary>
        public string MemberName { get; }
        /// <summary>UI 显示名（来自 [Blockly].DisplayName）。</summary>
        public string DisplayName { get; }
        /// <summary>菜单路径 = `&lt;Blockly.DisplayName on class&gt;/&lt;DisplayName&gt;`。</summary>
        public string MenuPath { get; }
        /// <summary>是否静态（method = 方法静态性；property / field = accessor 的静态性）。</summary>
        public bool IsStatic { get; }
        /// <summary>方法返回型；procedure 时为 typeof(void)。属性 / 字段 getter = 成员类型；setter = void。</summary>
        public Type ReturnType { get; }
        /// <summary>参数列表（method = 方法参数；setter = 单 value 参数；getter = 空）。</summary>
        public IReadOnlyList<ParamInfo> Parameters { get; }

        private ScannedMember(
            ScannedMemberKind kind,
            Type declaringType,
            string memberName,
            string displayName,
            string menuPath,
            bool isStatic,
            Type returnType,
            IReadOnlyList<ParamInfo> parameters)
        {
            Kind = kind;
            DeclaringType = declaringType;
            MemberName = memberName;
            DisplayName = displayName;
            MenuPath = menuPath;
            IsStatic = isStatic;
            ReturnType = returnType;
            Parameters = parameters;
        }

        public static ScannedMember FromMethod(Type type, MethodInfo method, BlocklyAttribute attr, string classDisplayName)
        {
            var paramInfos = method.GetParameters();
            var ps = new ParamInfo[paramInfos.Length];
            for (int i = 0; i < paramInfos.Length; i++)
            {
                ps[i] = new ParamInfo(attr.ParameterNames[i], paramInfos[i].Name, paramInfos[i].ParameterType);
            }
            return new ScannedMember(
                ScannedMemberKind.Method,
                type,
                method.Name,
                attr.DisplayName,
                $"{classDisplayName}/{attr.DisplayName}",
                attr.IsStatic,
                method.ReturnType,
                ps);
        }

        public static ScannedMember FromPropertyGetter(Type type, PropertyInfo prop, BlocklyAttribute attr, string classDisplayName)
        {
            var getter = prop.GetGetMethod(false);
            return new ScannedMember(
                ScannedMemberKind.PropertyGetter,
                type,
                prop.Name,
                attr.DisplayName,
                $"{classDisplayName}/{attr.DisplayName}",
                getter.IsStatic,
                prop.PropertyType,
                Array.Empty<ParamInfo>());
        }

        public static ScannedMember FromPropertySetter(Type type, PropertyInfo prop, BlocklyAttribute attr, string classDisplayName)
        {
            var setter = prop.GetSetMethod(false);
            var ps = new[] { new ParamInfo("value", "value", prop.PropertyType) };
            return new ScannedMember(
                ScannedMemberKind.PropertySetter,
                type,
                prop.Name,
                attr.DisplayName,
                $"{classDisplayName}/{attr.DisplayName}",
                setter.IsStatic,
                typeof(void),
                ps);
        }

        public static ScannedMember FromFieldGetter(Type type, FieldInfo field, BlocklyAttribute attr, string classDisplayName)
        {
            return new ScannedMember(
                ScannedMemberKind.PropertyGetter,
                type,
                field.Name,
                attr.DisplayName,
                $"{classDisplayName}/{attr.DisplayName}",
                field.IsStatic,
                field.FieldType,
                Array.Empty<ParamInfo>());
        }

        public static ScannedMember FromFieldSetter(Type type, FieldInfo field, BlocklyAttribute attr, string classDisplayName)
        {
            var ps = new[] { new ParamInfo("value", "value", field.FieldType) };
            return new ScannedMember(
                ScannedMemberKind.PropertySetter,
                type,
                field.Name,
                attr.DisplayName,
                $"{classDisplayName}/{attr.DisplayName}",
                field.IsStatic,
                typeof(void),
                ps);
        }
    }

    internal sealed class ParamInfo
    {
        /// <summary>参数 UI 显示名（来自 [Blockly].ParameterNames[i]）。</summary>
        public string DisplayName { get; }
        /// <summary>C# 标识符（方法签名参数名）。</summary>
        public string Identifier { get; }
        public Type ParameterType { get; }

        public ParamInfo(string displayName, string identifier, Type parameterType)
        {
            DisplayName = displayName;
            Identifier = identifier;
            ParameterType = parameterType;
        }
    }
}
