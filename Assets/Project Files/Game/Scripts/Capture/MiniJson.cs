 using System.Collections;
 using System.Text;

 internal static class MiniJson
    {
        public static string Serialize(object obj)
        {
            var sb = new StringBuilder();
            WriteValue(obj, sb);
            return sb.ToString();
        }

        static void WriteValue(object v, StringBuilder sb)
        {
            switch (v)
            {
                case null: sb.Append("null"); break;
                case string s: WriteString(s, sb); break;
                case bool b: sb.Append(b ? "true" : "false"); break;
                case IDictionary dict: WriteDict(dict, sb); break;
                case System.Collections.IEnumerable list when !(v is string): WriteList(list, sb); break;
                default:
                    if (v is float f) sb.Append(f.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                    else if (v is double d) sb.Append(d.ToString("R", System.Globalization.CultureInfo.InvariantCulture));
                    else if (v is int || v is long || v is short || v is byte)
                        sb.Append(System.Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture));
                    else WriteString(v.ToString(), sb);
                    break;
            }
        }

        static void WriteString(string s, StringBuilder sb)
        {
            sb.Append('\"');
            foreach (char c in s)
            {
                switch (c)
                {
                    case '\"': sb.Append("\\\""); break;
                    case '\\': sb.Append("\\\\"); break;
                    case '\b': sb.Append("\\b"); break;
                    case '\f': sb.Append("\\f"); break;
                    case '\n': sb.Append("\\n"); break;
                    case '\r': sb.Append("\\r"); break;
                    case '\t': sb.Append("\\t"); break;
                    default:
                        if (c < ' ' || c > 0x7E) sb.AppendFormat("\\u{0:X4}", (int)c);
                        else sb.Append(c);
                        break;
                }
            }
            sb.Append('\"');
        }

        static void WriteDict(IDictionary dict, StringBuilder sb)
        {
            sb.Append('{');
            bool first = true;
            foreach (var key in dict.Keys)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteString(key.ToString(), sb);
                sb.Append(':');
                WriteValue(dict[key], sb);
            }
            sb.Append('}');
        }

        static void WriteList(System.Collections.IEnumerable list, StringBuilder sb)
        {
            sb.Append('[');
            bool first = true;
            foreach (var it in list)
            {
                if (!first) sb.Append(',');
                first = false;
                WriteValue(it, sb);
            }
            sb.Append(']');
        }
    }