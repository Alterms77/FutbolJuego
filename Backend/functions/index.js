"use strict";

const functions = require("firebase-functions");
const admin     = require("firebase-admin");

admin.initializeApp();

const db   = admin.firestore();
const auth = admin.auth();

// ─── Helpers ───────────────────────────────────────────────────────────────────

/** Throws an HttpsError if the request is unauthenticated. */
function requireAuth(context) {
  if (!context.auth) {
    throw new functions.https.HttpsError(
      "unauthenticated",
      "You must be signed in to call this function."
    );
  }
  return context.auth.uid;
}

/** Simple server-side xG sanity check. */
function isPlausibleScore(homeScore, awayScore) {
  return (
    Number.isInteger(homeScore) && homeScore >= 0 && homeScore <= 15 &&
    Number.isInteger(awayScore) && awayScore >= 0 && awayScore <= 15
  );
}

// ─── User lifecycle ────────────────────────────────────────────────────────────

/**
 * Triggered when a new Firebase Auth user is created.
 * Bootstraps their Firestore profile and an empty game state.
 */
exports.onUserCreated = functions.auth.user().onCreate(async (user) => {
  const now = admin.firestore.FieldValue.serverTimestamp();

  const batch = db.batch();

  // User profile
  batch.set(db.doc(`users/${user.uid}/profile/data`), {
    displayName:  user.displayName || "New Manager",
    email:        user.email || "",
    createdAt:    now,
    managerLevel: 1,
    xp:           0,
  });

  // Initial game state (blank)
  batch.set(db.doc(`users/${user.uid}/gameState/data`), {
    currentTeamId:   null,
    season:          1,
    balance:         10000000,
    transferBudget:  5000000,
    managerXp:       0,
    lastSaved:       now,
  });

  // Daily-reward tracker
  batch.set(db.doc(`users/${user.uid}/rewards/daily`), {
    lastClaimedDate: null,
    currentStreak:   0,
  });

  await batch.commit();
  console.log(`[onUserCreated] Initialised Firestore for user ${user.uid}`);
});

// ─── Game state ────────────────────────────────────────────────────────────────

/**
 * Saves the caller's game state.
 * Expects: { gameState: object }
 */
exports.saveGameState = functions.https.onCall(async (data, context) => {
  const uid = requireAuth(context);

  if (!data.gameState || typeof data.gameState !== "object") {
    throw new functions.https.HttpsError("invalid-argument", "gameState must be an object.");
  }

  const stateRef = db.doc(`users/${uid}/gameState/data`);
  await stateRef.set(
    { ...data.gameState, lastSaved: admin.firestore.FieldValue.serverTimestamp() },
    { merge: true }
  );

  return { success: true, timestamp: Date.now() };
});

/**
 * Returns the caller's saved game state.
 */
exports.loadGameState = functions.https.onCall(async (data, context) => {
  const uid = requireAuth(context);

  const doc = await db.doc(`users/${uid}/gameState/data`).get();
  if (!doc.exists) {
    return { gameState: null };
  }
  return { gameState: doc.data() };
});

// ─── Leaderboard ───────────────────────────────────────────────────────────────

/**
 * Returns the top-50 entries for the requested leaderboard type.
 * Expects: { type: "season_points" | "manager_xp" | "trophies" }
 */
exports.getLeaderboard = functions.https.onCall(async (data, context) => {
  const type  = data?.type || "season_points";
  const limit = Math.min(data?.limit || 50, 100);

  const snap = await db
    .collection(`leaderboards/${type}/entries`)
    .orderBy("score", "desc")
    .limit(limit)
    .get();

  const entries = snap.docs.map((doc) => ({ id: doc.id, ...doc.data() }));
  return { entries };
});

/**
 * Upserts the caller's entry on the specified leaderboard.
 * Expects: { type: string, score: number, teamName: string, season: number }
 */
exports.submitLeaderboardScore = functions.https.onCall(async (data, context) => {
  const uid = requireAuth(context);

  const { type, score, teamName, season } = data;
  if (!type || typeof score !== "number") {
    throw new functions.https.HttpsError("invalid-argument", "type and score are required.");
  }

  const entryRef = db.doc(`leaderboards/${type}/entries/${uid}`);
  await entryRef.set({
    userId:    uid,
    score,
    teamName:  teamName || "Unknown",
    season:    season   || 1,
    timestamp: admin.firestore.FieldValue.serverTimestamp(),
  });

  return { success: true };
});

// ─── Daily rewards ─────────────────────────────────────────────────────────────

/**
 * Claims the daily reward for the current day.
 * Enforces one claim per calendar day (UTC).
 */
