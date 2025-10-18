[![Build](https://github.com/AndrewClements84/TokenKit/actions/workflows/dotnet.yml/badge.svg?branch=master)](https://github.com/AndrewClements84/TokenKit/actions)
[![NuGet Version](https://img.shields.io/nuget/v/TokenKit.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/TokenKit/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TokenKit.svg?style=flat&logo=nuget)](https://www.nuget.org/packages/TokenKit/)
[![Codecov](https://codecov.io/gh/AndrewClements84/TokenKit/branch/master/graph/badge.svg?style=flat&logo=codecov&label=Coverage)](https://app.codecov.io/gh/AndrewClements84/TokenKit)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat)](https://opensource.org/licenses/MIT)

# 🧠 TokenKit

> **TokenKit** — A professional .NET 8.0 library and CLI for **tokenization**, **validation**, **cost estimation**, and **model registry management** across multiple LLM providers (OpenAI, Anthropic, Gemini, etc.).

---

## ✨ Features

| Category | Description |
|-----------|-------------|
| 🔢 **Tokenization** | Analyze text or files and count tokens using multiple encoder engines (`simple`, `SharpToken`, `ML.Tokenizers`) |
| 💰 **Cost Estimation** | Automatically calculate estimated API cost based on model metadata |
| ✅ **Prompt Validation** | Validate prompt length against model context limits |
| 🧩 **Model Registry** | Manage model metadata (`maxTokens`, pricing, encodings, providers) via JSON registry |
| ⚙️ **CLI & SDK** | Use TokenKit as a .NET library *or* a global CLI tool |
| 🧮 **Multi-Encoder Support** | Dynamically select tokenization engines via `--engine` flag |
| 📦 **Self-contained Data** | Local registry stored in `Registry/models.data.json`, auto-updatable |
| 🔍 **Live Model Scraper** | Optional OpenAI API key support to fetch real-time model data |
| 📊 **Structured Logging** | All CLI commands logged to `tokenkit.log` with rotation (1MB max) |
| 🤫 **Quiet & JSON Modes** | Machine-readable (`--json`) and silent (`--quiet`) output modes for automation |
| 🎨 **CLI Polish** | Colorized output, ASCII banner, and improved user experience |

---

## ⚙️ Installation

### 📦 As a Library (NuGet)
```bash
dotnet add package TokenKit
```

### 💻 As a Global CLI Tool
```bash
dotnet tool install -g TokenKit
```

---

## 🚀 Usage (All-in-One Guide)

### 🔹 Analyze Inline Text
```bash
tokenkit analyze "Hello from TokenKit!" --model gpt-4o
```

### 🔹 Analyze File Input
```bash
tokenkit analyze prompt.txt --model gpt-4o
```

### 🔹 Pipe Input (stdin)
```bash
echo "This is piped text input" | tokenkit analyze --model gpt-4o
```

**Example Output:**
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

### 🔹 Validate Prompt Length
```bash
tokenkit validate "A very long prompt to validate" --model gpt-4o
```
```json
{
  "IsValid": true,
  "Message": "OK"
}
```

---

### 🔹 List Registered Models
```bash
tokenkit models list
```

#### Filter by Provider
```bash
tokenkit models list --provider openai
```

#### JSON Output
```bash
tokenkit models list --json
```

---

### 🔹 Update Model Data

#### Default Update (Offline Fallback)
```bash
tokenkit update-models
```

#### Using OpenAI API Key
```bash
tokenkit update-models --openai-key sk-xxxx
```

#### From JSON (stdin)
```bash
cat newmodels.json | tokenkit update-models
```

**Example Input:**
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

### 🔹 Scrape Latest Model Data (Preview)
```bash
tokenkit scrape-models --openai-key sk-xxxx
```

If no key is provided, TokenKit uses the local offline model registry.

**Example Output:**
```
🔍 Fetching latest OpenAI model data...
✅ Retrieved 3 models:
  - OpenAI: gpt-4o (128000 tokens)
  - OpenAI: gpt-4o-mini (64000 tokens)
  - OpenAI: gpt-3.5-turbo (4096 tokens)
```

---

### 🔹 CLI Output Modes

#### JSON Mode
```bash
tokenkit analyze "Hello" --model gpt-4o --json
```
Outputs pure JSON:
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

#### Quiet Mode
```bash
tokenkit analyze "Silent test" --model gpt-4o --quiet
```
No console output. Log entry saved to `tokenkit.log`.

---

## 🧩 Programmatic SDK Example

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

## 📦 Model Registry

TokenKit stores all model metadata in:
```
Registry/models.data.json
```
Each entry includes:
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

## 🧪 Testing & Quality Assurance

TokenKit maintains **100% test coverage** using xUnit and Codecov.

Run tests locally:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

---

## 🧭 Future Enhancements

| Feature | Description |
|----------|-------------|
| 🌐 **Extended Provider Support** | Add Gemini, Claude, and Mistral integrations |
| 💾 **Persistent Config Profiles** | Store model defaults and pricing overrides per project |
| 🧮 **Batch Analysis** | Analyze multiple files or prompts in a single command |
| 📊 **Report Generation** | Export CSV/JSON summaries of token usage and estimated cost |
| 🧠 **LLM-Aware Cost Planner** | Simulate conversation cost across multi-turn dialogues |
| 🧩 **IDE Integrations** | VS Code and JetBrains plugins for inline token analysis |
| ⚙️ **Custom Encoders** | Support community-built encoders and language models |

---

## 💡 License

Licensed under the [MIT License](LICENSE).  
© 2025 Andrew Clements — Flow Labs / TokenKit
