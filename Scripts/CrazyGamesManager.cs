using UnityEngine;
using CrazyGames;
using System; // Required for Action

public class CrazyGamesManager : MonoBehaviour
{
    void Start()
    {
        CrazySDK.Init(() => {
            Debug.Log("CrazyGames SDK Initialized!");
        });
    }

    public void StartGameplay()
    {
        CrazySDK.Game.GameplayStart();
    }

    public void StopGameplay()
    {
        CrazySDK.Game.GameplayStop();
    }

    // --- UPDATED AND CORRECTED REWARDED AD METHOD ---
    public void ShowRewardedAd()
    {
        Debug.Log("Requesting a rewarded ad...");

        // Define the callback actions
        Action adFinishedCallback = () => {
            Debug.Log("Rewarded ad finished successfully! Granting reward.");
            GameManager.Instance.AddCoins(500);
        };

        Action<SdkError> adErrorCallback = (error) => {
            Debug.Log($"Rewarded ad failed to show. Error: {error.message}");
        };
        
        Action adStartedCallback = () => {
            Debug.Log("Ad has started playing.");
        };

        // This is the correct way to call RequestAd for a rewarded video.
        // It takes the ad type and the three callback actions as separate arguments.
        // And we use the correct name 'Rewarded' with a capital 'R'.
        CrazySDK.Ad.RequestAd(CrazyAdType.Rewarded, adStartedCallback, adErrorCallback, adFinishedCallback);
    }
}