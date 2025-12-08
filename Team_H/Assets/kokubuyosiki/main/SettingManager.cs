using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; // 追加

public class SettingsManager : MonoBehaviour
{
    // 既存の変数
    public AudioMixer audioMixer;
    public Slider masterVolumeSlider;

    // ⭐ 追加: テキストコンポーネントの参照
    public TextMeshProUGUI masterVolumeText;
    // ※ TextMeshProUGUIは、Canvasの子要素として作成されたTMPテキストのコンポーネント名です。

    private const string MASTER_VOL_PARAM = "MasterVolume";
    private const string MASTER_VOL_KEY = "MasterVol";


    void Start()
    {
        LoadVolume();

        // スライダーのリスナーを設定
       // masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        // ⭐ 追加: 初回起動時にテキストを更新
        //UpdateVolumeText(masterVolumeSlider.value);
    }

    // スライダーの値 (0.0f ~ 1.0f) を受け取り、音量を設定する関数
    public void SetMasterVolume(float volume)
    {
        // 既存の音量設定処理（変更なし）
        float volumeInDb = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
        audioMixer.SetFloat(MASTER_VOL_PARAM, volumeInDb);
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, volume);
        PlayerPrefs.Save();

        // ⭐ 追加: テキスト更新関数を呼び出す
        UpdateVolumeText(volume);
    }

    // ⭐ 追加: テキストを更新する関数
    private void UpdateVolumeText(float volume)
    {
        // volume (0.0f～1.0f) を 0～100 の整数に変換
        int percentage = Mathf.RoundToInt(volume * 100f);

        // TextMeshProUGUIに文字列を設定
       // masterVolumeText.text = percentage.ToString() + "%";
    }

    void LoadVolume()
    {
        float savedVolume = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1.0f);
       // masterVolumeSlider.value = savedVolume;
        SetMasterVolume(savedVolume);
        // SetMasterVolume内ですでに UpdateVolumeText(volume) が呼ばれるため、ここではコメントアウトでもOK
    }
}