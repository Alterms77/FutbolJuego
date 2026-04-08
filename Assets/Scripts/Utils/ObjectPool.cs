using System.Collections.Generic;
using UnityEngine;

namespace FutbolJuego.Utils
{
    // ── Generic POCO pool ──────────────────────────────────────────────────────

    /// <summary>
    /// Thread-safe (single-threaded Unity) generic object pool for plain C#
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

    // ── MonoBehaviour pool ─────────────────────────────────────────────────────

    /// <summary>
    /// Unity prefab pool for <see cref="MonoBehaviour"/>-derived components.
    /// Attach to a manager GameObject and assign the <see cref="prefab"/>.
    /// </summary>
    public class MonoBehaviourPool<T> : MonoBehaviour where T : MonoBehaviour
    {
        [Tooltip("Prefab to pool.")]
        [SerializeField] private T prefab;

        [Tooltip("Objects to create during Awake.")]
        [SerializeField] private int prewarmCount = 10;

        private readonly Queue<T> pool = new Queue<T>();

        private void Awake()
        {
            for (int i = 0; i < prewarmCount; i++)
                ReturnNew();
        }

        /// <summary>
        /// Returns an instance from the pool (or instantiates one) and
        /// optionally re-parents it to <paramref name="parent"/>.
        /// </summary>
        public T Get(Transform parent = null)
        {
            T instance = pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab);
            instance.transform.SetParent(parent);
            instance.gameObject.SetActive(true);
            return instance;
        }

        /// <summary>
        /// Deactivates <paramref name="item"/> and returns it to the pool.
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            item.transform.SetParent(transform);
            pool.Enqueue(item);
        }

        /// <summary>Number of inactive items waiting in the pool.</summary>
        public int Count => pool.Count;

        private void ReturnNew()
        {
            T instance = Instantiate(prefab, transform);
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }
}
