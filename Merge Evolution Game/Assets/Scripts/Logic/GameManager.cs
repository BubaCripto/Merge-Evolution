using UnityEngine;
using UnityEngine.UI;

namespace MergeEvolution.Logic
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Rules")]
        public int startMoves = 30;
        public int targetScore = 1500;

        [Header("UI")]
        public Text movesText;
        public Text scoreText;
        public Text galleryText;
        public Text resultText;

        public int MovesLeft { get; private set; }
        public int Score { get; private set; }
        public int GalleryCount { get; private set; }
        public bool IsGameOver { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            EnsureUI();
            ResetGame();
        }

        public void ResetGame()
        {
            MovesLeft = startMoves;
            Score = 0;
            GalleryCount = 0;
            IsGameOver = false;
            if (resultText != null) resultText.text = string.Empty;
            RefreshUI();
        }

        public void SpendMove()
        {
            if (IsGameOver) return;
            MovesLeft = Mathf.Max(0, MovesLeft - 1);
            if (MovesLeft == 0)
            {
                IsGameOver = true;
                if (resultText != null)
                {
                    resultText.text = Score >= targetScore ? "Vitória!" : "Fim de jogo";
                    resultText.color = Score >= targetScore ? Color.green : Color.red;
                }
            }
            RefreshUI();
        }

        public void AddScore(int amount)
        {
            Score += amount;
            RefreshUI();
        }

        public void AddBabyToGallery(int amount = 1)
        {
            GalleryCount += amount;
            AddScore(120);
            RefreshUI();
        }

        private void EnsureUI()
        {
            if (movesText != null && scoreText != null && galleryText != null && resultText != null) return;

            var canvasGo = new GameObject("DemoHUD");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            canvasGo.AddComponent<GraphicRaycaster>();

            movesText = CreateText(canvas.transform, "MovesText", new Vector2(30, -30));
            scoreText = CreateText(canvas.transform, "ScoreText", new Vector2(30, -80));
            galleryText = CreateText(canvas.transform, "GalleryText", new Vector2(30, -130));
            resultText = CreateText(canvas.transform, "ResultText", new Vector2(30, -200), 40);
        }

        private static Text CreateText(Transform parent, string name, Vector2 pos, int size = 32)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(900, 70);

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.fontSize = size;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            return txt;
        }

        private void RefreshUI()
        {
            if (movesText != null) movesText.text = $"Movimentos: {MovesLeft}";
            if (scoreText != null) scoreText.text = $"Pontuação: {Score}/{targetScore}";
            if (galleryText != null) galleryText.text = $"Galeria (bebês): {GalleryCount}";
        }
    }
}
