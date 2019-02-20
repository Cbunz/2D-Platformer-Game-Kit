using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace UnityEngine
{
    public class RuleTile : TileBase
    {
        public Sprite defaultSprite;
        public Tile.ColliderType defaultColliderType;

        [Serializable]
        public class TilingRule
        {
            public Neighbor[] neighbors;
            public Sprite[] sprites;
            public float animationSpeed;
            public float perlinScale;
            public AutoTransform autoTransform;
            public OutputSprite output;
            public Tile.ColliderType colliderType;

            public TilingRule()
            {
                output = OutputSprite.Single;
                neighbors = new Neighbor[8];
                sprites = new Sprite[1];
                animationSpeed = 1f;
                perlinScale = 0.5f;
                colliderType = Tile.ColliderType.None;

                for (int i = 0; i < neighbors.Length; i++)
                    neighbors[i] = Neighbor.DontCare;
            }

            public enum AutoTransform { Fixed, Rotated, MirrorX, MirrorY }
            public enum Neighbor { DontCare, This, NotThis }
            public enum OutputSprite { Single, Random, Animation }
        }

        [HideInInspector] public List<TilingRule> tilingRules;

        public override void GetTileData(Vector3Int position, ITilemap tileMap, ref TileData tileData)
        {
            tileData.sprite = defaultSprite;
            tileData.colliderType = defaultColliderType;

            if (tilingRules.Count > 1)
            {
                tileData.flags = TileFlags.LockTransform;
                tileData.transform = Matrix4x4.identity;
            }

            foreach (TilingRule rule in tilingRules)
            {
                Matrix4x4 transform = Matrix4x4.identity;
                if (RuleMatches(rule, position, tileMap, ref transform))
                {
                    switch (rule.output)
                    {
                        case TilingRule.OutputSprite.Single:
                        case TilingRule.OutputSprite.Animation:
                            tileData.sprite = rule.sprites[0];
                            break;
                        case TilingRule.OutputSprite.Random:
                            int index = Mathf.Clamp(Mathf.RoundToInt(Mathf.PerlinNoise((position.x + 1000000f) * rule.perlinScale, (position.y + 1000000f) * rule.perlinScale) * rule.sprites.Length), 0, rule.sprites.Length - 1);
                            tileData.sprite = rule.sprites[index];
                            break;
                    }
                    tileData.transform = transform;
                    tileData.colliderType = rule.colliderType;
                    break;
                }
            }
        }

        public override bool GetTileAnimationData(Vector3Int position, ITilemap tilemap, ref TileAnimationData tileAnimationData)
        {
            foreach (TilingRule rule in tilingRules)
            {
                Matrix4x4 transform = Matrix4x4.identity;
                if (RuleMatches(rule, position, tilemap, ref transform) && rule.output == TilingRule.OutputSprite.Animation)
                {
                    tileAnimationData.animatedSprites = rule.sprites;
                    tileAnimationData.animationSpeed = rule.animationSpeed;
                    return true;
                }
            }
            return false;
        }

        public override void RefreshTile(Vector3Int location, ITilemap tileMap)
        {
            if (tilingRules != null && tilingRules.Count > 0)
            {
                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        base.RefreshTile(location + new Vector3Int(x, y, 0), tileMap);
                    }
                }
            }
            else
            {
                base.RefreshTile(location, tileMap);
            }
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
        {
            // Check rule against rotations of 0, 90, 180, 270
            for (int angle = 0; angle <= (rule.autoTransform == TilingRule.AutoTransform.Rotated ? 270 : 0); angle += 90)
            {
                if (RuleMatches(rule, position, tilemap, angle))
                {
                    transform = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0f, 0f, -angle), Vector3.one);
                    return true;
                }
            }

            // Check rule against x-axis mirror
            if ((rule.autoTransform == TilingRule.AutoTransform.MirrorX) && RuleMatches(rule, position, tilemap, true, false))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(-1f, 1f, 1f));
                return true;
            }

            // Check rule against y-axis mirror
            if ((rule.autoTransform == TilingRule.AutoTransform.MirrorY) && RuleMatches(rule, position, tilemap, false, true))
            {
                transform = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1f, -1f, 1f));
                return true;
            }

            return false;
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, int angle)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x != 0 || y != 0)
                    {
                        Vector3Int offset = new Vector3Int(x, y, 0);
                        Vector3Int rotated = GetRotatedPos(offset, angle);
                        int index = GetIndexOfOffset(rotated);
                        TileBase tile = tilemap.GetTile(position + offset);
                        if (rule.neighbors[index] == TilingRule.Neighbor.This && tile != this || rule.neighbors[index] == TilingRule.Neighbor.NotThis && tile == this)
                        {
                            return false;
                        }
                    }
                }

            }
            return true;
        }

        public bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, bool mirrorX, bool mirrorY)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    if (x != 0 || y != 0)
                    {
                        Vector3Int offset = new Vector3Int(x, y, 0);
                        Vector3Int mirrored = GetMirroredPos(offset, mirrorX, mirrorY);
                        int index = GetIndexOfOffset(mirrored);
                        TileBase tile = tilemap.GetTile(position + offset);
                        if (rule.neighbors[index] == TilingRule.Neighbor.This && tile != this || rule.neighbors[index] == TilingRule.Neighbor.NotThis && tile == this)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private int GetIndexOfOffset(Vector3Int offset)
        {
            int result = offset.x + 1 + (-offset.y + 1) * 3;
            if (result >= 4)
                result--;
            return result;
        }

        public Vector3Int GetRotatedPos(Vector3Int original, int rotation)
        {
            switch (rotation)
            {
                case 0:
                    return original;
                case 90:
                    return new Vector3Int(-original.y, original.x, original.z);
                case 180:
                    return new Vector3Int(-original.x, -original.y, original.z);
                case 270:
                    return new Vector3Int(original.y, -original.x, original.z);
            }
            return original;
        }

        public Vector3Int GetMirroredPos(Vector3Int original, bool mirrorX, bool mirrorY)
        {
            return new Vector3Int(original.x * (mirrorX ? -1 : 1), original.y * (mirrorY ? -1 : 1), original.z);
        }
    }
}

