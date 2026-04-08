using System;
using System.Collections.Generic;
using Godot;

namespace FutbolJuego.Core
{
    /// <summary>
    /// Thread-safe (main-thread) event bus that decouples publishers and
    /// subscribers using strongly-typed event payloads.
    /// </summary>
    public class EventBus
    {
        private static EventBus globalInstance;

        /// <summary>Global singleton instance (initialised lazily).</summary>
        public static EventBus Global => globalInstance ??= new EventBus();

        private readonly Dictionary<Type, Delegate> handlers = new Dictionary<Type, Delegate>();

        // ── Subscription ───────────────────────────────────────────────────────

        /// <summary>
        /// Subscribes <paramref name="handler"/> to events of type
        /// <typeparamref name="T"/>.
        /// </summary>
        public void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            Type key = typeof(T);
            if (handlers.TryGetValue(key, out Delegate existing))
                handlers[key] = Delegate.Combine(existing, handler);
            else
                handlers[key] = handler;
        }

        /// <summary>
        /// Removes <paramref name="handler"/> from the subscribers for
        /// <typeparamref name="T"/>. Safe to call even if not subscribed.
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;

            Type key = typeof(T);
            if (handlers.TryGetValue(key, out Delegate existing))
            {
                Delegate updated = Delegate.Remove(existing, handler);
                if (updated == null)
                    handlers.Remove(key);
                else
                    handlers[key] = updated;
            }
        }

        // ── Publishing ─────────────────────────────────────────────────────────

        /// <summary>
        /// Publishes <paramref name="eventData"/> to all subscribers of
        /// <typeparamref name="T"/>. Exceptions inside handlers are caught and
        /// logged so that one bad handler cannot interrupt others.
        /// </summary>
        public void Publish<T>(T eventData)
        {
            Type key = typeof(T);
            if (!handlers.TryGetValue(key, out Delegate del)) return;

            if (del is Action<T> action)
            {
                foreach (Action<T> subscriber in action.GetInvocationList())
                {
                    try
                    {
                        subscriber(eventData);
                    }
                    catch (Exception ex)
                    {
                        GD.PushError($"[EventBus] Handler exception for event {key.Name}: {ex}");
                    }
                }
            }
        }

        /// <summary>Removes all subscriptions for event type <typeparamref name="T"/>.</summary>
        public void ClearType<T>() => handlers.Remove(typeof(T));

        /// <summary>Removes every subscription on this bus instance.</summary>
        public void Clear() => handlers.Clear();

        /// <summary>Clears the global singleton instance's subscriptions.</summary>
        public static void ClearAll() => Global.Clear();

        /// <summary>Returns the number of distinct event types with active subscriptions.</summary>
        public int SubscriptionCount => handlers.Count;
    }

    // ── Common event payloads ──────────────────────────────────────────────────

    /// <summary>Published when the game state changes.</summary>
    public struct GameStateChangedEvent
    {
        /// <summary>State before the transition.</summary>
        public GameStateType PreviousState;
        /// <summary>State after the transition.</summary>
        public GameStateType NewState;
    }

    /// <summary>Published when a match finishes.</summary>
    public struct MatchCompletedEvent
    {
        /// <summary>Full result data for the completed match.</summary>
        public FutbolJuego.Models.MatchData MatchData;
    }

    /// <summary>Published when the player's finances change.</summary>
    public struct FinancesUpdatedEvent
    {
        /// <summary>New balance after the update.</summary>
        public long NewBalance;
    }

    /// <summary>Published when a transfer is completed.</summary>
    public struct TransferCompletedEvent
    {
        /// <summary>Player that was transferred.</summary>
        public FutbolJuego.Models.PlayerData Player;
        /// <summary>Buying team identifier.</summary>
        public string BuyerTeamId;
        /// <summary>Selling team identifier.</summary>
        public string SellerTeamId;
        /// <summary>Agreed transfer fee.</summary>
        public int Fee;
    }
}
