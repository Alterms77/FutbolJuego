# ⚽ FutbolJuego

![Unity](https://img.shields.io/badge/Unity-2022.3_LTS-black?logo=unity)
![Firebase](https://img.shields.io/badge/Firebase-Firestore%20%7C%20Functions-orange?logo=firebase)
![C#](https://img.shields.io/badge/C%23-9.0-purple?logo=csharp)
![Platform](https://img.shields.io/badge/Platform-iOS%20%7C%20Android-blue)

A professional mobile Football / Soccer Manager game featuring Poisson-based match simulation, Monte Carlo match prediction, Firebase backend, and a full live-service economy.

## Tech Stack
- **Engine:** Unity 2022.3 LTS (C# 9 / .NET Standard 2.1)
- **Backend:** Firebase Auth · Firestore · Cloud Functions (Node 18)
- **Architecture:** Service Locator · Event Bus · State Machine · Object Pool

## Quick Start
```bash
# 1. Clone
git clone https://github.com/your-org/FutbolJuego.git

# 2. Open in Unity 2022.3 LTS

# 3. Add Firebase config files
#    google-services.json  → Assets/
#    GoogleService-Info.plist → Assets/

# 4. Deploy backend
cd Backend/functions && npm install
firebase deploy --only firestore:rules,functions
```

## Architecture
See [Docs/ARCHITECTURE.md](Docs/ARCHITECTURE.md) for full system diagram.

| Layer | Namespace | Purpose |
|-------|-----------|---------|
| Core | `FutbolJuego.Core` | GameManager, SaveSystem, EventBus |
| Models | `FutbolJuego.Models` | Data objects (serialisable) |
| Systems | `FutbolJuego.Systems` | Simulation, economy, transfers |
| UI | `FutbolJuego.UI` | MonoBehaviour view controllers |
| AI | `FutbolJuego.AI` | Opponent decision-making |
| Utils | `FutbolJuego.Utils` | Constants, extensions, pooling |

## Key Features
- ⚽ **Poisson xG simulation** with home advantage, morale, fatigue
- 🔮 **Monte Carlo predictor** — 1,000-run win/draw/loss probability
- 💰 **Full economy** — wages, revenue, infrastructure upgrades
- 🤖 **AI opponents** — 4 difficulty tiers with tactical adaptation
- ☁️ **Firebase backend** — cloud saves, leaderboards, live events
- 🏆 **Live service** — daily rewards, weekly challenges

## Contributing
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Commit with descriptive messages
4. Open a Pull Request targeting `main`

## Docs
- [Architecture](Docs/ARCHITECTURE.md)
- [Game Design Document](Docs/GAME_DESIGN_DOCUMENT.md)
- [Database Schema](Docs/DATABASE_SCHEMA.md)
- [Roadmap](Docs/ROADMAP.md)
- [Monetisation](Docs/MONETIZATION_STRATEGY.md)
- [Publishing Guide](Docs/PUBLISHING_GUIDE.md)

## License
MIT © FutbolJuego Team

Full project structure and implementation coming soon.