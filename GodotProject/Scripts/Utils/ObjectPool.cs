using System.Collections.Generic;
using Godot;

namespace FutbolJuego.Utils
{
    // ── Generic POCO pool ──────────────────────────────────────────────────────

    /// <summary>
    /// Thread-safe (single-threaded) generic object pool for plain C#
    /// objects.  Pre-warms on construction if desired.
    /// </summary>
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Queue<T> pool = new Queue<T>();

        /// <summary>Creates a pool, optionally pre-warming it.</summary>
        public ObjectPool(int prewarmCount = 0)
        {
            Prewarm(prewarmCount);
        }

        /// <summary>
        /// Retrieves an instance from the pool, or creates a new one if empty.
        /// </summary>
        public T Get()
        {
            return pool.Count > 0 ? pool.Dequeue() : new T();
        }

        /// <summary>Returns an instance to the pool for reuse.</summary>
        public void Return(T item)
        {
            if (item == null) return;
            pool.Enqueue(item);
        }

        /// <summary>Pre-allocates <paramref name="count"/> instances.</summary>
        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                pool.Enqueue(new T());
        }

        /// <summary>Number of instances currently in the pool.</summary>
        public int Count => pool.Count;

        /// <summary>Disposes all pooled instances (if <typeparamref name="T"/> implements IDisposable).</summary>
        public void Clear() => pool.Clear();
    }

    // ── Node pool ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Godot node pool for <see cref="Node"/>-derived components.
    /// Attach to a manager node and assign the <see cref="prefab"/>.
    /// </summary>
    public partial class NodePool<T> : Node where T : Node
    {
        [Tooltip("Prefab to pool.")]
        [Export] private T prefab;

        [Tooltip("Objects to create during _Ready.")]
        [Export] private int prewarmCount = 10;

        private readonly Queue<T> pool = new Queue<T>();

        public override void _Ready()
        {
            for (int i = 0; i < prewarmCount; i++)
                ReturnNew();
        }

        /// <summary>
        /// Returns an instance from the pool (or duplicates the prefab) and
        /// optionally re-parents it to <paramref name="parent"/>.
        /// </summary>
        public T Get(Node parent = null)
        {
            T instance = pool.Count > 0 ? pool.Dequeue() : prefab.Duplicate() as T;
            parent?.AddChild(instance);
            instance.Visible = true;
            return instance;
        }

        /// <summary>
        /// Hides <paramref name="item"/> and returns it to the pool.
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            item.Visible = false;
            item.Reparent(this);
            pool.Enqueue(item);
        }

        /// <summary>Number of inactive items waiting in the pool.</summary>
        public int Count => pool.Count;

        private void ReturnNew()
        {
            T instance = prefab.Duplicate() as T;
            AddChild(instance);
            instance.Visible = false;
            pool.Enqueue(instance);
        }
    }
}
