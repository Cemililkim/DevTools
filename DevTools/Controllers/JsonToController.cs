using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace DevTools.Controllers
{
    public class JsonToController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConvertToCSV(string jsonInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    return Json(new { success = false, error = "JSON data cannot be empty." });
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                var csv = ConvertJsonToCSV(jsonElement);

                return Json(new { success = true, result = csv });
            }
            catch (JsonException ex)
            {
                return Json(new { success = false, error = $"Invalid JSON format: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToXML(string jsonInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    return Json(new { success = false, error = "JSON data cannot be empty." });
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                var xml = ConvertJsonToXML(jsonElement);

                return Json(new { success = true, result = xml });
            }
            catch (JsonException ex)
            {
                return Json(new { success = false, error = $"Invalid JSON format: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToYAML(string jsonInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    return Json(new { success = false, error = "JSON data cannot be empty." });
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                var yaml = ConvertJsonToYAML(jsonElement);

                return Json(new { success = true, result = yaml });
            }
            catch (JsonException ex)
            {
                return Json(new { success = false, error = $"Invalid JSON format: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToHTML(string jsonInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    return Json(new { success = false, error = "JSON data cannot be empty." });
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                var html = ConvertJsonToHTML(jsonElement);

                return Json(new { success = true, result = html });
            }
            catch (JsonException ex)
            {
                return Json(new { success = false, error = $"Invalid JSON format: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertFromCSV(string csvInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvInput))
                {
                    return Json(new { success = false, error = "CSV data cannot be empty." });
                }

                var json = ConvertCSVToJson(csvInput);
                return Json(new { success = true, result = json });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult FormatJSON(string jsonInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(jsonInput))
                {
                    return Json(new { success = false, error = "JSON data cannot be empty." });
                }

                var jsonElement = JsonSerializer.Deserialize<JsonElement>(jsonInput);
                var formattedJson = JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                return Json(new { success = true, result = formattedJson });
            }
            catch (JsonException ex)
            {
                return Json(new { success = false, error = $"Invalid JSON format: {ex.Message}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Formatting error: {ex.Message}" });
            }
        }

        private string ConvertJsonToCSV(JsonElement jsonElement)
        {
            var csv = new StringBuilder();

            if (jsonElement.ValueKind == JsonValueKind.Array)
            {
                var items = jsonElement.EnumerateArray().ToList();
                if (items.Count == 0) return "";

                // Collect headers
                var headers = new HashSet<string>();
                foreach (var item in items)
                {
                    if (item.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in item.EnumerateObject())
                        {
                            headers.Add(prop.Name);
                        }
                    }
                }

                var headerList = headers.ToList();
                csv.AppendLine(string.Join(",", headerList.Select(EscapeCsvValue)));

                // Data
                foreach (var item in items)
                {
                    var values = new List<string>();
                    foreach (var header in headerList)
                    {
                        var value = "";
                        if (item.ValueKind == JsonValueKind.Object && item.TryGetProperty(header, out var prop))
                        {
                            value = GetJsonElementValue(prop);
                        }
                        values.Add(EscapeCsvValue(value));
                    }
                    csv.AppendLine(string.Join(",", values));
                }
            }
            else if (jsonElement.ValueKind == JsonValueKind.Object)
            {
                csv.AppendLine("Key,Value");
                foreach (var prop in jsonElement.EnumerateObject())
                {
                    csv.AppendLine($"{EscapeCsvValue(prop.Name)},{EscapeCsvValue(GetJsonElementValue(prop.Value))}");
                }
            }

            return csv.ToString();
        }

        private string ConvertCSVToJson(string csvInput)
        {
            var lines = csvInput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 2) throw new ArgumentException("CSV must contain at least 2 lines (header + data)");

            var headers = ParseCSVLine(lines[0]);
            var result = new List<Dictionary<string, object>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCSVLine(lines[i]);
                var obj = new Dictionary<string, object>();

                for (int j = 0; j < Math.Min(headers.Length, values.Length); j++)
                {
                    obj[headers[j]] = ParseValue(values[j]);
                }
                result.Add(obj);
            }

            return JsonSerializer.Serialize(result, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private string ConvertJsonToXML(JsonElement jsonElement)
        {
            var xml = new StringBuilder();
            xml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            xml.AppendLine("<root>");
            ConvertJsonElementToXML(jsonElement, xml, 1);
            xml.AppendLine("</root>");
            return xml.ToString();
        }

        private string ConvertJsonToYAML(JsonElement jsonElement)
        {
            var yaml = new StringBuilder();
            ConvertJsonElementToYAML(jsonElement, yaml, 0);
            return yaml.ToString().TrimEnd();
        }

        private string ConvertJsonToHTML(JsonElement jsonElement)
        {
            var html = new StringBuilder();

            html.AppendLine("<div class=\"json-container\">");
            ConvertJsonElementToHTMLRecursive(jsonElement, html, "Root", 0);
            html.AppendLine("</div>");

            return html.ToString();
        }

        private void ConvertJsonElementToHTMLRecursive(JsonElement element, StringBuilder html, string title, int depth)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    html.AppendLine($"<div class=\"nested-object\">");
                    html.AppendLine($"<div class=\"key-label\">{System.Web.HttpUtility.HtmlEncode(title)} (Object)</div>");
                    html.AppendLine("<table>");
                    html.AppendLine("  <thead>");
                    html.AppendLine("    <tr>");
                    html.AppendLine("      <th style=\"width: 200px;\">Property</th>");
                    html.AppendLine("      <th>Value</th>");
                    html.AppendLine("    </tr>");
                    html.AppendLine("  </thead>");
                    html.AppendLine("  <tbody>");

                    foreach (var prop in element.EnumerateObject())
                    {
                        html.AppendLine("    <tr>");
                        html.AppendLine($"      <td><strong>{System.Web.HttpUtility.HtmlEncode(prop.Name)}</strong></td>");
                        html.AppendLine("      <td>");

                        if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            ConvertJsonElementToHTMLRecursive(prop.Value, html, prop.Name, depth + 1);
                        }
                        else
                        {
                            html.AppendLine(FormatSimpleValue(prop.Value));
                        }

                        html.AppendLine("      </td>");
                        html.AppendLine("    </tr>");
                    }

                    html.AppendLine("  </tbody>");
                    html.AppendLine("</table>");
                    html.AppendLine("</div>");
                    break;

                case JsonValueKind.Array:
                    var items = element.EnumerateArray().ToList();
                    html.AppendLine($"<div class=\"nested-array\">");
                    html.AppendLine($"<div class=\"key-label\">{System.Web.HttpUtility.HtmlEncode(title)} (Array - {items.Count} items)</div>");

                    // Eğer array elemanları hep aynı tipte object ise tablo olarak göster
                    if (items.Count > 0 && items.All(item => item.ValueKind == JsonValueKind.Object))
                    {
                        var allHeaders = new HashSet<string>();
                        foreach (var item in items)
                        {
                            foreach (var prop in item.EnumerateObject())
                            {
                                allHeaders.Add(prop.Name);
                            }
                        }

                        if (allHeaders.Count > 0)
                        {
                            var headerList = allHeaders.OrderBy(h => h).ToList();
                            html.AppendLine("<div class=\"table-wrapper\">");
                            html.AppendLine("<table >");
                            html.AppendLine("  <thead>");
                            html.AppendLine("    <tr>");

                            foreach (var header in headerList)
                            {
                                html.AppendLine($"      <th>{System.Web.HttpUtility.HtmlEncode(header)}</th>");
                            }

                            html.AppendLine("    </tr>");
                            html.AppendLine("  </thead>");
                            html.AppendLine("  <tbody>");

                            foreach (var item in items)
                            {
                                html.AppendLine("    <tr>");
                                foreach (var header in headerList)
                                {
                                    html.AppendLine("      <td>");
                                    if (item.TryGetProperty(header, out var prop))
                                    {
                                        if (prop.ValueKind == JsonValueKind.Object || prop.ValueKind == JsonValueKind.Array)
                                        {
                                            ConvertJsonElementToHTMLRecursive(prop, html, header, depth + 1);
                                        }
                                        else
                                        {
                                            html.AppendLine(FormatSimpleValue(prop));
                                        }
                                    }
                                    else
                                    {
                                        html.AppendLine("<span class=\"null-value\">-</span>");
                                    }
                                    html.AppendLine("      </td>");
                                }
                                html.AppendLine("    </tr>");
                            }

                            html.AppendLine("  </tbody>");
                            html.AppendLine("</table>");
                            html.AppendLine("</div>");
                        }
                    }
                    else
                    {
                        // Mixed array veya basit değerler için liste formatı
                        for (int i = 0; i < items.Count; i++)
                        {
                            html.AppendLine("<div class=\"array-item\">");
                            html.AppendLine($"<div class=\"key-label\">Item {i}</div>");

                            if (items[i].ValueKind == JsonValueKind.Object || items[i].ValueKind == JsonValueKind.Array)
                            {
                                ConvertJsonElementToHTMLRecursive(items[i], html, $"Item {i}", depth + 1);
                            }
                            else
                            {
                                html.AppendLine(FormatSimpleValue(items[i]));
                            }

                            html.AppendLine("</div>");
                        }
                    }

                    html.AppendLine("</div>");
                    break;

                default:
                    html.AppendLine(FormatSimpleValue(element));
                    break;
            }
        }

        private string FormatSimpleValue(JsonElement element)
        {
            var value = GetJsonElementValue(element);
            var encodedValue = System.Web.HttpUtility.HtmlEncode(value);

            return element.ValueKind switch
            {
                JsonValueKind.String => $"<span class=\"simple-value string-value\">\"{encodedValue}\"</span>",
                JsonValueKind.Number => $"<span class=\"simple-value number-value\">{encodedValue}</span>",
                JsonValueKind.True => $"<span class=\"simple-value boolean-value\">true</span>",
                JsonValueKind.False => $"<span class=\"simple-value boolean-value\">false</span>",
                JsonValueKind.Null => $"<span class=\"simple-value null-value\">null</span>",
                _ => $"<span class=\"simple-value\">{encodedValue}</span>"
            };
        }

        private void ConvertJsonElementToXML(JsonElement element, StringBuilder xml, int depth)
        {
            var indent = new string(' ', depth * 2);

            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    foreach (var prop in element.EnumerateObject())
                    {
                        xml.AppendLine($"{indent}<{SanitizeXmlElementName(prop.Name)}>");
                        ConvertJsonElementToXML(prop.Value, xml, depth + 1);
                        xml.AppendLine($"{indent}</{SanitizeXmlElementName(prop.Name)}>");
                    }
                    break;
                case JsonValueKind.Array:
                    foreach (var item in element.EnumerateArray())
                    {
                        xml.AppendLine($"{indent}<item>");
                        ConvertJsonElementToXML(item, xml, depth + 1);
                        xml.AppendLine($"{indent}</item>");
                    }
                    break;
                default:
                    xml.AppendLine($"{indent}{System.Security.SecurityElement.Escape(GetJsonElementValue(element))}");
                    break;
            }
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

        private string SanitizeXmlElementName(string name)
        {
            if (string.IsNullOrEmpty(name)) return "element";

            var sanitized = new StringBuilder();
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if ((i == 0 && (char.IsLetter(c) || c == '_')) ||
                    (i > 0 && (char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.')))
                {
                    sanitized.Append(c);
                }
                else
                {
                    sanitized.Append('_');
                }
            }
            return sanitized.ToString();
        }

        private string EscapeYamlKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return "\"\"";

            // Check if key needs quoting
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

            // Check if value needs quoting
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
                // Use double quotes and escape internal quotes
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

        private string[] ParseCSVLine(string line)
        {
            var result = new List<string>();
            var current = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // Skip next quote
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
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