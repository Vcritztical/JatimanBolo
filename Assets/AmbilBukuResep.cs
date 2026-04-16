using UnityEngine;
using UnityEngine.UI;
using System;

public class AmbilBukuResep : MonoBehaviour
{
    // Deklarasi event statis.
    // 'string' adalah tipe argumen yang akan dikirim bersama event (nama kunci resep)
    public static event Action<string> OnRecipeClaimed;

    [Tooltip("Nama kunci dari GameProgressKeys yang sesuai dengan resep ini.")]
    public string unlockKeyName;

    [Tooltip("Tombol 'Ambil Resep' pada prefab ini.")]
    public Button claimButton;

    private bool isAlreadyClaimed = false;

    void Start()
    {
        // ... (logika Start tetap sama untuk validasi dan pengecekan PlayerPrefs awal)
        if (claimButton == null) { /*...*/ return; }
        if (string.IsNullOrEmpty(unlockKeyName)) { /*...*/ return; }

        //isAlreadyClaimed = PlayerPrefs.GetInt(unlockKeyName, 0) == 1;
        /*if (isAlreadyClaimed)
        {
            claimButton.interactable = false;
            Text buttonText = claimButton.GetComponentInChildren<Text>(true);
            if (buttonText != null) buttonText.text = "RESEP DIAMBIL";
        }*/
        else
        {
            claimButton.interactable = true;
            claimButton.onClick.AddListener(ProcessClaimKey);
        }
    }

    public void ProcessClaimKey()
    {
        if (isAlreadyClaimed) return;

        PlayerPrefs.SetInt(unlockKeyName, 1);
        PlayerPrefs.Save();

        //isAlreadyClaimed = true;
        //claimButton.interactable = false;
        Text buttonText = claimButton.GetComponentInChildren<Text>(true);
        if (buttonText != null) buttonText.text = "BERHASIL DIAMBIL";

        Debug.Log("Resep '" + unlockKeyName + "' berhasil diambil! Memicu event OnRecipeClaimed.");

        // PENTING: Panggil event dan kirimkan nama kunci resep yang baru saja diklaim.
        // Tanda tanya (?) adalah null-conditional operator, memastikan event hanya dipanggil jika ada subscriber.
        OnRecipeClaimed?.Invoke(unlockKeyName);

        // Opsional: Nonaktifkan prefab setelah diklaim
        // gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (claimButton != null)
        {
            claimButton.onClick.RemoveListener(ProcessClaimKey);
        }
    }
}
