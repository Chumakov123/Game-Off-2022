using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapSplitting : MonoBehaviour
{
    public int segments;
    public bool isOriginal;
    private void Awake() {
        if (!isOriginal)
            return;
        isOriginal=false;
        var map = gameObject.GetComponent<Tilemap>();
        map.CompressBounds();
        var bounds = map.cellBounds;
        var len = Mathf.Abs(bounds.xMin-bounds.xMax)/segments;
        List<TileBase[]> tiles = new List<TileBase[]>();
        List<BoundsInt> boundsList = new List<BoundsInt>();
        for (int i = 0; i<segments; i++)
        {
            var b = bounds;
            var other = bounds;
            b.SetMinMax(new Vector3Int(b.xMin+len*i,b.yMin,b.zMin),new Vector3Int(b.xMin+len*(i+1),b.yMax,b.zMax));
            tiles.Add(map.GetTilesBlock(b));
            boundsList.Add(b);
        }
        map.ClearAllTiles();
        for (int i = 0; i<segments; i++)
        {
            var copy = Instantiate(transform);
            copy.SetParent(transform.parent);
            copy.name = name+" "+i;
            var smap = copy.GetComponent<Tilemap>();
            smap.SetTilesBlock(boundsList[i],tiles[i]);
            smap.CompressBounds();
        }
    }
}
