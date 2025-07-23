using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using System.Globalization;

namespace DevTools.Controllers
{
    public class CsvToController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConvertToJSON(string csvInput)
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
        public IActionResult ConvertToXML(string csvInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvInput))
                {
                    return Json(new { success = false, error = "CSV data cannot be empty." });
                }

                var json = ConvertCSVToJson(csvInput);
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
                var xml = ConvertJsonToXML(jsonElement);

                return Json(new { success = true, result = xml });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToYAML(string csvInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvInput))
                {
                    return Json(new { success = false, error = "CSV data cannot be empty." });
                }

                var json = ConvertCSVToJson(csvInput);
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
        public IActionResult ConvertToHTML(string csvInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvInput))
                {
                    return Json(new { success = false, error = "CSV data cannot be empty." });
                }

                var html = ConvertCSVToHTML(csvInput);
                return Json(new { success = true, result = html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult FormatCSV(string csvInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(csvInput))
                {
                    return Json(new { success = false, error = "CSV data cannot be empty." });
                }

                var formatted = FormatCSVData(csvInput);
                return Json(new { success = true, result = formatted });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Formatting error: {ex.Message}" });
            }
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

        private string ConvertCSVToHTML(string csvInput)
        {
            var lines = csvInput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length < 1) throw new ArgumentException("CSV must contain at least 1 line");

            var html = new StringBuilder();
            var headers = ParseCSVLine(lines[0]);

            html.AppendLine("<div class=\"csv-table-container\">");
            html.AppendLine("<table class=\"csv-table\">");
            html.AppendLine("  <thead>");
            html.AppendLine("    <tr>");

            foreach (var header in headers)
            {
                html.AppendLine($"      <th>{System.Web.HttpUtility.HtmlEncode(header)}</th>");
            }

            html.AppendLine("    </tr>");
            html.AppendLine("  </thead>");
            html.AppendLine("  <tbody>");

            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCSVLine(lines[i]);
                html.AppendLine("    <tr>");

                for (int j = 0; j < headers.Length; j++)
                {
                    var value = j < values.Length ? values[j] : "";
                    html.AppendLine($"      <td>{System.Web.HttpUtility.HtmlEncode(value)}</td>");
                }

                html.AppendLine("    </tr>");
            }

            html.AppendLine("  </tbody>");
            html.AppendLine("</table>");
            html.AppendLine("</div>");

            return html.ToString();
        }

        private string FormatCSVData(string csvInput)
        {
            var lines = csvInput.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var formatted = new StringBuilder();

            foreach (var line in lines)
            {
                var values = ParseCSVLine(line);
                var formattedValues = values.Select(v => EscapeCsvValue(v.Trim()));
                formatted.AppendLine(string.Join(",", formattedValues));
            }

            return formatted.ToString().TrimEnd();
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
                        i++;
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