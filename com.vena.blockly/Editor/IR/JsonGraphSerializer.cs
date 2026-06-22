// -----------------------------------------------------------------------------
// Vena Blockly
// Visual scripting block graph runtime for the Vena open-source Unity framework.
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Vena.Blockly.Editor
{

    /// <summary>
    /// IR JSON 序列化器。
    ///
    /// canonical 形态：
    /// - 字段顺序固定：顶层 schema/kind/rootNodeGuid/nodes/edges；
    ///   NodeIR  guid/sourceType/properties/position；
    ///   EdgeIR  from/to/wireKind；
    ///   PortRef nodeGuid/port；
    ///   Vec2    x/y；
    ///   NodePropertyIR key/value；
    ///   PropertyValueIR type/value。
    /// - 紧凑分隔符：键值 `":"`、项 `","`，缩进 0、无空白。
    /// - UTF-8 无 BOM、无尾随换行。字节级稳定。
    /// - 字符串不转 ASCII，保留原 UTF-8 码点；只做 JSON 必要转义（"\\\"\\\\\\b\\f\\n\\r\\t" 与 U+0000..U+001F）。
    /// </summary>
    internal sealed class JsonGraphSerializer : IBlocklyGraphSerializer
    {
        // ---------------------------------------------------------------- ToJson

        public string ToJson(GraphIR ir)
        {
            if (ir == null) throw new BlocklyIRSchemaException("GraphIR is null.");

            var sb = new StringBuilder(256);
            WriteGraph(sb, ir);
            return sb.ToString();
        }

        private static void WriteGraph(StringBuilder sb, GraphIR ir)
        {
            sb.Append('{');
            WriteIntField(sb, "schema", ir.Schema, first: true);
            WriteStringField(sb, "kind", KindToString(ir.Kind), first: false);
            WriteStringField(sb, "rootNodeGuid", ir.RootNodeGuid.ToString("D"), first: false);

            sb.Append(",\"nodes\":[");
            if (ir.Nodes != null)
            {
                for (int i = 0; i < ir.Nodes.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteNode(sb, ir.Nodes[i]);
                }
            }
            sb.Append(']');

            sb.Append(",\"edges\":[");
            if (ir.Edges != null)
            {
                for (int i = 0; i < ir.Edges.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteEdge(sb, ir.Edges[i]);
                }
            }
            sb.Append(']');

            sb.Append('}');
        }

        private static void WriteNode(StringBuilder sb, NodeIR node)
        {
            if (node == null) throw new BlocklyIRSchemaException("NodeIR is null.");
            sb.Append('{');
            WriteStringField(sb, "guid", node.Guid.ToString("D"), first: true);
            WriteStringField(sb, "sourceType", node.SourceType ?? string.Empty, first: false);

            sb.Append(",\"properties\":[");
            if (node.Properties != null)
            {
                for (int i = 0; i < node.Properties.Count; i++)
                {
                    if (i > 0) sb.Append(',');
                    WriteNodeProperty(sb, node.Properties[i]);
                }
            }
            sb.Append(']');

            sb.Append(",\"position\":");
            WriteVec2(sb, node.Position);
            sb.Append('}');
        }

        private static void WriteNodeProperty(StringBuilder sb, NodePropertyIR prop)
        {
            if (prop == null) throw new BlocklyIRSchemaException("NodePropertyIR is null.");
            sb.Append('{');
            WriteStringField(sb, "key", prop.Key ?? string.Empty, first: true);
            sb.Append(",\"value\":");
            WritePropertyValue(sb, prop.Value);
            sb.Append('}');
        }

        private static void WritePropertyValue(StringBuilder sb, PropertyValueIR value)
        {
            if (value == null) throw new BlocklyIRSchemaException("PropertyValueIR is null.");
            sb.Append('{');
            WriteStringField(sb, "type", value.Type ?? PropertyValueIR.TypeLiteral, first: true);
            sb.Append(",\"value\":");
            if (value.IsNodeRef)
            {
                if (value.Value is Guid g)
                {
                    sb.Append('{');
                    WriteStringField(sb, "nodeGuid", g.ToString("D"), first: true);
                    sb.Append('}');
                }
                else
                {
                    throw new BlocklyIRSchemaException(
                        $"PropertyValueIR.nodeRef value must be Guid; got {value.Value?.GetType().FullName ?? "null"}.");
                }
            }
            else
            {
                WriteJsonScalar(sb, value.Value);
            }
            sb.Append('}');
        }

        private static void WriteEdge(StringBuilder sb, EdgeIR edge)
        {
            if (edge == null) throw new BlocklyIRSchemaException("EdgeIR is null.");
            sb.Append('{');
            sb.Append("\"from\":");
            WritePortRef(sb, edge.From);
            sb.Append(",\"to\":");
            WritePortRef(sb, edge.To);
            WriteStringField(sb, "wireKind", WireKindToString(edge.WireKind), first: false);
            sb.Append('}');
        }

        private static void WritePortRef(StringBuilder sb, PortRef port)
        {
            sb.Append('{');
            WriteStringField(sb, "nodeGuid", port.NodeGuid.ToString("D"), first: true);
            WriteStringField(sb, "port", port.Port ?? string.Empty, first: false);
            sb.Append('}');
        }

        private static void WriteVec2(StringBuilder sb, Vec2 v)
        {
            sb.Append('{');
            sb.Append("\"x\":");
            sb.Append(FloatToString(v.X));
            sb.Append(",\"y\":");
            sb.Append(FloatToString(v.Y));
            sb.Append('}');
        }

        private static void WriteIntField(StringBuilder sb, string key, int value, bool first)
        {
            if (!first) sb.Append(',');
            sb.Append('"').Append(key).Append("\":");
            sb.Append(value.ToString(CultureInfo.InvariantCulture));
        }

        private static void WriteStringField(StringBuilder sb, string key, string value, bool first)
        {
            if (!first) sb.Append(',');
            sb.Append('"').Append(key).Append("\":");
            WriteEscapedString(sb, value);
        }

        private static void WriteJsonScalar(StringBuilder sb, object literal)
        {
            switch (literal)
            {
                case null:
                    sb.Append("null");
                    return;
                case bool b:
                    sb.Append(b ? "true" : "false");
                    return;
                case string s:
                    WriteEscapedString(sb, s);
                    return;
                case int i:
                    sb.Append(i.ToString(CultureInfo.InvariantCulture));
                    return;
                case long l:
                    sb.Append(l.ToString(CultureInfo.InvariantCulture));
                    return;
                case float f:
                    sb.Append(FloatToString(f));
                    return;
                case double d:
                    sb.Append(DoubleToString(d));
                    return;
                case short sh:
                    sb.Append(sh.ToString(CultureInfo.InvariantCulture));
                    return;
                case byte by:
                    sb.Append(by.ToString(CultureInfo.InvariantCulture));
                    return;
                case uint ui:
                    sb.Append(ui.ToString(CultureInfo.InvariantCulture));
                    return;
                case ulong ul:
                    sb.Append(ul.ToString(CultureInfo.InvariantCulture));
                    return;
                default:
                    throw new BlocklyIRSchemaException(
                        $"Unsupported literal type: {literal.GetType().FullName}. " +
                        $"Allowed: null/bool/string/int/long/short/byte/uint/ulong/float/double.");
            }
        }

        private static void WriteEscapedString(StringBuilder sb, string s)
        {
            sb.Append('"');
            if (s != null)
            {
                for (int i = 0; i < s.Length; i++)
                {
                    char c = s[i];
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\b': sb.Append("\\b"); break;
                        case '\f': sb.Append("\\f"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20)
                            {
                                sb.Append("\\u").Append(((int)c).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                sb.Append(c);
                            }
                            break;
                    }
                }
            }
            sb.Append('"');
        }

        private static string FloatToString(float f)
        {
            // R 形式保 round-trip；Invariant 强制点号。
            if (float.IsNaN(f) || float.IsInfinity(f))
                throw new BlocklyIRSchemaException($"Float NaN/Infinity not allowed in IR: {f}");
            return f.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string DoubleToString(double d)
        {
            if (double.IsNaN(d) || double.IsInfinity(d))
                throw new BlocklyIRSchemaException($"Double NaN/Infinity not allowed in IR: {d}");
            return d.ToString("R", CultureInfo.InvariantCulture);
        }

        private static string KindToString(GraphKind k)
        {
            switch (k)
            {
                case GraphKind.Behavior: return "Behavior";
                case GraphKind.Logic: return "Logic";
                default: throw new BlocklyIRSchemaException($"Unknown GraphKind: {k}");
            }
        }

        private static string WireKindToString(WireKind w)
        {
            switch (w)
            {
                case WireKind.Control: return "Control";
                case WireKind.Value: return "Value";
                default: throw new BlocklyIRSchemaException($"Unknown WireKind: {w}");
            }
        }

        // ---------------------------------------------------------------- FromJson

        public GraphIR FromJson(string json)
        {
            if (json == null) throw new BlocklyIRSchemaException("Input json is null.");
            var p = new JsonReader(json);
            p.SkipWs();
            var obj = p.ReadObject();
            p.SkipWs();
            if (!p.Eof) throw new BlocklyIRSchemaException("Trailing characters after top-level JSON object.");

            var ir = new GraphIR();
            HashSet<string> seen = new HashSet<string>();
            int? schema = null;
            string kindStr = null;
            string rootGuidStr = null;
            List<NodeIR> nodes = null;
            List<EdgeIR> edges = null;

            foreach (var kv in obj.Fields)
            {
                if (!seen.Add(kv.Key))
                    throw new BlocklyIRSchemaException($"Duplicate top-level key: {kv.Key}");
                switch (kv.Key)
                {
                    case "schema": schema = ExpectInt(kv.Value, "schema"); break;
                    case "kind": kindStr = ExpectString(kv.Value, "kind"); break;
                    case "rootNodeGuid": rootGuidStr = ExpectString(kv.Value, "rootNodeGuid"); break;
                    case "nodes": nodes = ParseNodes(ExpectArray(kv.Value, "nodes")); break;
                    case "edges": edges = ParseEdges(ExpectArray(kv.Value, "edges")); break;
                    default:
                        throw new BlocklyIRSchemaException($"Unknown top-level key: {kv.Key}");  // 拒绝未知字段
                }
            }

            if (!schema.HasValue) throw new BlocklyIRSchemaException("Missing required field: schema");
            if (kindStr == null) throw new BlocklyIRSchemaException("Missing required field: kind");
            if (rootGuidStr == null) throw new BlocklyIRSchemaException("Missing required field: rootNodeGuid");
            if (nodes == null) throw new BlocklyIRSchemaException("Missing required field: nodes");
            if (edges == null) throw new BlocklyIRSchemaException("Missing required field: edges");

            ir.Schema = schema.Value;
            ir.Kind = ParseKind(kindStr);
            ir.RootNodeGuid = ParseGuid(rootGuidStr, "rootNodeGuid");
            ir.Nodes = nodes;
            ir.Edges = edges;
            return ir;
        }

        private static List<NodeIR> ParseNodes(JsonArray arr)
        {
            var list = new List<NodeIR>(arr.Items.Count);
            foreach (var item in arr.Items)
            {
                var obj = ExpectObject(item, "nodes[]");
                var node = new NodeIR();
                var seen = new HashSet<string>();
                string guidStr = null;
                string sourceType = null;
                List<NodePropertyIR> props = null;
                Vec2? pos = null;
                foreach (var kv in obj.Fields)
                {
                    if (!seen.Add(kv.Key))
                        throw new BlocklyIRSchemaException($"Duplicate NodeIR key: {kv.Key}");
                    switch (kv.Key)
                    {
                        case "guid": guidStr = ExpectString(kv.Value, "node.guid"); break;
                        case "sourceType": sourceType = ExpectString(kv.Value, "node.sourceType"); break;
                        case "properties": props = ParseProperties(ExpectArray(kv.Value, "node.properties")); break;
                        case "position": pos = ParseVec2(ExpectObject(kv.Value, "node.position")); break;
                        default:
                            throw new BlocklyIRSchemaException($"Unknown NodeIR key: {kv.Key}");
                    }
                }
                if (guidStr == null) throw new BlocklyIRSchemaException("NodeIR missing field: guid");
                if (sourceType == null) throw new BlocklyIRSchemaException("NodeIR missing field: sourceType");
                if (props == null) throw new BlocklyIRSchemaException("NodeIR missing field: properties");
                if (!pos.HasValue) throw new BlocklyIRSchemaException("NodeIR missing field: position");
                node.Guid = ParseGuid(guidStr, "node.guid");
                node.SourceType = sourceType;
                node.Properties = props;
                node.Position = pos.Value;
                list.Add(node);
            }
            return list;
        }

        private static List<NodePropertyIR> ParseProperties(JsonArray arr)
        {
            var list = new List<NodePropertyIR>(arr.Items.Count);
            foreach (var item in arr.Items)
            {
                var obj = ExpectObject(item, "node.properties[]");
                var seen = new HashSet<string>();
                string key = null;
                PropertyValueIR value = null;
                foreach (var kv in obj.Fields)
                {
                    if (!seen.Add(kv.Key))
                        throw new BlocklyIRSchemaException($"Duplicate property key: {kv.Key}");
                    switch (kv.Key)
                    {
                        case "key": key = ExpectString(kv.Value, "property.key"); break;
                        case "value": value = ParsePropertyValue(ExpectObject(kv.Value, "property.value")); break;
                        default:
                            throw new BlocklyIRSchemaException($"Unknown property field: {kv.Key}");
                    }
                }
                if (key == null) throw new BlocklyIRSchemaException("property missing field: key");
                if (value == null) throw new BlocklyIRSchemaException("property missing field: value");
                list.Add(new NodePropertyIR(key, value));
            }
            return list;
        }

        private static PropertyValueIR ParsePropertyValue(JsonObject obj)
        {
            var seen = new HashSet<string>();
            string type = null;
            object rawValue = null;
            bool valueSet = false;
            foreach (var kv in obj.Fields)
            {
                if (!seen.Add(kv.Key))
                    throw new BlocklyIRSchemaException($"Duplicate PropertyValue field: {kv.Key}");
                switch (kv.Key)
                {
                    case "type": type = ExpectString(kv.Value, "PropertyValue.type"); break;
                    case "value": rawValue = kv.Value; valueSet = true; break;
                    default:
                        throw new BlocklyIRSchemaException($"Unknown PropertyValue field: {kv.Key}");
                }
            }
            if (type == null) throw new BlocklyIRSchemaException("PropertyValue missing field: type");
            if (!valueSet) throw new BlocklyIRSchemaException("PropertyValue missing field: value");
            switch (type)
            {
                case PropertyValueIR.TypeLiteral:
                    return new PropertyValueIR(PropertyValueIR.TypeLiteral, UnboxScalar(rawValue));
                case PropertyValueIR.TypeNodeRef:
                    var refObj = ExpectObject(rawValue, "nodeRef.value");
                    string nodeGuidStr = null;
                    var refSeen = new HashSet<string>();
                    foreach (var kv in refObj.Fields)
                    {
                        if (!refSeen.Add(kv.Key))
                            throw new BlocklyIRSchemaException($"Duplicate nodeRef field: {kv.Key}");
                        switch (kv.Key)
                        {
                            case "nodeGuid": nodeGuidStr = ExpectString(kv.Value, "nodeRef.nodeGuid"); break;
                            default: throw new BlocklyIRSchemaException($"Unknown nodeRef field: {kv.Key}");
                        }
                    }
                    if (nodeGuidStr == null) throw new BlocklyIRSchemaException("nodeRef missing field: nodeGuid");
                    return PropertyValueIR.NodeRef(ParseGuid(nodeGuidStr, "nodeRef.nodeGuid"));
                default:
                    throw new BlocklyIRSchemaException($"Unknown PropertyValueIR.type: {type}");
            }
        }

        private static List<EdgeIR> ParseEdges(JsonArray arr)
        {
            var list = new List<EdgeIR>(arr.Items.Count);
            foreach (var item in arr.Items)
            {
                var obj = ExpectObject(item, "edges[]");
                var seen = new HashSet<string>();
                PortRef? from = null, to = null;
                string wireStr = null;
                foreach (var kv in obj.Fields)
                {
                    if (!seen.Add(kv.Key))
                        throw new BlocklyIRSchemaException($"Duplicate EdgeIR field: {kv.Key}");
                    switch (kv.Key)
                    {
                        case "from": from = ParsePortRef(ExpectObject(kv.Value, "edge.from")); break;
                        case "to": to = ParsePortRef(ExpectObject(kv.Value, "edge.to")); break;
                        case "wireKind": wireStr = ExpectString(kv.Value, "edge.wireKind"); break;
                        default: throw new BlocklyIRSchemaException($"Unknown EdgeIR field: {kv.Key}");
                    }
                }
                if (!from.HasValue) throw new BlocklyIRSchemaException("EdgeIR missing field: from");
                if (!to.HasValue) throw new BlocklyIRSchemaException("EdgeIR missing field: to");
                if (wireStr == null) throw new BlocklyIRSchemaException("EdgeIR missing field: wireKind");
                var edge = new EdgeIR
                {
                    From = from.Value,
                    To = to.Value,
                    WireKind = ParseWireKind(wireStr),
                };
                list.Add(edge);
            }
            return list;
        }

        private static PortRef ParsePortRef(JsonObject obj)
        {
            string nodeGuidStr = null;
            string port = null;
            var seen = new HashSet<string>();
            foreach (var kv in obj.Fields)
            {
                if (!seen.Add(kv.Key))
                    throw new BlocklyIRSchemaException($"Duplicate PortRef field: {kv.Key}");
                switch (kv.Key)
                {
                    case "nodeGuid": nodeGuidStr = ExpectString(kv.Value, "port.nodeGuid"); break;
                    case "port": port = ExpectString(kv.Value, "port.port"); break;
                    default: throw new BlocklyIRSchemaException($"Unknown PortRef field: {kv.Key}");
                }
            }
            if (nodeGuidStr == null) throw new BlocklyIRSchemaException("PortRef missing field: nodeGuid");
            if (port == null) throw new BlocklyIRSchemaException("PortRef missing field: port");
            return new PortRef(ParseGuid(nodeGuidStr, "port.nodeGuid"), port);
        }

        private static Vec2 ParseVec2(JsonObject obj)
        {
            float? x = null, y = null;
            var seen = new HashSet<string>();
            foreach (var kv in obj.Fields)
            {
                if (!seen.Add(kv.Key))
                    throw new BlocklyIRSchemaException($"Duplicate Vec2 field: {kv.Key}");
                switch (kv.Key)
                {
                    case "x": x = ExpectFloat(kv.Value, "position.x"); break;
                    case "y": y = ExpectFloat(kv.Value, "position.y"); break;
                    default: throw new BlocklyIRSchemaException($"Unknown Vec2 field: {kv.Key}");
                }
            }
            if (!x.HasValue) throw new BlocklyIRSchemaException("Vec2 missing field: x");
            if (!y.HasValue) throw new BlocklyIRSchemaException("Vec2 missing field: y");
            return new Vec2(x.Value, y.Value);
        }

        // ---------------------------------------------------------------- expectations

        private static int ExpectInt(object node, string ctx)
        {
            if (node is JsonNumber n && n.IsInteger) return (int)n.AsLong;
            throw new BlocklyIRSchemaException($"Expected int at {ctx}.");
        }

        private static float ExpectFloat(object node, string ctx)
        {
            if (node is JsonNumber n) return (float)n.AsDouble;
            throw new BlocklyIRSchemaException($"Expected number at {ctx}.");
        }

        private static string ExpectString(object node, string ctx)
        {
            if (node is JsonString s) return s.Value;
            throw new BlocklyIRSchemaException($"Expected string at {ctx}.");
        }

        private static JsonObject ExpectObject(object node, string ctx)
        {
            if (node is JsonObject o) return o;
            throw new BlocklyIRSchemaException($"Expected object at {ctx}.");
        }

        private static JsonArray ExpectArray(object node, string ctx)
        {
            if (node is JsonArray a) return a;
            throw new BlocklyIRSchemaException($"Expected array at {ctx}.");
        }

        private static object UnboxScalar(object node)
        {
            switch (node)
            {
                case null:
                case JsonNull _: return null;
                case JsonString s: return s.Value;
                case JsonBool b: return b.Value;
                case JsonNumber n:
                    if (n.IsInteger)
                    {
                        long l = n.AsLong;
                        if (l >= int.MinValue && l <= int.MaxValue) return (int)l;
                        return l;
                    }
                    return n.AsDouble;
                default:
                    throw new BlocklyIRSchemaException($"Literal value must be scalar; got {node.GetType().Name}.");
            }
        }

        private static GraphKind ParseKind(string s)
        {
            switch (s)
            {
                case "Behavior": return GraphKind.Behavior;
                case "Logic": return GraphKind.Logic;
                default:
                    throw new BlocklyIRSchemaException($"Invalid kind: {s}. Allowed: Behavior, Logic.");
            }
        }

        private static WireKind ParseWireKind(string s)
        {
            switch (s)
            {
                case "Control": return WireKind.Control;
                case "Value": return WireKind.Value;
                default:
                    throw new BlocklyIRSchemaException($"Invalid wireKind: {s}. Allowed: Control, Value.");
            }
        }

        private static Guid ParseGuid(string s, string ctx)
        {
            if (Guid.TryParseExact(s, "D", out var g)) return g;
            throw new BlocklyIRSchemaException($"Invalid Guid at {ctx}: '{s}'");
        }

        // ---------------------------------------------------------------- minimal JSON AST + reader

        private abstract class JsonNode { }
        private sealed class JsonObject : JsonNode { public List<KeyValuePair<string, object>> Fields = new List<KeyValuePair<string, object>>(); }
        private sealed class JsonArray : JsonNode { public List<object> Items = new List<object>(); }
        private sealed class JsonString : JsonNode { public string Value; }
        private sealed class JsonNumber : JsonNode { public string Raw; public bool IsInteger; public long AsLong; public double AsDouble; }
        private sealed class JsonBool : JsonNode { public bool Value; }
        private sealed class JsonNull : JsonNode { }

        private sealed class JsonReader
        {
            private readonly string _s;
            private int _i;
            public JsonReader(string s) { _s = s; _i = 0; }
            public bool Eof => _i >= _s.Length;

            public void SkipWs()
            {
                while (_i < _s.Length)
                {
                    char c = _s[_i];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r') _i++;
                    else break;
                }
            }

            public object ReadValue()
            {
                SkipWs();
                if (_i >= _s.Length) throw new BlocklyIRSchemaException("Unexpected end of input.");
                char c = _s[_i];
                switch (c)
                {
                    case '{': return ReadObject();
                    case '[': return ReadArray();
                    case '"': return new JsonString { Value = ReadString() };
                    case 't': case 'f': return new JsonBool { Value = ReadBoolLiteral() };
                    case 'n': ReadNullLiteral(); return new JsonNull();
                    default:
                        if (c == '-' || (c >= '0' && c <= '9')) return ReadNumber();
                        throw new BlocklyIRSchemaException($"Unexpected token '{c}' at offset {_i}.");
                }
            }

            public JsonObject ReadObject()
            {
                Expect('{');
                var obj = new JsonObject();
                SkipWs();
                if (Peek() == '}') { _i++; return obj; }
                while (true)
                {
                    SkipWs();
                    string key = ReadString();
                    SkipWs();
                    Expect(':');
                    object value = ReadValue();
                    obj.Fields.Add(new KeyValuePair<string, object>(key, value));
                    SkipWs();
                    if (Peek() == ',') { _i++; continue; }
                    if (Peek() == '}') { _i++; break; }
                    throw new BlocklyIRSchemaException($"Expected ',' or '}}' at offset {_i}.");
                }
                return obj;
            }

            private JsonArray ReadArray()
            {
                Expect('[');
                var arr = new JsonArray();
                SkipWs();
                if (Peek() == ']') { _i++; return arr; }
                while (true)
                {
                    arr.Items.Add(ReadValue());
                    SkipWs();
                    if (Peek() == ',') { _i++; continue; }
                    if (Peek() == ']') { _i++; break; }
                    throw new BlocklyIRSchemaException($"Expected ',' or ']' at offset {_i}.");
                }
                return arr;
            }

            private string ReadString()
            {
                Expect('"');
                var sb = new StringBuilder();
                while (_i < _s.Length)
                {
                    char c = _s[_i++];
                    if (c == '"') return sb.ToString();
                    if (c == '\\')
                    {
                        if (_i >= _s.Length) throw new BlocklyIRSchemaException("Unterminated escape.");
                        char esc = _s[_i++];
                        switch (esc)
                        {
                            case '"': sb.Append('"'); break;
                            case '\\': sb.Append('\\'); break;
                            case '/': sb.Append('/'); break;
                            case 'b': sb.Append('\b'); break;
                            case 'f': sb.Append('\f'); break;
                            case 'n': sb.Append('\n'); break;
                            case 'r': sb.Append('\r'); break;
                            case 't': sb.Append('\t'); break;
                            case 'u':
                                if (_i + 4 > _s.Length) throw new BlocklyIRSchemaException("Truncated \\u escape.");
                                if (!ushort.TryParse(_s.Substring(_i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var code))
                                    throw new BlocklyIRSchemaException($"Invalid \\u escape at offset {_i}.");
                                sb.Append((char)code);
                                _i += 4;
                                break;
                            default:
                                throw new BlocklyIRSchemaException($"Invalid escape '\\{esc}' at offset {_i - 1}.");
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                throw new BlocklyIRSchemaException("Unterminated string.");
            }

            private bool ReadBoolLiteral()
            {
                if (_s.Length - _i >= 4 && _s.Substring(_i, 4) == "true") { _i += 4; return true; }
                if (_s.Length - _i >= 5 && _s.Substring(_i, 5) == "false") { _i += 5; return false; }
                throw new BlocklyIRSchemaException($"Expected true/false at offset {_i}.");
            }

            private void ReadNullLiteral()
            {
                if (_s.Length - _i >= 4 && _s.Substring(_i, 4) == "null") { _i += 4; return; }
                throw new BlocklyIRSchemaException($"Expected null at offset {_i}.");
            }

            private JsonNumber ReadNumber()
            {
                int start = _i;
                if (_s[_i] == '-') _i++;
                while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9') _i++;
                bool isFloat = false;
                if (_i < _s.Length && _s[_i] == '.')
                {
                    isFloat = true;
                    _i++;
                    while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9') _i++;
                }
                if (_i < _s.Length && (_s[_i] == 'e' || _s[_i] == 'E'))
                {
                    isFloat = true;
                    _i++;
                    if (_i < _s.Length && (_s[_i] == '+' || _s[_i] == '-')) _i++;
                    while (_i < _s.Length && _s[_i] >= '0' && _s[_i] <= '9') _i++;
                }
                string raw = _s.Substring(start, _i - start);
                var n = new JsonNumber { Raw = raw, IsInteger = !isFloat };
                if (!isFloat)
                {
                    if (!long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out n.AsLong))
                        throw new BlocklyIRSchemaException($"Integer overflow / invalid: {raw}");
                    n.AsDouble = n.AsLong;
                }
                else
                {
                    if (!double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out n.AsDouble))
                        throw new BlocklyIRSchemaException($"Invalid number: {raw}");
                }
                return n;
            }

            private char Peek() => _i < _s.Length ? _s[_i] : '\0';
            private void Expect(char c)
            {
                if (_i >= _s.Length || _s[_i] != c)
                    throw new BlocklyIRSchemaException($"Expected '{c}' at offset {_i}.");
                _i++;
            }
        }
    }
}
