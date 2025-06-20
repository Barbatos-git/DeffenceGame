using UnityEngine;

public class HexTile : MonoBehaviour
{
    public int q;
    public int r;
    public HexTileType type;

    public void Init(int q, int r, HexTileType type)
    {
        this.q = q;
        this.r = r;
        SetType(type);
    }

    public void SetType(HexTileType newType)
    {
        type = newType;
        name = $"HexTile_{q}_{r}_{type}";
    }
}
