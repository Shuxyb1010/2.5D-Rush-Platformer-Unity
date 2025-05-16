using UnityEngine;

[RequireComponent(typeof(Animator))]
public class CoinAnimation : MonoBehaviour {
    private Animator _anim;
    private static readonly int CollectedHash = Animator.StringToHash("Collected");

    void Start() {
        _anim = GetComponent<Animator>();
        StartIdleVariation();
    }

    void StartIdleVariation() {
        _anim.Play("Base Layer.Idle", 0, Random.Range(0f, 1f));
    }

    public void Collect() {
        _anim.SetTrigger(CollectedHash);
        GetComponent<Collider>().enabled = false;
    }
}