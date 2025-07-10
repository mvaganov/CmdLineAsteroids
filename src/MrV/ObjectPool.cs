// MIT license - TL;DR - Do whatever you want with it, I won't fix it for you!
#define FAIL_FAST
#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// <para>Used for memory recycling of reference types, like GameObjects</para>
/// <para>Example usage:</para>
/// <para>ObjectPool&lt;GameObject&gt; objPool = new ObjectPool&lt;GameObject&gt;(); // construction</para>
/// <para>objPool.Setup(
///		()    => { return Instantiate(prefab); },
///		(obj) => { obj.SetActive(true);  },
///		(obj) => { obj.SetActive(false); },
///		(obj) => { Object.Destroy(obj);  }
/// );</para>
/// <para>GameObject gobj = objPool.Alloc();  // allocate object in pool</para>
/// <para>objPool.Free(gobj); // deallocate object in pool</para>
/// <para>ObjectPoolItem.Destroy(gobj); // deallocate object in pool OR Object.Destroy non-ObjectPool object (for GameObjects only)</para>
/// </summary>
public class ObjectPool<T> : IList<T> {
	private List<T> allObjects = new List<T>();
	private int freeObjectCount = 0;
	/// <summary>
	/// Used to memory-allocate/marshall a new object
	/// </summary>
	/// <returns>a fresh object</returns>
	public delegate T DelegateBeginLife();
	/// <summary>
	/// Used to clean an object before use as a newly allocated object
	/// </summary>
	/// <param name="obj"></param>
	public delegate void DelegateCommission(T obj);
	/// <summary>
	/// Used to deactivate an object fully before it is put into the pool of free objects
	/// </summary>
	/// <param name="obj"></param>
	public delegate void DelegateDecommission(T obj);
	/// <summary>
	/// Used to memory-free an object, when the memory is no longer needed, in final stages of lifecycle
	/// </summary>
	/// <param name="obj"></param>
	public delegate void DelegateEndLife(T obj);

	/// <inheritdoc cref="DelegateBeginLife"/>
	public DelegateBeginLife Birth;
	/// <inheritdoc cref="DelegateEndLife"/>
	public DelegateEndLife Death;
	/// <inheritdoc cref="DelegateCommission"/>
	public DelegateCommission Commission;
	/// <inheritdoc cref="DelegateDecommission"/>
	public DelegateDecommission Decommission;

	public int Count => allObjects.Count - freeObjectCount;

	public bool IsReadOnly => false;

	public T this[int index] { get => allObjects[index]; set => throw new System.NotImplementedException(); }

	public int Capacity {
		get { return allObjects.Count; }
		set {
			allObjects.Capacity = value;
			for (int i = allObjects.Count; i < allObjects.Capacity; ++i) {
				T obj = Alloc();
				allObjects.Add(obj);
				Free(obj);
			}
		}
	}

	/// <summary></summary>
	/// <returns>True if Setup was called with all non-null methods</returns>
	public bool IsFullySetup() {
		return Birth != null && Commission != null && Decommission != null && Death != null;
	}

	/// <summary>
	/// Example usage:
	/// <para>objPool.Setup(
	///		()    => Instantiate(prefab),
	///		(obj) => obj.SetActive(true),
	///		(obj) => obj.SetActive(false),
	///		(obj) => Object.Destroy(obj)
	/// );</para>
	/// </summary>
	/// <param name="create">callback function or delegate used to create a new object of type T</param>
	/// <param name="activate">(optional) callback function or delegate used to activate an object of type T</param>
	/// <param name="deactivate">(optional) callback function or delegate used to de-activate an object of type T</param>
	/// <param name="destroy">(optional) callback function or delegate used to destroy an object of type T</param>
	public void Setup(DelegateBeginLife create, DelegateCommission activate, DelegateDecommission deactivate, DelegateEndLife destroy) {
		Birth = create; Commission = activate; Decommission = deactivate; Death = destroy;
	}

