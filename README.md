
# DevTools

**DevTools** is a web-based suite of developer utilities built with ASP.NET Core MVC. It includes a wide range of tools to simplify common developer tasks, all accessible via a clean and user-friendly interface.

---

## 🎯 Project Purpose

This project provides developers, testers, and engineers with a centralized set of essential tools to streamline daily operations such as text transformation, format conversion, encoding, generation, and debugging. Instead of relying on many scattered online tools, DevTools brings them all into a single local, secure, and customizable platform.

---

## ✨ Features

### 🧰 Tools Included

- **Base64 Converter** – Encode/decode text to and from Base64.
- **Case Converter** – Convert text to upper case, lower case, title case, etc.
- **Color Converter** – Convert between color formats like HEX, RGB.
- **Color Palette Generator** – Generate cohesive color schemes.
- **CSV to Converter** – Convert CSV data to JSON, XML, or other formats.
- **Diff Checker** – Compare two text blocks and highlight differences.
- **Duplicate Line Remover** – Eliminate repeated lines in a block of text.
- **GitIgnore Generator** – Generate .gitignore files for multiple languages/frameworks.
- **Hash Generator** – Create hashes (MD5, SHA-1, SHA-256, etc.) from text.
- **JSON/XML/YAML to...** – Convert data between different structured formats.
- **UUID Generator** – Create unique identifiers.
- **Word Counter** – Count words, characters, lines in text.
- **Password Generator** – Generate secure random passwords.
- **SMTP Tester** – Send test emails and verify SMTP configuration.
- **Lorem Ipsum Generator** – Generate placeholder text for design/mockups.
- **URL Converter** – Encode/decode URLs.
- **Markdown Previewer** – Convert Markdown to HTML.
- **QR Code Tools** – Generate and possibly decode QR codes.

---

## 🧱 Technologies Used

- **ASP.NET Core MVC**
- **C#**
- **Razor Views**
- **HTML/CSS/JavaScript**
- **Bootstrap (likely for UI)**
- **MVC Routing**

---

## 📁 Project Structure

```
DevTools/
├── Controllers/               # Logic for each tool
├── Views/                     # Razor views for UI
│   ├── ToolName/Index.cshtml  # Each tool has its own view
│   └── Shared/                # Layout and shared components
├── wwwroot/                   # Static files (JS, CSS, etc.)
├── Program.cs                 # Application entry point
├── DevTools.csproj            # Project file
└── DevTools.sln               # Solution file
```

---

## 🚀 Getting Started

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Cemililkim/DevTools.git
   cd DevTools
   ```

2. **Build and run the project:**
   ```bash
   dotnet build
   dotnet run
   ```

3. Visit `https://localhost:5001` or the specified port to access the tools.

---

## 👥 Contributing

Want to add more tools or improve UI/UX? Feel free to fork this repository and submit a pull request.

---

## 📄 License

This project is licensed under the MIT License.
