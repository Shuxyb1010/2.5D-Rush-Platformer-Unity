using UnityEngine;

using Unity.Cinemachine;

public class CameraShake : MonoBehaviour {
    public static CameraShake Instance { get; private set; }
    private CinemachineImpulseSource impulseSource;

    private void Awake() {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void Shake(float force) {
        impulseSource.GenerateImpulse(new Vector3(1, 1, 0) * force);
    }
}