	/// <summary>Constructs and calls <see cref="Setup"/></summary>
	public ObjectPool(
		DelegateBeginLife create,
		DelegateCommission activate,
		DelegateDecommission deactivate,
		DelegateEndLife destroy) {
		Setup(create, activate, deactivate, destroy);
	}

	/// <summary> Be sure to call <see cref="Setup"/>!</summary>
	public ObjectPool() { }

	/// <summary>Returns an object from the memory pool, which may have just been created</summary>
	public T Alloc() {
		T freeObject = default;
		if (freeObjectCount == 0) {
#if FAIL_FAST
			if (Birth == null) { throw new System.Exception("Call .Setup(), and provide a create method!"); }
#endif
			if (allObjects == null) { allObjects = new List<T>(); }
			freeObject = Birth();
			allObjects.Add(freeObject);
#if UNITY_5_3_OR_NEWER
			if (typeof(T) == typeof(GameObject)) {
				GameObject go = freeObject as GameObject;
				go.AddComponent<ObjectPoolItem>().SetPool(this as ObjectPool<GameObject>);
			}
#endif
		} else {
			freeObject = allObjects[allObjects.Count - freeObjectCount];
			--freeObjectCount;
		}
		if (Commission != null) { Commission(freeObject); }
		return freeObject;
	}

	/// <summary>Which object to mark as free in the memory pool</summary>
	public void Free(T obj) {
		int indexOfObject = allObjects.IndexOf(obj);
#if FAIL_FAST
		if (indexOfObject < 0) { throw new System.Exception("woah, this isn't one of mine..."); }
		if (indexOfObject >= (allObjects.Count - freeObjectCount)) { throw new System.Exception("hey, you're freeing this twice..."); }
#endif
		freeObjectCount++;
		int beginningOfFreeList = allObjects.Count - freeObjectCount;
		allObjects[indexOfObject] = allObjects[beginningOfFreeList];
		allObjects[beginningOfFreeList] = obj;
		if (Decommission != null) { Decommission(obj); }
	}

	/// <summary>performs the given delegate on each object in the memory pool</summary>
	public void ForEach(DelegateCommission action) {
		for (int i = 0; i < allObjects.Count; ++i) {
			action.Invoke(allObjects[i]);
		}
	}

	public void ForEachCommissioned(DelegateCommission action) {
		for (int i = 0; i < allObjects.Count - freeObjectCount; ++i) {
			action.Invoke(allObjects[i]);
		}
	}

	public void ForEachDecommissioned(DelegateCommission action) {
		for (int i = allObjects.Count - freeObjectCount; i < allObjects.Count; ++i) {
			action.Invoke(allObjects[i]);
		}
	}

	/// <summary>Destroys all objects in the pool, after deactivating each one.</summary>
	public void DeallocateAll() {
		ForEachCommissioned((item) => Decommission(item));
#if UNITY_5_3_OR_NEWER
		if (typeof(T) == typeof(GameObject)) {
			ForEach((item) => {
				GameObject go = item as GameObject;
				Object.DestroyImmediate(go.GetComponent<ObjectPoolItem>());
			});
		}
#endif
		if (Death != null) { ForEach((item) => Death(item)); }
		allObjects.Clear();
	}

	public void Clear() {
		for (int i = allObjects.Count - freeObjectCount - 1; i >= 0; --i) {
			Free(allObjects[i]);
		}
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	public int IndexOf(T item) => allObjects.IndexOf(item);
	public void Insert(int index, T item) => throw new System.NotImplementedException();
	public void RemoveAt(int index) => throw new System.NotImplementedException();
	public void Add(T item) => throw new System.NotImplementedException();
	public bool Contains(T item) => IndexOf(item) != -1;
	public bool Remove(T item) => throw new System.NotImplementedException();
	public void CopyTo(T[] array, int arrayIndex) {
		for(int i = arrayIndex, index = 0; i < array.Length && index < Count; ++i, ++index) {
			array[i] = this[index];
		}
	}
	private class ObjectPoolEnumerator : IEnumerator<T> {
		private int index = -1, max = 0; private IList<T> list;
		public T Current => list[index]; object IEnumerator.Current => Current;
		public void Dispose() {} public bool MoveNext() => ++index < max; public void Reset() => index = -1;
		public ObjectPoolEnumerator(IList<T> list, int max) { this.list = list; this.max = max; }
	}
	public IEnumerator<T> GetEnumerator() => new ObjectPoolEnumerator(allObjects, Count);
}

#if UNITY_5_3_OR_NEWER
public class ObjectPool : MonoBehaviour {
	ObjectPool<GameObject> pool = new ObjectPool<GameObject>();
	[Tooltip("An object to create and destroy a lot of"), ContextMenuItem("Create Another", "CreateAnother")]
	public GameObject prefab;

