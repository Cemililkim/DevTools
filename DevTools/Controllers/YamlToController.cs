using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DevTools.Controllers
{
    public class YamlToController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConvertToJSON(string yamlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yamlInput))
                {
                    return Json(new { success = false, error = "YAML data cannot be empty." });
                }

                var json = ConvertYAMLToJson(yamlInput);
                return Json(new { success = true, result = json });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToXML(string yamlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yamlInput))
                {
                    return Json(new { success = false, error = "YAML data cannot be empty." });
                }

                var json = ConvertYAMLToJson(yamlInput);
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
        public IActionResult ConvertToCSV(string yamlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yamlInput))
                {
                    return Json(new { success = false, error = "YAML data cannot be empty." });
                }

                var csv = ConvertYAMLToCSV(yamlInput);
                return Json(new { success = true, result = csv });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult ConvertToHTML(string yamlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yamlInput))
                {
                    return Json(new { success = false, error = "YAML data cannot be empty." });
                }

                var html = ConvertYAMLToHTML(yamlInput);
                return Json(new { success = true, result = html });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Conversion error: {ex.Message}" });
            }
        }

        [HttpPost]
        public IActionResult FormatYAML(string yamlInput)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(yamlInput))
                {
                    return Json(new { success = false, error = "YAML data cannot be empty." });
                }

                var formatted = FormatYAMLData(yamlInput);
                return Json(new { success = true, result = formatted });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = $"Formatting error: {ex.Message}" });
            }
        }

        private string ConvertYAMLToJson(string yamlInput)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlObject = deserializer.Deserialize(yamlInput);

            return JsonSerializer.Serialize(yamlObject, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
        }

        private string ConvertYAMLToCSV(string yamlInput)
        {
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlInput);

            if (yamlObject is IList<object> list && list.Count > 0)
            {
                var csv = new StringBuilder();
                var firstItem = list[0];

                if (firstItem is IDictionary<object, object> firstDict)
                {
                    // Write headers
                    var headers = firstDict.Keys.Select(k => k.ToString()).ToArray();
                    csv.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

                    // Write data rows
                    foreach (var item in list)
                    {
                        if (item is IDictionary<object, object> dict)
                        {
                            var values = headers.Select(h => dict.ContainsKey(h) ? dict[h]?.ToString() ?? "" : "").ToArray();
                            csv.AppendLine(string.Join(",", values.Select(EscapeCsvValue)));
                        }
                    }
                }

                return csv.ToString().TrimEnd();
            }

            throw new ArgumentException("YAML must contain an array of objects to convert to CSV");
        }

        private string ConvertYAMLToHTML(string yamlInput)
        {
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlInput);

            if (yamlObject is IList<object> list && list.Count > 0)
            {
                var html = new StringBuilder();
                var firstItem = list[0];

                if (firstItem is IDictionary<object, object> firstDict)
                {
                    var headers = firstDict.Keys.Select(k => k.ToString()).ToArray();

                    html.AppendLine("<div class=\"yaml-table-container\">");
                    html.AppendLine("<table class=\"yaml-table\">");
                    html.AppendLine("  <thead>");
                    html.AppendLine("    <tr>");

                    foreach (var header in headers)
                    {
                        html.AppendLine($"      <th>{System.Web.HttpUtility.HtmlEncode(header)}</th>");
                    }

                    html.AppendLine("    </tr>");
                    html.AppendLine("  </thead>");
                    html.AppendLine("  <tbody>");

                    foreach (var item in list)
                    {
                        if (item is IDictionary<object, object> dict)
                        {
                            html.AppendLine("    <tr>");
                            foreach (var header in headers)
                            {
                                var value = dict.ContainsKey(header) ? dict[header]?.ToString() ?? "" : "";
                                html.AppendLine($"      <td>{System.Web.HttpUtility.HtmlEncode(value)}</td>");
                            }
                            html.AppendLine("    </tr>");
                        }
                    }

                    html.AppendLine("  </tbody>");
                    html.AppendLine("</table>");
                    html.AppendLine("</div>");

                    return html.ToString();
                }
            }

            throw new ArgumentException("YAML must contain an array of objects to convert to HTML table");
        }

        private string FormatYAMLData(string yamlInput)
        {
            var deserializer = new DeserializerBuilder().Build();
            var yamlObject = deserializer.Deserialize(yamlInput);

            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            return serializer.Serialize(yamlObject);
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
    }
}