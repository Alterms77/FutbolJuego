# FutbolJuego – Firestore Database Schema

## Collection: `users/{userId}`

### `users/{userId}/profile/data`
```json
{
  "displayName":       "Alex Manager",
  "email":             "alex@example.com",
  "createdAt":         "2024-01-15T10:00:00Z",
  "managerLevel":      5,
  "xp":                1250,
  "coins":             12500,
  "premiumCurrency":   40
}
```

### `users/{userId}/gameState/data`
```json
{
  "currentTeamId":    "team-fcm",
  "season":           2,
  "balance":          18500000,
  "transferBudget":   8000000,
  "managerXp":        1250,
  "leaguePosition":   3,
  "lastSaved":        "2024-03-10T18:22:00Z"
}
```

### `users/{userId}/rewards/daily`
```json
{
  "lastClaimedDate": "2024-03-10",
  "currentStreak":   4
}
```

## Collection: `leaderboards/{type}/entries/{userId}`
Types: `season_points`, `manager_xp`, `trophies`
```json
{
  "userId":    "uid_abc123",
  "score":     87,
  "teamName":  "FC Metropolitan",
  "season":    2,
  "timestamp": "2024-03-10T22:00:00Z"
}
```

## Collection: `liveEvents/{eventId}`
```json
{
  "name":        "Weekend Cup Rush",
  "startDate":   "2024-03-08T00:00:00Z",
  "endDate":     "2024-03-10T23:59:59Z",
  "description": "Win 3 matches this weekend to earn bonus FutCoins!",
  "rewards": [
    { "currencyType": "premiumCurrency", "amount": 50 }
  ]
}
```

## Collection: `weeklyChallenge/{docId}`
```json
{
  "description": "Score 10 goals in league matches",
  "target":      10,
  "field":       "leagueGoals",
  "startDate":   "2024-03-11T00:00:00Z",
  "endDate":     "2024-03-17T23:59:59Z",
  "reward":      { "type": "premiumCurrency", "amount": 30 }
}
```

## Collection: `matches/{matchId}`
```json
{
  "homeTeamId":  "team-fcm",
  "awayTeamId":  "team-ar",
  "homeScore":   2,
  "awayScore":   1,
  "validated":   true,
  "validatedBy": "uid_abc123",
  "timestamp":   "2024-03-09T15:00:00Z"
}
```

## Collection: `config/{docId}`
```json
{
  "gameVersion":          "1.0.0",
  "maintenanceMode":      false,
  "defaultTransferBudget": 5000000,
  "seasonLengthWeeks":    38
}
```

## Security Summary
| Collection       | Read             | Write                    |
|-----------------|------------------|--------------------------|
| users/{uid}/**  | Owner only       | Owner only               |
| leaderboards/** | Public           | Cloud Functions only     |
| liveEvents/**   | Public           | Admin only               |
| weeklyChallenge | Public           | Cloud Functions only     |
| matches/**      | Authenticated    | Cloud Functions only     |
| config/**       | Public           | Admin only               |
