using HairvestMoon.Core;
using HairvestMoon.Player;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

namespace HairvestMoon.Interaction
{
    public class TileTargetingSystem : MonoBehaviour, IBusListener
    {
        [Header("References")]
        public Grid Grid;
        public Tilemap selectionHighlightTilemap;
        public Tile highlightTile;

        [Header("Settings")]
        public int highlightRange = 1;
        public Vector2Int coneSize = new(3, 1);
        public float footPositionYOffset = -0.25f;
        public float mouseTargetMaxDistance = 1.5f;
        public bool drawDebugGizmos = true;
        public Color gizmoColor = new(1, 1, 0, 0.4f);

        public Vector3Int? CurrentTargetedTile => _currentTargetedTile;
        private Vector3Int? _currentTargetedTile;
        private Vector3Int _lastHighlighted;
        private bool isInitialized = false;

        public void RegisterBusListeners()
        {
            ServiceLocator.Get<GameEventBus>().GlobalSystemsInitialized += OnGlobalSystemsInitialized;
        }

        private void OnGlobalSystemsInitialized()
        {
            isInitialized = true;
        }

        private void Update()
        {
            if (!isInitialized) return;

            Vector3 footPos = ServiceLocator.Get<Player_Controller>().Position + new Vector3(0, footPositionYOffset, 0);
            Vector3Int playerCell = Grid.WorldToCell(footPos);

            Vector3Int? targetTile = ServiceLocator.Get<InputController>().CurrentMode == ControlMode.Mouse
                ? GetMouseTargetTile(playerCell, footPos)
                : GetArcTargetTile(playerCell, footPos);

            if (_lastHighlighted != null)
                selectionHighlightTilemap.SetTile(_lastHighlighted, null);

            if (targetTile.HasValue)
            {
                selectionHighlightTilemap.SetTile(targetTile.Value, highlightTile);
                _lastHighlighted = targetTile.Value;
                _currentTargetedTile = targetTile.Value;
            }
            else
            {
                _currentTargetedTile = null;
            }
        }

        private Vector3Int? GetMouseTargetTile(Vector3Int playerCell, Vector3 worldPos)
        {
            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
            mouseWorld.z = 0;
            Vector3Int cursorCell = Grid.WorldToCell(mouseWorld);
            Vector3Int offset = cursorCell - playerCell;

            if (Mathf.Abs(offset.x) <= 1 && Mathf.Abs(offset.y) <= 1)
            {
                float dist = Vector3.Distance(Grid.GetCellCenterWorld(cursorCell), worldPos);
                if (dist <= mouseTargetMaxDistance)
                    return cursorCell;
            }
            return null;
        }

        private Vector3Int? GetArcTargetTile(Vector3Int origin, Vector3 originWorld)
        {
            var facing = ServiceLocator.Get<PlayerFacingController>().CurrentFacing;
            var arcTiles = GetArcTiles(origin, facing);

            Vector3Int? best = null;
            float bestScore = float.MaxValue;

            foreach (var cell in arcTiles)
            {
                float dist = Vector3.Distance(Grid.GetCellCenterWorld(cell), originWorld);
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
            for (int depth = 1; depth <= highlightRange; depth++)
            {
                for (int offset = -coneSize.x / 2; offset <= coneSize.x / 2; offset++)
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
    }
}
