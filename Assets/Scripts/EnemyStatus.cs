using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

public class EnemyStatus : MonoBehaviour {
   
    [SerializeField]
    private string uniqueID = "";

    public string UniqueID => uniqueID;

    private bool didRegister = false;

#if UNITY_EDITOR
    private void OnValidate() {
        if (!Application.isPlaying) {
            bool isInstanceInScene = gameObject.scene.IsValid();
            if (string.IsNullOrEmpty(uniqueID) && isInstanceInScene) {
                GenerateUniqueID();
                EditorUtility.SetDirty(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
    }

    private void Reset() {
        GenerateUniqueID();
        if (!Application.isPlaying) {
            EditorUtility.SetDirty(this);
            if (gameObject.scene.IsValid())
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
#endif

    void Awake() {
        if (string.IsNullOrEmpty(uniqueID)) {
            GenerateUniqueID();
        }

        if (!string.IsNullOrEmpty(uniqueID)) {
            RegisterWithGameManager();
        }
    }

    void OnEnable() {
        if (!didRegister && !string.IsNullOrEmpty(uniqueID)) {
            RegisterWithGameManager();
        }
    }

    void OnDisable() {
        if (didRegister) {
            UnregisterWithGameManager();
        }
    }

    void OnDestroy() {
        if (didRegister) {
            UnregisterWithGameManager();
        }
    }

    private void GenerateUniqueID() {
        uniqueID = System.Guid.NewGuid().ToString();
    }

    private void RegisterWithGameManager() {
        if (GameManager.Instance != null) {
            GameManager.Instance.RegisterEnemy(this);
            didRegister = true;
        }
    }

    private void UnregisterWithGameManager() {
        if (GameManager.Instance != null) {
            GameManager.Instance.UnregisterEnemy(this);
        }
        didRegister = false;
    }

    public void AssignUniqueIDRuntime() {
        if (string.IsNullOrEmpty(uniqueID)) {
            uniqueID = System.Guid.NewGuid().ToString();
        }
    }
}
