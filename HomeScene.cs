using TMPro;
using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using Unity.Services.Core;
using UnityEngine.UI;
using UnityEngine;

public class HomeScene : MonoBehaviour
{
    [SerializeField] private Button loginButton;

    [SerializeField] private TMP_Text userIdText;
    [SerializeField] private TMP_Text userNameText;


    private PlayerInfo playerInfo;


    private async void Awake()
    {    
        await UnityServices.InitializeAsync();
        PlayerAccountService.Instance.SignedIn += SignedIn;
        Debug.Log("frr?");
    }

    private async void SignedIn()
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await SignInWithUnityAsync(accessToken);

        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }

    public async Task InitSignIn()
    {
        await PlayerAccountService.Instance.StartSignInAsync();
    }

    private async Task SignInWithUnityAsync(string accessToken)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            Debug.Log("SignIn is successful.");

            playerInfo = AuthenticationService.Instance.PlayerInfo;

            var name = await AuthenticationService.Instance.GetPlayerNameAsync();

            Debug.Log("PLAYER INFO : " + playerInfo);
            Debug.Log("PLAYER NAME : " + playerInfo);


            userIdText.text = $"id_{playerInfo.Id}";
            userNameText.text = name;
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    private void OnDestroy()
    {
        PlayerAccountService.Instance.SignedIn -= SignedIn;
    }

    public  void LoginButtonPressed()
    {
        Debug.Log("zerzer");
        InitSignIn();
    }


}