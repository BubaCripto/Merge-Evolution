using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MergeEvolution.Core
{
    public class MergeGameManager : MonoBehaviour
    {
        public static MergeGameManager Instance { get; private set; }

        [Header("Core")]
        public BoardGrid boardGrid;
        public int startMoves = 15;
        public int goalScore = 4000;
        public int initialPieces = 12;

        [Header("UI")]
        public Text scoreText;
        public Text movesText;
        public Text goalText;
        public Text resultText;

        private int _movesLeft;
        private int _score;
        private int _comboStreak;

        private static readonly int[] BaseScoreByLevel = { 10, 20, 50, 120, 300, 800, 2000 };

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            if (boardGrid == null)
            {
                boardGrid = FindFirstObjectByType<BoardGrid>();
            }

            if (boardGrid == null)
            {
                var boardGo = new GameObject("BoardGrid");
                boardGo.transform.position = Vector3.zero;
                boardGrid = boardGo.AddComponent<BoardGrid>();
            }

            EnsureHud();

            _movesLeft = startMoves;
            _score = 0;
            _comboStreak = 0;

            SpawnInitialPieces();
            RefreshHud();
        }

        private void EnsureHud()
        {
            if (scoreText != null && movesText != null && goalText != null) return;

            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            scoreText = CreateHudText(canvas.transform, "ScoreText", new Vector2(180, -60));
            movesText = CreateHudText(canvas.transform, "MovesText", new Vector2(180, -100));
            goalText = CreateHudText(canvas.transform, "GoalText", new Vector2(180, -140));
            resultText = CreateHudText(canvas.transform, "ResultText", new Vector2(350, -60), 28);
        }

        private Text CreateHudText(Transform parent, string name, Vector2 anchoredPos, int size = 24)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(900, 60);

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (txt.font == null)
            {
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            txt.fontSize = size;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            return txt;
        }

        private void SpawnInitialPieces()
        {
            var empties = boardGrid.GetEmptyCells();
            var count = Mathf.Min(initialPieces, empties.Count);

            for (var i = 0; i < count; i++)
            {
                SpawnRandomPiece();
            }
        }

        private int RollInitialLevel()
        {
            var r = Random.value;
            if (r < 0.70f) return 1;
            if (r < 0.95f) return 2;
            return 3;
        }

        private void SpawnRandomPiece()
        {
            var empties = boardGrid.GetEmptyCells();
            if (empties.Count == 0) return;

            var targetCell = empties[Random.Range(0, empties.Count)];

            var pieceGo = new GameObject("Piece");
            var piece = pieceGo.AddComponent<Piece>();
            piece.SetLevel(RollInitialLevel());

            targetCell.SetPiece(piece);
            piece.SnapToCell(targetCell);
        }

        public void HandleDrop(Piece draggedPiece)
        {
            if (_movesLeft <= 0)
            {
                draggedPiece.ReturnToStartCell();
                return;
            }

            var targetCell = boardGrid.GetClosestCell(draggedPiece.transform.position);
            var startCell = draggedPiece.dragStartCell;

            if (targetCell == null || startCell == null)
            {
                draggedPiece.ReturnToStartCell();
                return;
            }

            if (targetCell == startCell)
            {
                startCell.SetPiece(draggedPiece);
                draggedPiece.dragStartCell = null;
                return;
            }

            var mergeHappened = false;

            if (!targetCell.IsOccupied)
            {
                targetCell.SetPiece(draggedPiece);
                draggedPiece.dragStartCell = null;
            }
            else
            {
                var targetPiece = targetCell.currentPiece;
                if (targetPiece != null && targetPiece.level == draggedPiece.level && targetPiece.level < 7)
                {
                    // Merge de 2 iguais -> nível acima no alvo
                    targetPiece.SetLevel(targetPiece.level + 1);

                    Destroy(draggedPiece.gameObject);
                    draggedPiece.dragStartCell = null;
                    mergeHappened = true;

                    RegisterMergeScore(targetPiece.level);
                }
                else
                {
                    // Swap
                    startCell.SetPiece(targetPiece);
                    targetPiece.SnapToCell(startCell);

                    targetCell.SetPiece(draggedPiece);
                    draggedPiece.dragStartCell = null;
                    _comboStreak = 0;
                }
            }

            if (!mergeHappened && targetCell.IsOccupied && targetCell.currentPiece == draggedPiece)
            {
                _comboStreak = 0;
            }

            _movesLeft--;
            SpawnRandomPiece();
            RefreshHud();
            CheckEndGame();
        }

        private void RegisterMergeScore(int resultLevel)
        {
            _comboStreak++;

            float comboBonus = _comboStreak switch
            {
                1 => 0.10f,
                2 => 0.20f,
                3 => 0.40f,
                _ => 0.75f
            };

            var baseScore = BaseScoreByLevel[Mathf.Clamp(resultLevel - 1, 0, BaseScoreByLevel.Length - 1)];
            var gained = Mathf.RoundToInt(baseScore * (1f + comboBonus));
            _score += gained;
        }

        private void CheckEndGame()
        {
            if (_movesLeft > 0) return;

            if (_score >= goalScore)
            {
                resultText.text = "Vitória!";
                resultText.color = new Color(0.45f, 1f, 0.45f);
            }
            else
            {
                resultText.text = "Fail! Tente novamente.";
                resultText.color = new Color(1f, 0.55f, 0.55f);
            }
        }

        private void RefreshHud()
        {
            if (scoreText != null) scoreText.text = $"Score: {_score}";
            if (movesText != null) movesText.text = $"Jogadas: {_movesLeft}";
            if (goalText != null) goalText.text = $"Meta: {goalScore}";
        }
    }
}
