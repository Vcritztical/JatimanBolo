using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Advertisements;
using Watermelon; // Biar bisa akses Currencies
using System;

public class RewardedAdsButton : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [Header("Ads Settings")]
    [SerializeField] string _gameId = "6095955";
    [SerializeField] Button _showAdButton;
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";

    [Header("Reward & Cooldown")]
    [SerializeField] int _jumlahHadiah = 100;
    [SerializeField] float _menitTunggu = 5f; // Jeda antar iklan (menit)

    private string _adUnitId = null;
    private bool _isCooldown = false;
    private DateTime _nextReadyTime;

    void Awake()
    {
        // Setup ID Platform
#if UNITY_IOS
        _adUnitId = "Rewarded_iOS";
#else
        _adUnitId = _androidAdUnitId;
#endif

        // NYALAIN MESIN IKLAN
        if (!Advertisement.isInitialized)
        {
            Advertisement.Initialize(_gameId, true); // true = Test Mode
        }

        _showAdButton.interactable = false;
    }

    void Start()
    {
        // Jangan langsung LoadAd, tapi tunggu sampai mesinnya beneran Initialize
        StartCoroutine(WaitAndLoad());
    }

    System.Collections.IEnumerator WaitAndLoad()
    {
        // Tunggu sampai Advertisement.isInitialized jadi true
        while (!Advertisement.isInitialized)
        {
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log("Mesin Ads SIAP, mulai loading iklan...");
        LoadAd();
    }

    void Update()
    {
        // Cek apakah waktu nunggu sudah beres
        if (_isCooldown && DateTime.Now >= _nextReadyTime)
        {
            _isCooldown = false;
            Debug.Log("Jeda beres, loading iklan lagi...");
            LoadAd();
        }
    }

    public void LoadAd()
    {
        if (_isCooldown) return;

        Debug.Log("Loading Ad: " + _adUnitId);
        Advertisement.Load(_adUnitId, this);
    }

    public void ShowAd()
    {
        _showAdButton.interactable = false;
        Advertisement.Show(_adUnitId, this);
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        if (adUnitId.Equals(_adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
        {
            Debug.Log("Iklan Kelar! Dapet Rp " + _jumlahHadiah);

            // 1. KASIH DUIT KE SISTEM WATERMELON
            CurrenciesController.Add((CurrencyType)0, _jumlahHadiah);

            // 2. PASANG BATA (COOLDOWN)
            _isCooldown = true;
            _nextReadyTime = DateTime.Now.AddMinutes(_menitTunggu);
            _showAdButton.interactable = false;
        }
        else
        {
            // Kalau gagal/skip, coba load lagi biar tombol nyala
            LoadAd();
        }
    }

    // --- Listener Load (Penting biar tombol nyala otomatis) ---
    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        if (adUnitId.Equals(_adUnitId) && !_isCooldown)
        {
            _showAdButton.onClick.RemoveAllListeners();
            _showAdButton.onClick.AddListener(ShowAd);
            _showAdButton.interactable = true;
        }
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        if (!_isCooldown) Invoke("LoadAd", 5f);
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message) { LoadAd(); }
    public void OnUnityAdsShowStart(string adUnitId) { }
    public void OnUnityAdsShowClick(string adUnitId) { }
    void OnDestroy() { _showAdButton.onClick.RemoveAllListeners(); }
}