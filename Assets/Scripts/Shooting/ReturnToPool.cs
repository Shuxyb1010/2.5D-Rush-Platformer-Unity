using UnityEngine;
using static PoolItem;

public class ReturnToPool : MonoBehaviour {
    public static void Return(GameObject gameObject) {
        Pool.instance.Return(gameObject);
    }

    public void Return() {
        Pool.instance.Return(gameObject);
    }
}
