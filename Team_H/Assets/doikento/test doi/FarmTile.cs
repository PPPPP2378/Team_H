
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using UnityEngine.InputSystem.XR.Haptics;

[CreateAssetMenu(menuName ="Tiles/FarmTile")]
public class FarmTile : Tile
{
    public enum FarmState
    {
        Plow,
        Watered,
        Seed,
        Grown
    }

    [Header ("畑の状態")]
    public  Sprite plowedSprite;  // 耕した後の見た目
    public  Sprite wateredSprite; //水やり
    public  Sprite seedSprite;　　//種植え後
    public  Sprite grownSprite;  //成長後

    [Header("設定")]
    public float growTime = 5f;//成長時間
    private FarmState currentState = FarmState.Plow;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    //状態に応じてスプライトを変える
    public void SetState(FarmState newState,Tilemap map,Vector3Int pos)
    {
        currentState = newState;

        //状態毎に見た目変更
        switch (currentState)
        {
            case FarmState.Plow:
                this.sprite = plowedSprite;
                break;
            case FarmState.Watered:
                this.sprite = wateredSprite;
                break;
            case FarmState.Seed:
                this.sprite = seedSprite;
                break;
            case FarmState.Grown:
                this.sprite = grownSprite;
                break;
        }
        map.RefreshTile(pos);
    }

    private IEnumerator GrowCoroutime(Tilemap map,Vector3Int pos)
    {
        yield return new WaitForSeconds(growTime);
        SetState(FarmState.Grown, map, pos);
    }
    public FarmState GetState() => currentState;
}
