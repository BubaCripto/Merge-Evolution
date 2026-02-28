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
        public Text comboText;
        public Text bestText;
        public Text resultText;

        public bool CanInteract => _state == MatchState.Playing;

        private int _movesLeft;
        private int _score;
        private int _comboStreak;
        private int _bestScore;
        private MatchState _state;

        private const string BestScoreKey = "merge_best_score";
        private static readonly int[] BaseScoreByLevel = { 10, 20, 50, 120, 300, 800, 2000 };

        private enum MatchState
        {
            Playing,
            Ended
        }

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
            StartMatch();
        }

        private void Update()
        {
            if (_state == MatchState.Ended && Input.GetKeyDown(KeyCode.R))
            {
                RestartMatch();
            }
        }

        private void StartMatch()
        {
            _movesLeft = startMoves;
            _score = 0;
            _comboStreak = 0;
            _state = MatchState.Playing;
            _bestScore = PlayerPrefs.GetInt(BestScoreKey, 0);

            ClearBoardPieces();
            SpawnInitialPieces();
            if (resultText != null) resultText.text = string.Empty;
            RefreshHud();
        }

        private void RestartMatch()
        {
            StartMatch();
        }

        private void ClearBoardPieces()
        {
            foreach (var cell in boardGrid.AllCells)
            {
                if (cell.currentPiece != null)
                {
                    Destroy(cell.currentPiece.gameObject);
                    cell.ClearPiece();
                }
            }
        }

        private void EnsureHud()
        {
            if (scoreText != null && movesText != null && goalText != null) return;

            var canvasGo = new GameObject("HUD_Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            scoreText = CreateHudText(canvas.transform, "ScoreText", new Vector2(42, -54));
            movesText = CreateHudText(canvas.transform, "MovesText", new Vector2(42, -106));
            goalText = CreateHudText(canvas.transform, "GoalText", new Vector2(42, -158));
            comboText = CreateHudText(canvas.transform, "ComboText", new Vector2(42, -210));
            bestText = CreateHudText(canvas.transform, "BestText", new Vector2(42, -262));
            resultText = CreateHudText(canvas.transform, "ResultText", new Vector2(42, -340), 34);
        }

        private Text CreateHudText(Transform parent, string name, Vector2 anchoredPos, int size = 30)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = anchoredPos;
            rect.sizeDelta = new Vector2(1000, 60);

            var txt = go.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txt.fontSize = size;
            txt.color = Color.white;
            txt.alignment = TextAnchor.MiddleLeft;
            return txt;
        }

        private void SpawnInitialPieces()
        {
            var count = Mathf.Min(initialPieces, boardGrid.GetEmptyCells().Count);
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
            if (_state != MatchState.Playing)
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
                _comboStreak = 0;
            }
            else
            {
                var targetPiece = targetCell.currentPiece;
                if (targetPiece != null && targetPiece.level == draggedPiece.level && targetPiece.level < 7)
                {
                    targetPiece.SetLevel(targetPiece.level + 1);

                    Destroy(draggedPiece.gameObject);
                    draggedPiece.dragStartCell = null;
                    mergeHappened = true;

                    RegisterMergeScore(targetPiece.level);
                }
                else
                {
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

            var comboBonus = _comboStreak switch
            {
                1 => 0.10f,
                2 => 0.20f,
                3 => 0.40f,
                _ => 0.75f
            };

            var baseScore = BaseScoreByLevel[Mathf.Clamp(resultLevel - 1, 0, BaseScoreByLevel.Length - 1)];
            var gained = Mathf.RoundToInt(baseScore * (1f + comboBonus));
            _score += gained;

            if (_score > _bestScore)
            {
                _bestScore = _score;
                PlayerPrefs.SetInt(BestScoreKey, _bestScore);
                PlayerPrefs.Save();
            }
        }

        private void CheckEndGame()
        {
            if (_movesLeft > 0) return;

            _state = MatchState.Ended;

            var stars = CalculateStars();
            if (_score >= goalScore)
            {
                resultText.text = $"Vitória! {stars}⭐  (R para reiniciar)";
                resultText.color = new Color(0.45f, 1f, 0.45f);
            }
            else
            {
                resultText.text = $"Fail! {stars}⭐  (R para reiniciar)";
                resultText.color = new Color(1f, 0.55f, 0.55f);
            }
        }

        private int CalculateStars()
        {
            if (_score >= goalScore) return 3;
            if (_score >= Mathf.RoundToInt(goalScore * 0.6f)) return 2;
            if (_score >= Mathf.RoundToInt(goalScore * 0.3f)) return 1;
            return 0;
        }

        private void RefreshHud()
        {
            if (scoreText != null) scoreText.text = $"Score: {_score}";
            if (movesText != null) movesText.text = $"Jogadas: {_movesLeft}";
            if (goalText != null) goalText.text = $"Meta 3⭐: {goalScore}";
            if (comboText != null) comboText.text = $"Combo: x{Mathf.Max(1, _comboStreak)}";
            if (bestText != null) bestText.text = $"Melhor: {_bestScore}";
        }
    }
}
