using HairvestMoon.Core;
using HairvestMoon.Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace HairvestMoon.Interaction
{
    /// <summary>
    /// Handles tile targeting for interaction and highlighting (mouse or stick/arc).
    /// Subscribes to facing changes for instant updates.
    /// </summary>
    public class TileTargetingSystem : MonoBehaviour, IBusListener
    {
        [Header("References")]
        [SerializeField] private Grid _grid;
        [SerializeField] private Tilemap _highlightTilemap;
        [SerializeField] private Tile _highlightTile;

        [Header("Settings")]
        [SerializeField] private int _highlightRange = 1;
        [SerializeField] private Vector2Int _coneSize = new(3, 1);
        [SerializeField] private float _footPositionYOffset = -0.25f;
        [SerializeField] private float _mouseTargetMaxDistance = 1.5f;
        [SerializeField] private bool _drawDebugGizmos = true;
        [SerializeField] private Color _gizmoColor = new(1, 1, 0, 0.4f);

        // Cached references
        private Player_Controller _player;
        private InputController _inputController;
        private PlayerFacingController _facingController;
        private GameEventBus _eventBus;
        private bool _isInitialized = false;

        private Vector3Int? _currentTargetedTile;
        private Vector3Int? _lastHighlightedTile;

        public Vector3Int? CurrentTargetedTile => _currentTargetedTile;
        public Grid Grid => _grid;

        // --- Event Bus Registration ---
        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
            _eventBus.FacingChanged += OnPlayerFacingChanged; // <-- subscribe here
        }

        private void OnGlobalSystemsInitialized()
        {
            _player = ServiceLocator.Get<Player_Controller>();
            _inputController = ServiceLocator.Get<InputController>();
            _facingController = ServiceLocator.Get<PlayerFacingController>();
            _isInitialized = true;
            RefreshHighlight();
        }

        private void OnPlayerFacingChanged(PlayerFacingController.FacingDirection newDir)
        {
            // Refresh highlight/arc as soon as facing changes
            RefreshHighlight();
        }

        // --- Core Update ---

        private void Update()
        {
            if (!_isInitialized) return;
            if (_grid == null || _highlightTilemap == null || _highlightTile == null) return;

            Vector3Int? newTarget = ComputeTargetTile();

            if (newTarget != _lastHighlightedTile)
            {
                if (_lastHighlightedTile.HasValue)
                    _highlightTilemap.SetTile(_lastHighlightedTile.Value, null);

                if (newTarget.HasValue)
                    _highlightTilemap.SetTile(newTarget.Value, _highlightTile);

                _lastHighlightedTile = newTarget;
            }

            _currentTargetedTile = newTarget;
        }

        private void RefreshHighlight()
        {
            // Forces a highlight refresh on next Update
            _lastHighlightedTile = null;
        }

        private Vector3Int? ComputeTargetTile()
        {
            Vector3 footPos = _player.Position + new Vector3(0, _footPositionYOffset, 0);
            Vector3Int playerCell = _grid.WorldToCell(footPos);

            if (_inputController.CurrentMode == ControlMode.Mouse)
                return GetMouseTargetTile(playerCell, footPos);
            else
                return GetArcTargetTile(playerCell, footPos);
        }

        private Vector3Int? GetMouseTargetTile(Vector3Int playerCell, Vector3 worldPos)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            mouseWorld.z = 0;
            Vector3Int cursorCell = _grid.WorldToCell(mouseWorld);
            Vector3Int offset = cursorCell - playerCell;

            if (Mathf.Abs(offset.x) <= 1 && Mathf.Abs(offset.y) <= 1)
            {
                float dist = Vector3.Distance(_grid.GetCellCenterWorld(cursorCell), worldPos);
                if (dist <= _mouseTargetMaxDistance)
                    return cursorCell;
            }
            return null;
        }

        private Vector3Int? GetArcTargetTile(Vector3Int origin, Vector3 originWorld)
        {
            var facing = _facingController.CurrentFacing;
            var arcTiles = GetArcTiles(origin, facing);

            Vector3Int? best = null;
            float bestScore = float.MaxValue;

            foreach (var cell in arcTiles)
            {
                float dist = Vector3.Distance(_grid.GetCellCenterWorld(cell), originWorld);
                if (dist < bestScore)
                {
                    best = cell;
                    bestScore = dist;
                }
            }
            return best;
        }

        private List<Vector3Int> GetArcTiles(Vector3Int origin, PlayerFacingController.FacingDirection facing)
        {
            List<Vector3Int> result = new();
            for (int depth = 1; depth <= _highlightRange; depth++)
            {
                for (int offset = -_coneSize.x / 2; offset <= _coneSize.x / 2; offset++)
                {
                    Vector3Int offsetVec = facing switch
                    {
                        PlayerFacingController.FacingDirection.Up => new(offset, depth, 0),
                        PlayerFacingController.FacingDirection.Down => new(offset, -depth, 0),
                        PlayerFacingController.FacingDirection.Left => new(-depth, offset, 0),
                        PlayerFacingController.FacingDirection.Right => new(depth, offset, 0),
                        _ => Vector3Int.zero
                    };
                    result.Add(origin + offsetVec);
                }
            }
            return result;
        }

        // --- Debug Gizmos (optional) ---

        private void OnDrawGizmos()
        {
            if (!_drawDebugGizmos || _grid == null || !_isInitialized) return;

            Vector3 footPos = _player != null
                ? _player.Position + new Vector3(0, _footPositionYOffset, 0)
                : transform.position + new Vector3(0, _footPositionYOffset, 0);

            Vector3Int playerCell = _grid.WorldToCell(footPos);
            var arcTiles = GetArcTiles(playerCell, _facingController != null ? _facingController.CurrentFacing : PlayerFacingController.FacingDirection.Right);

            Gizmos.color = _gizmoColor;
            foreach (var cell in arcTiles)
                Gizmos.DrawCube(_grid.GetCellCenterWorld(cell), Vector3.one * 0.8f);
        }
    }
}
