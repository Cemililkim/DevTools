using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Globalization;

namespace DevTools.Controllers
{
    public class XmlToController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConvertToJSON(string xmlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xmlInput))
                {
                    return Json(new { success = false, error = "XML data cannot be empty." });
                }

                var json = ConvertXMLToJson(xmlInput);
                return Json(new { success = true, result = json });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToCSV(string xmlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xmlInput))
                {
                    return Json(new { success = false, error = "XML data cannot be empty." });
                }

                var csv = ConvertXMLToCSV(xmlInput);
                return Json(new { success = true, result = csv });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToYAML(string xmlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xmlInput))
                {
                    return Json(new { success = false, error = "XML data cannot be empty." });
                }

                var json = ConvertXMLToJson(xmlInput);
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                var yaml = ConvertJsonToYAML(jsonElement);

                return Json(new { success = true, result = yaml });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToHTML(string xmlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xmlInput))
                {
                    return Json(new { success = false, error = "XML data cannot be empty." });
                }

                var html = ConvertXMLToHTML(xmlInput);
                return Json(new { success = true, result = html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult FormatXML(string xmlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(xmlInput))
                {
                    return Json(new { success = false, error = "XML data cannot be empty." });
                }

                var formatted = FormatXMLData(xmlInput);
                return Json(new { success = true, result = formatted });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Formatting error: {ex.Message}" });
            }
        }

        private string ConvertXMLToJson(string xmlInput)
        {
            var doc = XDocument.Parse(xmlInput);
            var result = ConvertXElementToObject(doc.Root);

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private object ConvertXElementToObject(XElement element)
        {
            if (element.HasElements)
            {
                var obj = new Dictionary<string, object>();

                // Add attributes
                foreach (var attr in element.Attributes())
                {
                    obj[$"@{attr.Name}"] = ParseValue(attr.Value);
                }

                // Group elements by name
                var elementGroups = element.Elements().GroupBy(e => e.Name.LocalName);

                foreach (var group in elementGroups)
                {
                    if (group.Count() == 1)
                    {
                        obj[group.Key] = ConvertXElementToObject(group.First());
                    }
                    else
                    {
                        obj[group.Key] = group.Select(ConvertXElementToObject).ToArray();
                    }
                }

                // Add text content if exists and no child elements
                if (!element.HasElements && !string.IsNullOrWhiteSpace(element.Value))
                {
                    if (obj.Count == 0)
                        return ParseValue(element.Value);
                    else
                        obj["#text"] = ParseValue(element.Value);
                }

                return obj;
            }
            else
            {
                var result = new Dictionary<string, object>();

                // Add attributes
                foreach (var attr in element.Attributes())
                {
                    result[$"@{attr.Name}"] = ParseValue(attr.Value);
                }

                // Add text content
                if (!string.IsNullOrWhiteSpace(element.Value))
                {
                    if (result.Count == 0)
                        return ParseValue(element.Value);
                    else
                        result["#text"] = ParseValue(element.Value);
                }

                return result.Count > 0 ? result : ParseValue(element.Value);
            }
        }

        private string ConvertXMLToCSV(string xmlInput)
        {
            var doc = XDocument.Parse(xmlInput);
            var csv = new StringBuilder();
            var rows = new List<Dictionary<string, string>>();

            // Find repeating elements (likely rows)
            var rootElement = doc.Root;
            var childElements = rootElement.Elements().ToList();

            if (childElements.Count == 0)
            {
                throw new ArgumentException("XML must contain child elements to convert to CSV");
            }

            // Check if all children have the same name (array-like structure)
            var firstChildName = childElements.First().Name.LocalName;
            bool isArrayLike = childElements.All(e => e.Name.LocalName == firstChildName);

            List<XElement> dataElements;
            if (isArrayLike)
            {
                dataElements = childElements;
            }
            else
            {
                // Look for the first element that has multiple children with same names
                dataElements = childElements.Where(e => e.HasElements).SelectMany(e => e.Elements()).ToList();
                if (dataElements.Count == 0)
                {
                    dataElements = childElements;
                }
            }

            // Extract all unique column names
            var allColumns = new HashSet<string>();
            foreach (var element in dataElements)
            {
                ExtractColumnNames(element, "", allColumns);
            }

            var columnList = allColumns.OrderBy(c => c).ToList();

            // Write header
            csv.AppendLine(string.Join(",", columnList.Select(EscapeCsvValue)));

            // Write data rows
            foreach (var element in dataElements)
            {
                var row = new List<string>();
                foreach (var column in columnList)
                {
                    var value = GetValueFromPath(element, column);
                    row.Add(EscapeCsvValue(value));
                }
                csv.AppendLine(string.Join(",", row));
            }

            return csv.ToString().TrimEnd();
        }

        private void ExtractColumnNames(XElement element, string prefix, HashSet<string> columns)
        {
            // Add attributes
            foreach (var attr in element.Attributes())
            {
                var columnName = string.IsNullOrEmpty(prefix) ? $"@{attr.Name}" : $"{prefix}.@{attr.Name}";
                columns.Add(columnName);
            }

            if (element.HasElements)
            {
                foreach (var child in element.Elements())
                {
                    var columnName = string.IsNullOrEmpty(prefix) ? child.Name.LocalName : $"{prefix}.{child.Name.LocalName}";

                    if (child.HasElements)
                    {
                        ExtractColumnNames(child, columnName, columns);
                    }
                    else
                    {
                        columns.Add(columnName);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(element.Value))
            {
                var columnName = string.IsNullOrEmpty(prefix) ? "value" : prefix;
                columns.Add(columnName);
            }
        }

        private string GetValueFromPath(XElement element, string path)
        {
            var parts = path.Split('.');
            var current = element;

            foreach (var part in parts)
            {
                if (part.StartsWith("@"))
                {
                    var attrName = part.Substring(1);
                    return current.Attribute(attrName)?.Value ?? "";
                }
                else if (part == "value")
                {
                    return current.Value;
                }
                else
                {
                    current = current.Element(part);
                    if (current == null) return "";
                }
            }

            return current?.Value ?? "";
        }

        private string ConvertXMLToHTML(string xmlInput)
        {
            var doc = XDocument.Parse(xmlInput);
            var html = new StringBuilder();

            html.AppendLine("<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse;'>");
            html.AppendLine("<thead><tr><th>Element</th><th>Attributes</th><th>Value</th></tr></thead>");
            html.AppendLine("<tbody>");
            ConvertXElementToTable(doc.Root, html);
            html.AppendLine("</tbody>");
            html.AppendLine("</table>");

            return html.ToString();
        }

        private void ConvertXElementToTable(XElement element, StringBuilder html)
        {
            var attributes = string.Join(" ", element.Attributes()
                .Select(a => $"{a.Name}=\"{System.Web.HttpUtility.HtmlEncode(a.Value)}\""));

            var value = element.HasElements ? "" : System.Web.HttpUtility.HtmlEncode(element.Value);

            html.AppendLine("<tr>");
            html.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(element.Name.LocalName)}</td>");
            html.AppendLine($"<td>{attributes}</td>");
            html.AppendLine($"<td>{value}</td>");
            html.AppendLine("</tr>");

            foreach (var child in element.Elements())
            {
                ConvertXElementToTable(child, html);
            }
        }

        private void ConvertXElementToHTML(XElement element, StringBuilder html, int depth)
        {
            var indent = new string(' ', depth * 2);

            html.AppendLine($"{indent}<div style=\"margin-left: {depth * 20}px;\">");

            // Element name and attributes
            var elementInfo = new StringBuilder();
            elementInfo.Append($"&lt;{element.Name.LocalName}");

            foreach (var attr in element.Attributes())
            {
                elementInfo.Append($" {attr.Name}=\"{System.Web.HttpUtility.HtmlEncode(attr.Value)}\"");
            }
            elementInfo.Append("&gt;");

            html.AppendLine($"{indent}  <div>{elementInfo}</div>");

            if (element.HasElements)
            {
                foreach (var child in element.Elements())
                {
                    ConvertXElementToHTML(child, html, depth + 1);
                }
            }
            else if (!string.IsNullOrWhiteSpace(element.Value))
            {
                html.AppendLine($"{indent}  <div>{System.Web.HttpUtility.HtmlEncode(element.Value)}</div>");
            }

            html.AppendLine($"{indent}  <div>&lt;/{element.Name.LocalName}&gt;</div>");
            html.AppendLine($"{indent}</div>");
        }

        private string FormatXMLData(string xmlInput)
        {
            var doc = XDocument.Parse(xmlInput);
            return doc.ToString();
        }

        private string ConvertJsonToYAML(JsonElement jsonElement)
        {
            var yaml = new StringBuilder();
            ConvertJsonElementToYAML(jsonElement, yaml, 0);
            return yaml.ToString().TrimEnd();
        }

        private void ConvertJsonElementToYAML(JsonElement element, StringBuilder yaml, int depth)
        {
            var indent = new string(' ', depth * 2);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    if (depth > 0) yaml.AppendLine();
                    foreach (var prop in element.EnumerateObject())
                    {
                        yaml.Append($"{indent}{EscapeYamlKey(prop.Name)}:");
                        if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            ConvertJsonElementToYAML(prop.Value, yaml, depth + 1);
                        }
                        else
                        {
                            yaml.AppendLine($" {EscapeYamlValue(GetJsonElementValue(prop.Value))}");
                        }
                    }
                    break;
                case JsonValueKind.Array:
                    yaml.AppendLine();
                    foreach (var item in element.EnumerateArray())
                    {
                        yaml.Append($"{indent}-");
                        if (item.ValueKind == JsonValueKind.Object || item.ValueKind == JsonValueKind.Array)
                        {
                            ConvertJsonElementToYAML(item, yaml, depth + 1);
                        }
                        else
                        {
                            yaml.AppendLine($" {EscapeYamlValue(GetJsonElementValue(item))}");
                        }
                    }
                    break;
                default:
                    yaml.AppendLine($"{indent}{EscapeYamlValue(GetJsonElementValue(element))}");
                    break;
            }
        }

        private string EscapeYamlKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "\"\"";

            if (key.Contains(':') || key.Contains('-') || key.Contains('#') ||
                key.Contains('[') || key.Contains(']') || key.Contains('{') || key.Contains('}') ||
                key.StartsWith(' ') || key.EndsWith(' ') || char.IsDigit(key[0]))
            {
                return $"\"{key.Replace("\"", "\\\"")}\"";
            }

            return key;
        }

        private string EscapeYamlValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";

            if (value.Contains('\n') || value.Contains('\r') || value.Contains(':') ||
                value.Contains('#') || value.Contains('[') || value.Contains(']') ||
                value.Contains('{') || value.Contains('}') || value.Contains('|') ||
                value.Contains('>') || value.Contains('&') || value.Contains('*') ||
                value.StartsWith(' ') || value.EndsWith(' ') ||
                value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
                value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
                double.TryParse(value, out _))
            {
                return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
            }

            return value;
        }

        private string GetJsonElementValue(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? "",
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "",
                _ => element.GetRawText()
            };
        }

        private string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";

            if (value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }

        private object ParseValue(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (int.TryParse(value, out int intVal)) return intVal;
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double doubleVal)) return doubleVal;
            if (bool.TryParse(value, out bool boolVal)) return boolVal;

            return value;
        }
    }
}