exports.claimDailyReward = functions.https.onCall(async (data, context) => {
  const uid = requireAuth(context);

  const rewardRef = db.doc(`users/${uid}/rewards/daily`);
  const rewardDoc = await rewardRef.get();
  const rewardData = rewardDoc.exists ? rewardDoc.data() : {};

  const today = new Date().toISOString().slice(0, 10); // "YYYY-MM-DD"
  if (rewardData.lastClaimedDate === today) {
    return { success: false, reason: "Already claimed today." };
  }

  // Compute streak
  const yesterday = new Date(Date.now() - 86400000).toISOString().slice(0, 10);
  const streak    = rewardData.lastClaimedDate === yesterday
    ? (rewardData.currentStreak || 0) + 1
    : 1;

  // Weekly reward cycle (day 1-7)
  const day = ((streak - 1) % 7) + 1;

  // Reward table
  const rewardTable = [
    { type: "coins",           amount: 500   },
    { type: "coins",           amount: 750   },
    { type: "premiumCurrency", amount: 10    },
    { type: "coins",           amount: 1000  },
    { type: "coins",           amount: 1500  },
    { type: "premiumCurrency", amount: 25    },
    { type: "premiumCurrency", amount: 50    },
  ];
  const reward = rewardTable[day - 1];

  // Batch update
  const batch = db.batch();
  batch.update(rewardRef, {
    lastClaimedDate: today,
    currentStreak:   streak,
  });

  // Credit reward to user profile
  const profileRef = db.doc(`users/${uid}/profile/data`);
  if (reward.type === "premiumCurrency") {
    batch.update(profileRef, {
      premiumCurrency: admin.firestore.FieldValue.increment(reward.amount),
    });
  } else {
    batch.update(profileRef, {
      coins: admin.firestore.FieldValue.increment(reward.amount),
    });
  }

  await batch.commit();
  return { success: true, day, reward, streak };
});

// ─── Live events ───────────────────────────────────────────────────────────────

/**
 * Returns all currently active live events (endDate > now).
 */
exports.getActiveEvents = functions.https.onCall(async (data, context) => {
  const now  = admin.firestore.Timestamp.now();
  const snap = await db
    .collection("liveEvents")
    .where("endDate", ">", now)
    .where("startDate", "<=", now)
    .get();

  const events = snap.docs.map((doc) => ({ id: doc.id, ...doc.data() }));
  return { events };
});

// ─── Weekly challenge generator ────────────────────────────────────────────────

/**
 * Runs every Monday at midnight UTC.
 * Generates a fresh weekly challenge document.
 */
exports.generateWeeklyChallenge = functions.pubsub
  .schedule("every monday 00:00")
  .timeZone("UTC")
  .onRun(async (_context) => {
    const challenges = [
      { description: "Score 10 goals in league matches",   target: 10, field: "leagueGoals"   },
      { description: "Win 3 consecutive league matches",    target: 3,  field: "winStreak"     },
      { description: "Keep 3 clean sheets",                target: 3,  field: "cleanSheets"   },
      { description: "Sign 2 players from the free market",target: 2,  field: "freeSignings"  },
      { description: "Upgrade any facility",               target: 1,  field: "upgrades"      },
    ];

    const chosen = challenges[Math.floor(Math.random() * challenges.length)];

    const weekStart = new Date();
    weekStart.setHours(0, 0, 0, 0);
    const weekEnd = new Date(weekStart.getTime() + 7 * 24 * 60 * 60 * 1000);

    await db.collection("weeklyChallenge").add({
      ...chosen,
      startDate: admin.firestore.Timestamp.fromDate(weekStart),
      endDate:   admin.firestore.Timestamp.fromDate(weekEnd),
      reward:    { type: "premiumCurrency", amount: 30 },
      createdAt: admin.firestore.FieldValue.serverTimestamp(),
    });

    console.log(`[generateWeeklyChallenge] Created challenge: ${chosen.description}`);
  });

// ─── Anti-cheat: validate match result ────────────────────────────────────────

/**
 * Server-side sanity check on a reported match result.
 * Expects: { homeScore, awayScore, homeTeamId, awayTeamId, matchId }
 */
exports.validateMatchResult = functions.https.onCall(async (data, context) => {
  const uid = requireAuth(context);

  const { homeScore, awayScore, homeTeamId, awayTeamId, matchId } = data;

  // Basic plausibility
  if (!isPlausibleScore(homeScore, awayScore)) {
    throw new functions.https.HttpsError(
      "invalid-argument",
      `Implausible score: ${homeScore}-${awayScore}`
    );
  }

  // Check the match isn't already recorded
  const matchRef  = db.doc(`matches/${matchId}`);
  const matchSnap = await matchRef.get();
  if (matchSnap.exists && matchSnap.data().validated) {
    return { valid: false, reason: "Match already validated." };
  }

  // Store validated result
  await matchRef.set({
    homeTeamId,
    awayTeamId,
    homeScore,
    awayScore,
    validatedBy: uid,
    validated:   true,
    timestamp:   admin.firestore.FieldValue.serverTimestamp(),
  }, { merge: true });

  return { valid: true };
});

// ─── Profile read ──────────────────────────────────────────────────────────────

/**
 * Returns the caller's profile data (manager level, XP, currencies).
 */
exports.getProfile = functions.https.onCall(async (data, context) => {
  const uid  = requireAuth(context);
  const doc  = await db.doc(`users/${uid}/profile/data`).get();
  return doc.exists ? { profile: doc.data() } : { profile: null };
});
