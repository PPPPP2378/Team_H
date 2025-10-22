
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

    [Header ("���̏��")]
    public  Sprite plowedSprite;  // �k������̌�����
    public  Sprite wateredSprite; //�����
    public  Sprite seedSprite;�@�@//��A����
    public  Sprite grownSprite;  //������

    [Header("�ݒ�")]
    public float growTime = 5f;//��������
    private FarmState currentState = FarmState.Plow;

    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    //��Ԃɉ����ăX�v���C�g��ς���
    public void SetState(FarmState newState,Tilemap map,Vector3Int pos)
    {
        currentState = newState;

        //��Ԗ��Ɍ����ڕύX
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
