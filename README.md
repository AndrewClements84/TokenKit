[![Build](https://github.com/AndrewClements84/TokenKit/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/AndrewClements84/TokenKit/actions)
[![NuGet Version](https://img.shields.io/nuget/v/TokenKit.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/TokenKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TokenKit.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/TokenKit/)
[![Codecov](https://codecov.io/gh/AndrewClements84/TokenKit/branch/master/graph/badge.svg?style=flat&logo=codecov&label=Coverage)](https://app.codecov.io/gh/AndrewClements84/TokenKit)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat)](https://opensource.org/licenses/MIT)


# 🧠 TokenKit

> **TokenKit** — A lightweight .NET 8.0 library and CLI tool for unified **tokenization**, **validation**, and **model registry management** across multiple LLM providers (OpenAI, Anthropic, Gemini, etc.)

---

## ✨ Features

| Category | Description |
|-----------|-------------|
| 🔢 **Tokenization** | Analyze text or files and count tokens using provider-specific encodings |
| 💰 **Cost Estimation** | Automatically calculate estimated API cost based on token usage |
| ✅ **Prompt Validation** | Validate that prompts fit within model context limits |
| 🔄 **Model Registry** | Maintain up-to-date model metadata (`maxTokens`, pricing, encodings, etc.) |
| 🧩 **CLI & SDK** | Use TokenKit as a .NET library *or* a standalone global CLI |
| 📦 **Self-contained** | All data stored in `Registry/models.data.json`, auto-updated via command |
| 🌐 **Optional Live Scraper** | Fetch the latest OpenAI model data using an API key, or use trusted fallback data |

---

## ⚙️ Installation

### 📦 NuGet (Library use)
```bash
dotnet add package TokenKit
```

### 🧰 Global CLI Tool
```bash
dotnet tool install -g TokenKit
```

---

## 🚀 Quick Start (CLI)

Analyze, validate, or update model data directly from your terminal.

### 1️⃣ Analyze Inline Text
```bash
tokenkit analyze "Hello from TokenKit!" --model gpt-4o
```

**Output:**
```json
{
  "Model": "gpt-4o",
  "Provider": "OpenAI",
  "TokenCount": 4,
  "EstimatedCost": 0.00002,
  "Valid": true
}
```

---

### 2️⃣ Analyze File Input
```bash
tokenkit analyze prompt.txt --model gpt-4o
```

---

### 3️⃣ Pipe Input (stdin)
```bash
echo "This is piped text input" | tokenkit analyze --model gpt-4o
```

---

### 4️⃣ Validate Prompt
```bash
tokenkit validate "A very long prompt to validate" --model gpt-4o
```

**Output:**
```json
{
  "IsValid": true,
  "Message": "OK"
}
```

---

### 5️⃣ Update Model Data

#### Default Update (No API Key)
Fetch the latest built-in model metadata:
```bash
tokenkit update-models
```

#### Update Using OpenAI API Key
Use your OpenAI key to fetch live model data from the `/v1/models` endpoint:
```bash
tokenkit update-models --openai-key sk-xxxx
```

#### Update from JSON (stdin)
Pipe a JSON file with model specs:
```bash
cat newmodels.json | tokenkit update-models
```

**Example `newmodels.json`:**
```json
[
  {
    "Id": "gpt-4o-mini",
    "Provider": "OpenAI",
    "MaxTokens": 64000,
    "InputPricePer1K": 0.002,
    "OutputPricePer1K": 0.01,
    "Encoding": "cl100k_base"
  }
]
```

---

### 6️⃣ Scrape Model Data (Preview Only)

Fetch latest OpenAI model data (does not overwrite your registry):
```bash
tokenkit scrape-models --openai-key sk-xxxx
```

If no key is provided, TokenKit falls back to its offline model list.

**Example Output:**
```
🔍 Fetching latest OpenAI model data...
✅ Retrieved 3 models:
  - OpenAI: gpt-4o (128000 tokens)
  - OpenAI: gpt-4o-mini (64000 tokens)
  - OpenAI: gpt-3.5-turbo (4096 tokens)
```

---

## 🧩 Model Registry

TokenKit stores all known model information in:

```
src/TokenKit/Registry/models.data.json
```

Each entry contains:
```json
{
  "Id": "gpt-4o",
  "Provider": "OpenAI",
  "MaxTokens": 128000,
  "InputPricePer1K": 0.005,
  "OutputPricePer1K": 0.015,
  "Encoding": "cl100k_base"
}
```

---

## 🧮 CLI Command Reference

| Command | Description |
|----------|--------------|
| `tokenkit analyze "<text | path>" --model <model-id>` | Analyze and count tokens for inline text, file, or stdin input |
| `tokenkit validate "<text | path>" --model <model-id>` | Validate prompt against model token limits |
| `tokenkit update-models` | Update local registry using default fallback data |
| `tokenkit update-models --openai-key <key>` | Update registry using OpenAI API (requires valid key) |
| `cat newmodels.json | tokenkit update-models` | Update registry from piped JSON input |
| `tokenkit scrape-models [--openai-key <key>]` | Fetch and preview OpenAI model data without saving |
| `tokenkit --help` | Display CLI usage guide |

---

## 🧠 Programmatic Use (SDK)

```csharp
using TokenKit.Registry;
using TokenKit.Services;

var model = ModelRegistry.Get("gpt-4o");
var tokenizer = new TokenizerService();

var result = tokenizer.Analyze("Hello from TokenKit!", model!.Id);
var cost = CostEstimator.Estimate(model, result.TokenCount);

Console.WriteLine($"Tokens: {result.TokenCount}, Cost: ${cost}");
```

---

## 🧪 Testing

TokenKit includes an **xUnit** test suite with coverage for tokenization, cost estimation, and registry loading.

```bash
dotnet test
```

---

## 🛠 Project Structure

```
TokenKit/
├── src/
│   └── TokenKit/
│       ├── Models/
│       ├── Services/
│       ├── Registry/
│       ├── CLI/
│       └── Program.cs
└── tests/
    └── TokenKit.Tests/
```

---


---

## ⚙️ Phase 6 Additions (Advanced Tokenization & CLI UX)

| Feature | Description |
|----------|-------------|
| 🧩 **Multi-Encoder Support** | TokenKit now supports multiple tokenization engines via the `--engine` flag (`simple`, `sharptoken`, `mltokenizers`). |
| ⚙️ **CLI Runtime Switching** | Analyze or validate text using any supported encoder on demand. |
| 📦 **`models list` Command** | View all registered models (provider, ID, token limits, pricing) in a clean tabular view. |
| 🔍 **Provider Filtering** | Use `tokenkit models list --provider OpenAI` to filter models by provider (case-insensitive). |
| 🧪 **Multi-Engine Tests** | Added xUnit tests verifying token count consistency across encoders. |
| ⚠️ **Disclaimer** | TokenKit provides cost estimates and token counts based on available model data. The author is **not responsible** for legacy, outdated, or provider-changed calculation differences. |

### 🧠 Example Usage

#### List all models
```bash
tokenkit models list
```

#### Filter by provider (case-insensitive)
```bash
tokenkit models list --provider openai
tokenkit models list --provider Anthropic
```

#### Analyze with a specific encoder
```bash
tokenkit analyze "Hello from TokenKit" --model gpt-4o --engine sharptoken
```

---

## 🗺️ Roadmap

- [x] Tokenization, cost, and validation services  
- [x] CLI for `analyze`, `validate`, and `update-models`  
- [x] Stdin + file + inline input support  
- [x] Model registry auto-load and safe paths  
- [x] Live model scraping from OpenAI (optional API key)  
- [ ] Add `tokenkit models list` command  
- [ ] Optional SharpToken / Microsoft.ML.Tokenizers integration  
- [ ] Publish stable v1.0.0 to NuGet + dotnet tool feed  

---

## 💡 License

Licensed under the [MIT License](LICENSE).  
© 2025 Andrew Clements


---

## 🎨 Phase 8 — CLI Polish, Logging & Automation Support

| Feature | Description |
|----------|-------------|
| 🧾 **Colorized Output** | All CLI commands now use `ConsoleStyler` for clear, color-coded feedback (green ✅, yellow ⚠️, red ❌). |
| 🤫 **Quiet Mode (`--quiet`)** | Suppresses console output while still writing structured logs to `tokenkit.log`. Ideal for CI/CD pipelines. |
| ⚙️ **Structured Logging** | Every operation is logged with timestamps and severity in `tokenkit.log` (auto-rotating, max 1MB). |
| 🧩 **JSON Mode (`--json`)** | Outputs raw JSON (no colors or emojis) for automation and machine-readable workflows. |
| 🧠 **ASCII Banner** | TokenKit now includes a startup banner and version info header for professional CLI presentation. |
| 🧪 **Enhanced Tests** | Coverage expanded to include encoders, CLI output modes, and logging behavior. |

---

## 🧪 Extended CLI Examples

### 🔹 Standard Analysis
```bash
tokenkit analyze "Hello from TokenKit!" --model gpt-4o
```
✅ Produces colorized JSON summary + log entry.

### 🔹 JSON Mode (Automation / CI)
```bash
tokenkit analyze "Hello world" --model gpt-4o --json
```
Outputs pure JSON only, suppressing banner and emojis:
```json
{
  "Model": "gpt-4o",
  "Provider": "OpenAI",
  "TokenCount": 7,
  "EstimatedCost": 0.000105,
  "Engine": "simple",
  "Valid": true
}
```

### 🔹 Quiet Mode (Log Only)
```bash
tokenkit analyze "Silent test" --model gpt-4o --quiet
```
No console output. Log file receives entries like:
```
2025-10-17 22:43:15 [INFO] Analyze started with model=gpt-4o
2025-10-17 22:43:15 [SUCCESS] Analyzed 7 tokens using simple (gpt-4o)
```

### 🔹 Model Listing with JSON
```bash
tokenkit models list --json
```

---

## 📜 Logs

All CLI runs write to `tokenkit.log` (auto-rotated at 1 MB).  
You can find it under your TokenKit working directory, e.g.:
```
src/TokenKit/bin/Debug/net8.0/tokenkit.log
```

---

## 📈 Code Coverage

TokenKit targets **100% test coverage** with xUnit and Codecov integration.  
Run coverage locally:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

View detailed results in Codecov:  
[![Codecov](https://codecov.io/gh/AndrewClements84/TokenKit/branch/master/graph/badge.svg)](https://app.codecov.io/gh/AndrewClements84/TokenKit)

---

## 🗺️ Updated Roadmap (as of 2025-10-17)

| Phase | Feature | Status |
|-------|----------|---------|
| 1 | Core tokenization + cost estimation | ✅ Done |
| 2 | Validation logic | ✅ Done |
| 3 | Model registry (JSON-based) | ✅ Done |
| 4 | CLI commands (`analyze`, `validate`, `update-models`) | ✅ Done |
| 5 | Scraper service (OpenAI API optional) | ✅ Done |
| 6 | Advanced encoders (`SharpToken`, `ML.Tokenizers`) | ✅ Done |
| 7 | Tests + Codecov integration | ✅ Done |
| 8 | CLI polish (`--json`, `--quiet`, logging, banner) | ✅ Done |
| 9 | NuGet + global CLI release (v1.0.0) | 🔄 Pending Release |

---

© 2025 Andrew Clements — MIT License  
Flow Labs / TokenKit — https://github.com/AndrewClements84/TokenKit
