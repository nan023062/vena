using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// 程序集级 UGC 注解扫描器。识别带 [UgcClass] 的源类、收集其内部 [UgcMethod]/[UgcProperty] 成员，
    /// 计算并返回每个成员对应的 codegen 输入项 <see cref="ScannedMember"/>。
    /// 不发射代码、不写盘 —— Editor only POCO 阶段。详见 module.md `UgcAnnotationScanner`。
    /// </summary>
    internal static class UgcAnnotationScanner
    {
        /// <summary>
        /// 扫描入口。按 <paramref name="config"/> 程序集白名单 + 类型白名单过滤、
        /// 跳过自身已带 [UgcSource] 或 [UgcGenerated] 的类、按 [UgcSourceProperty.Order] 升序固定顺序。
        /// </summary>
        public static IReadOnlyList<ScannedSource> Scan(UgcCodegenConfig config)
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

                    // 跳过自身就是 Source / Generated
                    if (type.GetCustomAttribute<UgcSourceAttribute>() != null) continue;
                    if (type.GetCustomAttribute<UgcGeneratedAttribute>() != null) continue;

                    var classAttr = type.GetCustomAttribute<UgcClassAttribute>();
                    if (classAttr == null) continue;

                    var members = CollectMembers(type, classAttr);
                    if (members.Count == 0) continue;

                    results.Add(new ScannedSource(type, classAttr.DisplayName, members));
                }
            }

            return results;
        }

        private static IReadOnlyList<ScannedMember> CollectMembers(Type type, UgcClassAttribute classAttr)
        {
            var list = new List<ScannedMember>();

            // 仅扫描 public instance/static 成员。private 不进入扫描集（Editor 合约 §1 注解作用面要求）。
            const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

            foreach (var method in type.GetMethods(flags))
            {
                var methodAttr = method.GetCustomAttribute<UgcMethodAttribute>();
                if (methodAttr == null) continue;

                ValidateMethodSignature(type, method, methodAttr);
                list.Add(ScannedMember.FromMethod(type, method, methodAttr, classAttr.DisplayName));
            }

            foreach (var prop in type.GetProperties(flags))
            {
                var propAttr = prop.GetCustomAttribute<UgcPropertyAttribute>();
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

            return list;
        }

        private static void ValidateMethodSignature(Type type, MethodInfo method, UgcMethodAttribute attr)
        {
            var paramInfos = method.GetParameters();
            var declaredNames = attr.ParameterNames ?? Array.Empty<string>();
            if (declaredNames.Length != paramInfos.Length)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [UgcMethod] parameterNames 数 ({declaredNames.Length}) " +
                    $"与方法签名参数数 ({paramInfos.Length}) 不匹配。Editor 合约 §1 要求严格一致。");
            }
            if (attr.IsStatic && !method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [UgcMethod] isStatic=true 但方法非 static。");
            }
            if (!attr.IsStatic && method.IsStatic)
            {
                throw new InvalidOperationException(
                    $"[Vena.Blockly] {type.FullName}.{method.Name}: [UgcMethod] isStatic=false 但方法是 static。");
            }
        }

        private static HashSet<string> ToSet(string[] arr)
        {
            return new HashSet<string>(arr ?? Array.Empty<string>(), StringComparer.Ordinal);
        }
    }

    /// <summary>扫描结果：一个 [UgcClass] 源类 + 其下所有 codegen 成员。</summary>
    internal sealed class ScannedSource
    {
        public Type SourceType { get; }
        public string ClassDisplayName { get; }
        public IReadOnlyList<ScannedMember> Members { get; }

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
        /// <summary>方法对应 method.Name；属性 getter / setter 对应 property.Name。</summary>
        public string MemberName { get; }
        /// <summary>UI 显示名（来自 [UgcMethod].DisplayName / [UgcProperty].DisplayName）。</summary>
        public string DisplayName { get; }
        /// <summary>菜单路径 = `<UgcClass.DisplayName>/<DisplayName>`。</summary>
        public string MenuPath { get; }
        /// <summary>是否静态（仅 method 有意义；property 复用 IsStatic = property accessor 的静态性）。</summary>
        public bool IsStatic { get; }
        /// <summary>方法返回型；procedure 时为 typeof(void)。属性 getter = property type；setter = void。</summary>
        public Type ReturnType { get; }
        /// <summary>参数列表（method = 方法参数；property setter = 单 value 参数；getter = 空）。</summary>
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

        public static ScannedMember FromMethod(Type type, MethodInfo method, UgcMethodAttribute attr, string classDisplayName)
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

        public static ScannedMember FromPropertyGetter(Type type, PropertyInfo prop, UgcPropertyAttribute attr, string classDisplayName)
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

        public static ScannedMember FromPropertySetter(Type type, PropertyInfo prop, UgcPropertyAttribute attr, string classDisplayName)
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
    }

    internal sealed class ParamInfo
    {
        /// <summary>参数 UI 显示名（来自 [UgcMethod].parameterNames[i]）。</summary>
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
