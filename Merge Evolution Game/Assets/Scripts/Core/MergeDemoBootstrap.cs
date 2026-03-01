using MergeEvolution.Board;
using MergeEvolution.Input;
using MergeEvolution.Logic;
using UnityEngine;

namespace MergeEvolution.Core
{
    public class MergeDemoBootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<BoardManager>() != null) return;

            SetupCamera();
            SetupBackground();

            var systems = new GameObject("DemoSystems");
            systems.AddComponent<GameManager>();
            systems.AddComponent<BoardManager>();
            systems.AddComponent<InputManager>();
        }

        private static void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGo = new GameObject("Main Camera");
                cam = camGo.AddComponent<Camera>();
                cam.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 6.4f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.35f, 0.72f, 0.95f);
        }

        private static void SetupBackground()
        {
#if UNITY_EDITOR
            CreateLayer("Sky", "Assets/Sprites/UI/ceu-cenario.png", new Vector3(0, 0, 10), 0.055f);
            CreateLayer("Trees", "Assets/Sprites/UI/mega-arvores.png", new Vector3(0, -2.2f, 9), 0.04f);
            CreateLayer("Board", "Assets/Sprites/UI/taboleiro5x8.png", new Vector3(0, -0.2f, 0), 0.032f);
#endif
        }

#if UNITY_EDITOR
        private static void CreateLayer(string name, string assetPath, Vector3 pos, float scale)
        {
            if (GameObject.Find(name) != null) return;

            var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite == null) return;

            var go = new GameObject(name);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = name == "Board" ? 5 : 0;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * scale;
        }
#endif
    }
}
