// MenuUIManager.cs
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{
    [System.Serializable]
    public struct RecipeDisplayItem
    {
        public string keyToCheck;
        public GameObject lockImageObject;
    }

    [Header("Daftar Item Resep di Menu")]
    public RecipeDisplayItem[] recipeMenuItems = new RecipeDisplayItem[6];

    // Dipanggil ketika GameObject yang memiliki skrip ini menjadi aktif dan enabled
    void OnEnable()
    {
        // Mendaftarkan fungsi 'HandleRecipeClaimedEvent' untuk mendengarkan event 'OnRecipeClaimed'
        AmbilBukuResep.OnRecipeClaimed += HandleRecipeClaimedEvent;

        // Tetap refresh tampilan awal saat menu aktif, untuk menangani status dari PlayerPrefs
        RefreshMenuLocks();
        Debug.Log("MenuUIManager aktif dan mendengarkan event OnRecipeClaimed.");
    }

    // Dipanggil ketika GameObject yang memiliki skrip ini menjadi nonaktif atau dihancurkan
    void OnDisable()
    {
        // PENTING: Batalkan pendaftaran fungsi dari event untuk mencegah error dan memory leak
        AmbilBukuResep.OnRecipeClaimed -= HandleRecipeClaimedEvent;
        Debug.Log("MenuUIManager nonaktif, berhenti mendengarkan event OnRecipeClaimed.");
    }

    // Fungsi ini akan dipanggil SECARA OTOMATIS ketika event OnRecipeClaimed dipicu
    private void HandleRecipeClaimedEvent(string claimedKeyName)
    {
        Debug.Log("MenuUIManager menerima notifikasi: Resep '" + claimedKeyName + "' telah diklaim. Memperbarui UI.");
        // Setelah menerima notifikasi, kita bisa langsung me-refresh seluruh tampilan menu
        RefreshMenuLocks();

        // Alternatif (lebih optimal jika banyak item): hanya update item yang relevan
        /*
        foreach (RecipeDisplayItem item in recipeMenuItems)
        {
            if (item.keyToCheck == claimedKeyName)
            {
                if (item.lockImageObject != null)
                {
                    item.lockImageObject.SetActive(false); // Langsung buka gemboknya
                }
                Debug.Log("Gembok untuk '" + claimedKeyName + "' telah dibuka secara spesifik.");
                break; // Keluar dari loop karena item yang dimaksud sudah ditemukan
            }
        }
        */
    }

    // Fungsi ini tetap digunakan untuk menyegarkan semua gembok berdasarkan PlayerPrefs
    public void RefreshMenuLocks()
    {
        Debug.Log("Merefresh semua status gembok di menu...");
        foreach (RecipeDisplayItem item in recipeMenuItems)
        {
            if (string.IsNullOrEmpty(item.keyToCheck) || item.lockImageObject == null)
            {
                // Debug.LogWarning("Konfigurasi RecipeDisplayItem tidak lengkap.");
                continue;
            }
            bool isUnlocked = PlayerPrefs.GetInt(item.keyToCheck, 0) == 1;
            item.lockImageObject.SetActive(!isUnlocked);
        }
    }

    [ContextMenu("Hapus Semua Kunci Resep (Development)")]
    public void ResetAllRecipeKeys()
    {
        // ... (Fungsi ResetAllRecipeKeys tetap sama seperti sebelumnya)
        PlayerPrefs.DeleteKey(BukuResep.ResepSotoLamongan);
        PlayerPrefs.DeleteKey(BukuResep.ResepRujakCingur);
        PlayerPrefs.DeleteKey(BukuResep.ResepLontongBalap);
        PlayerPrefs.DeleteKey(BukuResep.ResepPecelMadiun);
        PlayerPrefs.DeleteKey(BukuResep.ResepRawon);
        PlayerPrefs.DeleteKey(BukuResep.ResepSateMadura);
        // ... (dst untuk semua kunci)
        PlayerPrefs.Save();
        RefreshMenuLocks();
        Debug.Log("Semua kunci resep telah dihapus dari PlayerPrefs.");
    }
}