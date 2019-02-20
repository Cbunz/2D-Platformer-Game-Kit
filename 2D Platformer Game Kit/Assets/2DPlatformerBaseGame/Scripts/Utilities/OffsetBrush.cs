using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/*
namespace UnityEditor
{
    [CreateAssetMenu]
    [CustomGridBrush(false, true, true, "Offset Brush")]
    public class OffsetBrush : GridBrush
    {
        private ArrayList locations;
        private ArrayList tiles;

        private ArrayList Locations
        {
            get
            {
                if (locations == null)
                    locations = new ArrayList();
                return locations;
            }
        }

        private ArrayList Tiles
        {
            get
            {
                if (tiles == null)
                    tiles = new ArrayList();
                return tiles;
            }
        }

        public override void BoxFill(GridLayout gridLayout, GameObject brushTarget, BoundsInt position)
        {
            if (brushTarget == null)
                return;

            Tilemap map = brushTarget.GetComponent<Tilemap>();
            if (map == null)
                return;

            Locations.Clear();
            Tiles.Clear();

            foreach (Vector3Int location in position.allPositionsWithin)
            {
                Vector3Int local = location - position.min;
                BrushCell cell = cells[GetCellIndexWrapAround(local.x, local.y, local.z)];
                if (cell.tile == null)
                    continue;

                //if (local.x % 2 == 0 || local.y % 2 == 0 || local.z % 2 == 0)
                //    continue;

                Locations.Add(location);
                Tiles.Add(cell.tile);
            }

            map.SetTiles((Vector3Int[])Locations.ToArray(typeof(Vector3Int)), (TileBase[])Tiles.ToArray(typeof(TileBase)));

            foreach (Vector3Int location in position.allPositionsWithin)
            {
                Vector3Int local = location - position.min;
                BrushCell cell = cells[GetCellIndexWrapAround(local.x, local.y, local.z)];
                if (cell.tile == null)
                    continue;

                map.SetTransformMatrix(location, cell.matrix);
                map.SetColor(location, cell.color);
            }
        }
    }
}
*/