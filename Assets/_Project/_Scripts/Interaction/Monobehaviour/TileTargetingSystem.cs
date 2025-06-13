using HairvestMoon.Core;
using HairvestMoon.Farming;
using HairvestMoon.Player;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace HairvestMoon.Interaction
{
    /// <summary>
    /// Handles tile targeting for interaction and highlighting (mouse only).
    /// </summary>
    public class TileTargetingSystem : MonoBehaviour, IBusListener
    {
        [Header("References")]
        [SerializeField] private Grid _grid;
        [SerializeField] private Tilemap _highlightTilemap;
        [SerializeField] private Tile _highlightTile;

        [Header("Settings")]
        [SerializeField] private float _footPositionYOffset = -0.25f;
        [SerializeField] private float _mouseTargetMaxDistance = 1.5f;
        [SerializeField] private bool _drawDebugGizmos = true;
        [SerializeField] private Color _gizmoColor = new(1, 1, 0, 0.4f);

        // Cached references
        private Player_Controller _player;
        private GameEventBus _eventBus;
        private bool _isInitialized = false;
        private FarmTileDataManager _farmTileData;
        
        public Func<Vector3Int, bool> TileIsValid;

        private Vector3Int? _currentTargetedTile;
        private Vector3Int? _lastHighlightedTile;

        public Vector3Int? CurrentTargetedTile => _currentTargetedTile;
        public Grid Grid => _grid;

        // --- Event Bus Registration ---
        public void RegisterBusListeners()
        {
            _eventBus = ServiceLocator.Get<GameEventBus>();
            _eventBus.GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            _player = ServiceLocator.Get<Player_Controller>();
            _farmTileData = ServiceLocator.Get<FarmTileDataManager>();
            TileIsValid = cell => _farmTileData.IsFarmTile(cell);
            _isInitialized = true;
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

            Vector3Int? candidate = GetMouseTargetTile(playerCell, footPos);

            // Filter by farm tile + delegate
            if (candidate.HasValue && TileIsValid != null && !TileIsValid(candidate.Value))
                return null;

            return candidate;
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

        // --- Debug Gizmos (optional) ---

        private void OnDrawGizmos()
        {
            if (!_drawDebugGizmos || _grid == null || !_isInitialized) return;

            Vector3 footPos = _player != null
                ? _player.Position + new Vector3(0, _footPositionYOffset, 0)
                : transform.position + new Vector3(0, _footPositionYOffset, 0);

            Vector3Int playerCell = _grid.WorldToCell(footPos);

            Gizmos.color = _gizmoColor;
            // Optionally, draw a square of selectable tiles
            for (int dx = -1; dx <= 1; dx++)
                for (int dy = -1; dy <= 1; dy++)
                    Gizmos.DrawCube(_grid.GetCellCenterWorld(playerCell + new Vector3Int(dx, dy, 0)), Vector3.one * 0.8f);
        }
    }
}
