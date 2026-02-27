# ♟️ Chaalbaaz

> **AI-powered chess move prediction and real-time coaching assistant**

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/)
[![Python](https://img.shields.io/badge/Python-3.11+-yellow.svg)](https://python.org)
[![Azure](https://img.shields.io/badge/Cloud-Azure-0078D4.svg)](https://azure.microsoft.com)
[![Status](https://img.shields.io/badge/Status-In_Development-orange.svg)]()

---

## 🧠 What is Chaalbaaz?

**Chaalbaaz** (Hindi: *चालबाज* — "the tactician") is an open-source AI chess assistant that monitors your live Chess.com game and suggests your next best move in real time.

It's not just a Stockfish wrapper — it learns **your** playing style, identifies **your** weaknesses, and gives you suggestions that are personalised to **your** level and patterns.

Think of it as a **grandmaster coach watching over your shoulder** — for practice sessions.

---

## ✨ Features

- 🔴 **Live Game Monitoring** — connects to Chess.com live game feed
- 🤖 **AI Move Suggestions** — real-time best move recommendations
- 🧬 **Personalised Analysis** — learns from your game history
- 📊 **Pattern Recognition** — identifies your recurring weaknesses
- 🌐 **Cloud-backed** — powered by Microsoft Azure
- 📱 **Cross-platform** — web app + browser extension

---

## 🏗️ Architecture

```
Chess.com Live Game
        ↓
  Chess.com Public API
        ↓
  Chaalbaaz Backend (.NET 8)
        ↓
  AI Engine (Python + Stockfish)
        ↓
  SignalR WebSocket
        ↓
  React Frontend / Browser Extension
```

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend API | .NET 8 (C#) |
| AI / ML Engine | Python 3.11, Stockfish |
| Frontend | React + TypeScript |
| Real-time | SignalR (WebSockets) |
| Cloud | Microsoft Azure |
| Infrastructure | Terraform |
| CI/CD | GitHub Actions |

---

## 🚀 Getting Started

> ⚙️ **Work in Progress** — setup instructions will be added as the project develops.

```bash
# Clone the repo
git clone https://github.com/anandmaurya456/chaalbaaz.git
cd chaalbaaz
```

---

## ⚠️ Legal Disclaimer & Fair Play Notice

> **Please read this carefully before using Chaalbaaz.**

### Intended Use
Chaalbaaz is designed **exclusively** as a **learning and practice tool**. It is intended to help chess players improve their skills through analysis, pattern recognition, and move explanation.

### ❌ Prohibited Use in Rated Games
Using Chaalbaaz or any AI assistance tool during **rated, competitive, or tournament games** on Chess.com or any other platform is:

- A direct violation of [Chess.com's Fair Play Policy](https://www.chess.com/legal/user-agreement)
- Considered cheating by all major chess federations (FIDE, etc.)
- Grounds for permanent account bans on chess platforms

**The creators of Chaalbaaz take no responsibility for account bans, penalties, or legal consequences arising from misuse of this application.**

### ✅ Responsible Use
- Use Chaalbaaz only for **unrated / practice games**
- Use it for **post-game analysis and review**
- Use it as an **educational tool** to understand better moves

### Chess.com Affiliation
Chaalbaaz is an independent open-source project. It is **not affiliated with, endorsed by, or officially connected to Chess.com** in any way. We use only Chess.com's publicly available API in compliance with their terms.

---

## 📄 Legal Documents

- [Privacy Policy](./PRIVACY_POLICY.md)
- [Terms of Use](./TERMS_OF_USE.md)
- [License (Apache 2.0)](./LICENSE)

---

## 🤝 Contributing

Contributions are welcome! Please read our contributing guidelines (coming soon) and open a pull request.

---

## 📬 Contact

- **GitHub Issues:** [https://github.com/anandmaurya456/chaalbaaz/issues](https://github.com/anandmaurya456/chaalbaaz/issues)
- **Author:** [@anandmaurya456](https://github.com/anandmaurya456)

---

*Chaalbaaz is built with ♟️ and ☕ — for chess lovers, by chess lovers.*
