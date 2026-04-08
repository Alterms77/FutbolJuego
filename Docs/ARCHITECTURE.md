# FutbolJuego – Architecture

## System Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         Unity Client                         │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────────┐  │
│  │   Core   │  │  Models  │  │ Systems  │  │     UI     │  │
│  │GameMgr   │  │PlayerData│  │MatchSim  │  │MainMenuUI  │  │
│  │StateMach │  │TeamData  │  │Tactical  │  │SquadUI     │  │
│  │SaveSys   │  │LeagueData│  │Economy   │  │TacticsUI   │  │
│  │EventBus  │  │MatchData │  │Transfer  │  │MatchDayUI  │  │
│  │SvcLocatr │  │FinanceData  │Training  │  │FinancesUI  │  │
│  └──────────┘  └──────────┘  │Injury    │  │TrainingUI  │  │
│                               │Morale    │  │LeagueUI    │  │
│  ┌──────────┐  ┌──────────┐  │Competit. │  │SettingsUI  │  │
│  │    AI    │  │   Data   │  │Predict.  │  └────────────┘  │
│  │AIManager │  │DataLoader│  │LiveEvent │                   │
│  │AITactical│  │JsonHandlr│  │Progress. │                   │
│  └──────────┘  └──────────┘  └──────────┘                   │
│                    │                                          │
│         ┌──────────┘                                          │
│         │ Resources/Data/*.json                               │
└─────────┼───────────────────────────────────────────────────┘
          │ HTTPS (Firebase SDK)
┌─────────▼───────────────────────────────────────────────────┐
│                     Firebase Backend                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────┐  │
│  │Cloud Functions│  │  Firestore   │  │  Firebase Auth   │  │
│  │saveGameState  │  │/users/{uid}  │  │  Email/Google    │  │
│  │loadGameState  │  │/leaderboards │  │  onUserCreated   │  │
│  │getLeaderboard │  │/liveEvents   │  │  trigger         │  │
│  │claimDailyRew. │  │/matches      │  └──────────────────┘  │
│  │validateMatch  │  │/config       │                         │
│  │generateChall. │  └──────────────┘                         │
│  └──────────────┘                                            │
└─────────────────────────────────────────────────────────────┘
```

## Layer Descriptions

### Core
`GameManager` (singleton MonoBehaviour) owns the game lifecycle. It boots all
systems via `ServiceLocator`, drives the `GameStateMachine`, and exposes the
`OnStateChanged` event consumed by UI panels.

### Models
Plain serialisable C# data objects (no Unity dependencies, safe for JSON round-
trips). `PlayerData`, `TeamData`, `LeagueData`, `MatchData`, `TacticData`,
`FinanceData`, `ClubInfrastructure`.

### Systems
Stateless service classes registered in `ServiceLocator`:
- **MatchSimulationEngine** – Poisson-based match simulation with full xG math.
- **TacticalSystem** – Formation positions, formation matchup table, AI tactic generation.
- **EconomySystem** – Revenue, wages, prize money, infrastructure upgrades.
- **TransferMarketSystem** – Valuation, negotiation, procedural player generation.
- **TrainingSystem** – Attribute gains, fatigue accumulation and recovery.
- **InjurySystem** – Probability rolls, severity, daily recovery.
- **MoraleSystem** – Match-result morale deltas, manager interaction events.
- **CompetitionSystem** – Round-robin fixture generation, table updates, cup brackets.
- **ProgressionSystem** – Manager XP, levels, feature unlocks.
- **MatchPredictorSystem** – Monte Carlo simulation (1 000 runs) for win/draw/loss %.
- **LiveEventSystem** – Daily rewards, weekly challenges, active events.

### UI
MonoBehaviour controllers wired to Unity Canvas buttons. Subscribe to
`GameManager.OnStateChanged` to show/hide panels. Use `ServiceLocator.Get<T>()`
to call system APIs.

### AI
`AIManager` orchestrates tactical, substitution, and transfer decisions for
computer clubs. `AITacticalEngine` handles in-match adaptation (going behind →
press higher; winning → park the bus).

### Backend (Firebase)
Cloud Functions enforce server-side trust: score validation, daily-reward
deduplication, leaderboard writes. Firestore security rules restrict document
access to owning users; leaderboards are public-read only.

## Event Bus Pattern
```
EventBus.Subscribe<MatchCompletedEvent>(OnMatchCompleted);
EventBus.Publish(new MatchCompletedEvent { match = result });
EventBus.Unsubscribe<MatchCompletedEvent>(OnMatchCompleted);
```

## Service Locator Usage
```csharp
// Registration (GameManager.InitializeSystems)
ServiceLocator.Register<MatchSimulationEngine>(new MatchSimulationEngine());

// Retrieval (anywhere)
var engine = ServiceLocator.Get<MatchSimulationEngine>();
```

## Save System Architecture
```
Local:  Application.persistentDataPath/save.json  (AES-encrypted JSON)
Cloud:  Firestore users/{uid}/gameState/data       (via saveGameState function)
Auto:   Every 5 minutes + on application pause
```
