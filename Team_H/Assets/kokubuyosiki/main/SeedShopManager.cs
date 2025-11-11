using UnityEngine;
using UnityEngine.UI;
using System.Collections; // コルーチンを使うために必要です

public class SeedShopManager : MonoBehaviour
{
    // === 1. UI要素の参照 (Inspectorで設定が必要) ===

    [Header("確認ダイアログ")]
    public GameObject confirmationPanel; // 確認パネルの親オブジェクト
    public Text messageText;           // 確認メッセージを表示するText

    [Header("購入通知メッセージ")]
    public GameObject notificationObject; // 通知メッセージの親オブジェクト (普段は非表示)
    public Text notificationText;         // 通知メッセージを表示するText
    public float displayDuration = 3f;    // メッセージの表示時間（秒）

    [Header("アイテムデータ")]
    public ItemData[] availableItems; // Inspectorで設定するアイテムリスト

    // === 2. 内部変数 ===
    private int currentItemID; // 現在選択中のアイテムIDを保持

    // 🚀 ゲーム開始時に実行される処理
    void Start()
    {
        // 確認パネルと通知メッセージを非表示にしておく
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
        if (notificationObject != null)
        {
            notificationObject.SetActive(false);
        }
    }

    // === 3. 各商品ボタンから呼ばれる関数 ===
    // 引数がintのみの関数 (OpenConfirmation(int))
    public void OpenConfirmation(int itemID)
    {
        // 配列の範囲チェック
        if (itemID < 0 || itemID >= availableItems.Length)
        {
            Debug.LogError("Invalid Item ID: " + itemID);
            return;
        }

        ItemData selectedItem = availableItems[itemID];

        currentItemID = itemID;

        // 確認メッセージを設定
        messageText.text = $"{selectedItem.itemName}を {selectedItem.price}G で購入しますか？";

        // 確認パネルを表示
        confirmationPanel.SetActive(true);
    }

    // === 4. 「YES (購入)」ボタンから呼ばれる関数 ===
    public void BuyItem()
    {
        // 実際にはここに「残金チェック」や「インベントリへのアイテム追加」処理が入ります

        string boughtItemName = availableItems[currentItemID].itemName;

        Debug.Log($"{boughtItemName}を購入しました！");

        // 1. 確認ダイアログを閉じる
        confirmationPanel.SetActive(false);

        // 2. 購入完了メッセージの表示を開始する
        StartCoroutine(ShowNotificationCoroutine($"{boughtItemName}を買いました！"));
    }

    // === 5. 「NO (キャンセル)」ボタンから呼ばれる関数 ===
    public void CloseConfirmation()
    {
        // 確認パネルを閉じる
        confirmationPanel.SetActive(false);
    }

    // === 6. コルーチン関数 (メッセージ表示用) ===
    private IEnumerator ShowNotificationCoroutine(string message)
    {
        // メッセージを設定し、表示する
        notificationText.text = message;
        notificationObject.SetActive(true);

        // 設定した秒数だけ待機する
        yield return new WaitForSeconds(displayDuration);

        // 待機後、メッセージを非表示にする
        notificationObject.SetActive(false);
    }
}
// ↑↑↑↑↑ SeedShopManager の閉じ括弧はここまで ↑↑↑↑↑

// 2. ItemData クラス（ファイルの末尾に記述）
[System.Serializable]
public class ItemData
{
    public string itemName;
    public int price;
}