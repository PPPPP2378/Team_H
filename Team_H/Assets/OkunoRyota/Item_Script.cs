using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "ScriptableObject/Create Item")]
public class Item : ScriptableObject
{
    // アイテムの名前（Object.name を隠す）
    public new string name = "New Item";

    // アイテムのアイコン
    public Sprite icon = null;

    // アイテムの使用
    public void Use()
    {
        Debug.Log(name + "を使用しました");
    }
}