	public void Init() {
		if (!pool.IsFullySetup()) {
			pool.Setup(() => Instantiate(prefab), (obj) => obj.SetActive(true), (obj) => obj.SetActive(false), (obj) => Destroy(obj));
		}
	}

	public void Awake() { Init(); }

	public GameObject CreateAnother() {
#if UNITY_EDITOR
		Init();
#endif
		AssertInit(); return pool.Alloc();
	}

	public void AssertInit() {
#if FAIL_FAST
		if (!pool.IsFullySetup()) throw new System.Exception("Call Init() on this memory pool before using it");
#endif
	}

	public void Setup(ObjectPool<GameObject>.DelegateBeginLife create, ObjectPool<GameObject>.DelegateCommission activate,
		ObjectPool<GameObject>.DelegateDecommission deactivate, ObjectPool<GameObject>.DelegateEndLife destroy) {
		pool.Setup(create, activate, deactivate, destroy);
	}

	/// <summary> Be sure to call <see cref="Setup"/>!</summary>
	public ObjectPool() { }

	/// <summary>Returns an object from the memory pool, which may have just been created</summary>
	public GameObject Alloc() { AssertInit(); return pool.Alloc(); }

	/// <summary>Which object to mark as free in the memory pool</summary>
	public void Free(GameObject obj) { AssertInit(); pool.Free(obj); }

	/// <summary>performs the given delegate on each object in the memory pool</summary>
	public void ForEach(ObjectPool<GameObject>.DelegateCommission action) { AssertInit(); pool.ForEach(action); }

	/// <summary>Destroys all objects in the pool, after deactivating each one.</summary>
	public void DeallocateAll() { AssertInit(); pool.DeallocateAll(); }

	/// <summary>If the given GameObject belongs to a memory pool, mark it as free in that pool. Otherwise, Object.Destroy()</summary>
	static public void Destroy(GameObject go) { ObjectPoolItem.Destroy(go); }
}

[System.Serializable]
public class ObjectPoolItem : MonoBehaviour {
	private ObjectPool<GameObject> gameObjectPool;
	public ObjectPoolItem SetPool(ObjectPool<GameObject> pool) { gameObjectPool = pool; return this; }
	static private bool shuttingDown = false;
	static public void SetShutdown(bool sceneIsEnding) { shuttingDown = sceneIsEnding; }
#if FAIL_FAST
	void OnApplicationQuit() { SetShutdown(true); }
	void Start() { SetShutdown(false); }
	void OnDestroy() {
		if (!shuttingDown) throw new System.Exception("Instead of Object.Destroy(" + gameObject + "), call ObjectPoolItem.Destroy(" + gameObject + ")\n"
			+ "When changing levels, call ObjectPoolItem.SetShutdown(true) first");
	}
#endif
	public void FreeSelf() { gameObjectPool.Free(gameObject); }
	/// <summary>If the given GameObject belongs to a memory pool, mark it as free in that pool. Otherwise, Object.Destroy()</summary>
	static public void Destroy(GameObject go) {
		ObjectPoolItem i = go.GetComponent<ObjectPoolItem>();
		if (i != null) { i.FreeSelf(); } else { Object.Destroy(go); }
	}
}
#endif
