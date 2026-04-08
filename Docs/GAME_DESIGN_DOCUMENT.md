# FutbolJuego – Game Design Document

## Executive Summary
FutbolJuego is a mobile Football/Soccer Manager game for iOS and Android.
Players take charge of a club, manage tactics, sign players, develop youth,
and compete across league and cup competitions. The core differentiator is
real-time match prediction powered by Poisson-based Monte Carlo simulation,
giving managers data-driven insight before every kick-off.

## Core Gameplay Loop
```
Weekly Cycle:
  Monday–Friday : Training sessions, Transfer window, Infrastructure upgrades
  Saturday      : Match Day – tactics setup → simulate → result
  Sunday        : Finance week processed, morale updated, injuries healed
```

## 12 Game Systems

1. **Match Simulation** – Poisson xG model with home advantage, morale, fatigue modifiers.
2. **Tactics** – 7 formations, 5 play styles, pressing/tempo/width/line sliders.
3. **Squad Management** – 25-man squads, position roles, morale & fatigue tracking.
4. **Transfer Market** – Valuation curve, negotiation, procedural player generation.
5. **Training** – Six focus areas, facility quality multiplier, age-based gain curve.
6. **Injuries** – Probabilistic rolls, four severity tiers, medical centre recovery.
7. **Morale** – Team & individual, driven by results and manager interactions.
8. **Finances** – Match-day, sponsor, prize money; wage bill; infrastructure costs.
9. **Competition** – Round-robin leagues, single-elimination cups, promotion/relegation.
10. **AI Opponents** – Four difficulty tiers, tactical adaptation, transfer logic.
11. **Progression** – Manager XP, 20 levels, feature unlocks, achievements.
12. **Live Service** – Daily login rewards (7-day cycle), weekly challenges, live events.

## Player Progression
- Manager XP earned from match results, achievements, and challenges.
- Levels 1-20 unlock features: Transfer Market (L2), Prediction Tool (L7), Continental (L10).
- Achievement system with 50+ milestones.

## Monetization
- **FutCoins** (premium currency): cosmetic kits, skip timers, extra scout slots.
- **Season Pass** (monthly): daily premium reward boost, exclusive kit, bonus FutCoins.
- **Rewarded Ads**: optional double-XP for a match, recover a fatigue point.
- Non-P2W: all simulation-affecting content earnable through gameplay.

## Technical Requirements
- Unity 2022.3 LTS, C# 9, .NET Standard 2.1
- Firebase Auth, Firestore, Cloud Functions (Node 18)
- Target: iOS 14+, Android API 26+
- 60 fps on mid-range devices (Snapdragon 665 / A13)
- Maximum 200 MB initial download

## Art Direction
- Clean flat-design UI with bold typography (Inter / Roboto)
- Pitch visualisation: top-down 2D with animated player dots
- Club colours auto-applied to UI chrome per selected team
- Accessibility: colour-blind-safe palette, minimum 44pt touch targets
