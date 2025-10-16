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
tokenkit analyze prompt.txt --model claude-3
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

#### Default Update
Fetch the latest built-in model metadata:
```bash
tokenkit update-models
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
| `tokenkit update-models` | Update local registry using default source |
| `cat newmodels.json | tokenkit update-models` | Update registry from piped JSON |
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

## 🗺️ Roadmap

- [x] Tokenization, cost, and validation services  
- [x] CLI for `analyze`, `validate`, and `update-models`  
- [x] Stdin + file + inline input support  
- [x] Model registry auto-load and safe paths  
- [ ] Live model scraping from OpenAI / Anthropic / Gemini APIs  
- [ ] Add `tokenkit models list` command  
- [ ] Optional SharpToken / Microsoft.ML.Tokenizers integration  
- [ ] Publish stable v1.0.0 to NuGet + dotnet tool feed  

---

## 💡 License

Licensed under the [MIT License](LICENSE).  
© 2025 Andrew Clements
