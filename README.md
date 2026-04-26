# MrtOps

![.NET Version](https://img.shields.io/badge/.NET-10.0-blue.svg)
![License](https://img.shields.io/badge/License-MIT-green.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)

**MrtOps** is an enterprise-grade, hybrid CLI and WPF application designed to streamline the lifecycle management of Stimulsoft Report files (`.mrt`). 

Built with **Clean Architecture** principles and a focus on maintainability, MrtOps allows developers and report designers to scaffold new templates, execute batch modifications across hundreds of files, and enforce corporate styling with ease and safety.

## 🚀 Key Features

- **Template Scaffolding:** Generate pre-configured `.mrt` files from customizable JSON templates, ensuring standardized authors, units, and variables from day one.
- **Batch Processing:** Inject variables or categories across entire directories of existing reports in seconds.
- **Style Synchronization:** Enforce corporate identity by applying a single Stimulsoft Style file (`.sts`) to an entire batch of reports.
- **Advanced Operations Engine:**
    - **Dry-Run Mode:** Safely simulate massive batch operations to see exactly what would change without touching any files on disk.
    - **Stateful Rollback:** Every modification creates an automatic backup. Use the `undo` command to instantly revert operations if something goes wrong.
- **Database Scanner:** Scan SQL Server instances to list available databases and automatically generate optimized connection strings.
- **Hybrid Interface:** Functions natively as a high-performance command-line tool for automation and as a WPF desktop application for interactive workflows.
- **Full Localization (i18n):** Automatically adapts console messages and UI text to the host system's language (English and Italian supported).

## 🏗 Project Structure

The project follows **Clean Architecture** to ensure total decoupling between business logic and external frameworks:

```text
MrtOps/
├── Core/             # Domain Logic: Models, Interfaces, and Operation Engine
├── Application/      # Use Cases: Batch services and orchestration
├── Infrastructure/   # Implementation: Stimulsoft wrappers, SQL connectors, Storage
└── Presentation/     # UI Layer: CLI (Spectre.Console) and WPF
```

## 🛠 Installation

MrtOps can be installed as a .NET Global Tool. From the project root, run:

```bash
dotnet pack -c Release
dotnet tool install --global --add-source ./src/MrtOps/nupkg MrtOps
```

## 📖 CLI Usage Examples

Below are the primary commands available in the MrtOps CLI:

```bash
# Generate a new report using a template
mrtops gen "C:\Reports\NewReport.mrt" --name "InvoiceReport" --template "Standard_Template"

# Batch add a variable to a folder (Dry-Run and Real execution)
mrtops batch "C:\Reports\Sales" --add-var "Environment" --dry-run
mrtops batch "C:\Reports\Sales" --add-var "Environment"

# Synchronize corporate styles across a directory
mrtops sync-style "C:\Reports\Work" "C:\Styles\Default.sts"

# Scan SQL Server for available databases and connection strings
mrtops db-scan --server "localhost\SQLEXPRESS"

# Rollback the last operation
mrtops undo
```

## 🗺 Roadmap

- [ ] Complete WPF Graphical User Interface.
- [ ] Implement localized string dictionary synchronization.
- [ ] Code Generation module for C# component wrappers.
- [ ] Automated CI/CD pipeline for GitHub Actions.

## ⚖️ License & Legal Disclaimer

This project is released under the **MIT License**.

**Legal Disclaimer:** MrtOps utilizes the `Stimulsoft.Reports.Engine.NetCore` library to manipulate `.mrt` files. Stimulsoft is a commercial, closed-source product. Using this tool does not grant you a license to use Stimulsoft products. A valid license from [Stimulsoft](https://www.stimulsoft.com/) is required for commercial use.
