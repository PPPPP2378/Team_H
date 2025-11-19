using UnityEngine;
using UnityEngine.Audio; // AudioMixerを扱うために必要
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    // InspectorからAudioMixerを設定できるようにする
    public AudioMixer audioMixer;

    // スライダーを参照できるようにする (Hierarchyで設定)
    public Slider masterVolumeSlider;

    // AudioMixerで設定したExposed Parameterの名前
    private const string MASTER_VOL_PARAM = "MasterVolume";

    // PlayerPrefsに保存するキー
    private const string MASTER_VOL_KEY = "MasterVol";

    void Start()
    {
        // ゲーム開始時に保存された音量をロード
        LoadVolume();

        // スライダーのリスナーを設定 (スライダーが動いたときに呼ばれる関数)
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
    }

    // スライダーの値 (0.0f ~ 1.0f) を受け取り、音量を設定する関数
    public void SetMasterVolume(float volume)
    {
        // スライダーの値 (リニア) をAudioMixerで使えるデシベル値 (対数) に変換
        // volumeが最小(0.0001f)のとき、-80dBなど、音をほぼ聞こえなくする
        float volumeInDb = Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;

        // AudioMixerのパラメータを設定
        audioMixer.SetFloat(MASTER_VOL_PARAM, volumeInDb);

        // 設定を保存
        PlayerPrefs.SetFloat(MASTER_VOL_KEY, volume);
        PlayerPrefs.Save();
    }

    void LoadVolume()
    {
        // 保存された値があればロード、なければデフォルト値 (1.0f) を使う
        float savedVolume = PlayerPrefs.GetFloat(MASTER_VOL_KEY, 1.0f);

        // スライダーの値をロードした値に設定
        masterVolumeSlider.value = savedVolume;

        // ロードした値で音量を設定
        SetMasterVolume(savedVolume);
    }
}