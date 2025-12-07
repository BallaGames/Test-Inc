using Balla.Input;
using Heathen.SteamworksIntegration;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendsMenu : MonoBehaviour
{
    public static FriendsMenu Instance;


    public CanvasGroup trans;
    public RectTransform root;
    public RectTransform thisGameRoot, otherRoot;
    public float menuOpenTime;
    float menuTime;
    public AnimationCurve menuOpenCurve;
    public AnimationCurve menuCloseCurve;
    public Vector3 openPosition, closePosition;

    public List<FriendMenuEntry> friendEntries;
    public FriendMenuEntry FME_Prefab;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            return;
        }
        LocalSteamData.OnInitialise += Initialise;
        PlayerInput.OnPauseChanged += GamePaused;
    }

    void Initialise()
    {
        GenerateFriends();
        StartCoroutine(ToggleMenu(false));
        StartCoroutine(UpdateFriends());
    }
    IEnumerator UpdateFriends()
    {
        while(true)
        {
            GenerateFriends();
            yield return new WaitForSeconds(10);
        }
    }
    void GamePaused(bool paused)
    {
        StartCoroutine(ToggleMenu(paused));
    }
    IEnumerator ToggleMenu(bool paused)
    {
        float speed = 1 / menuOpenTime;
        if (paused)
        {
            trans.alpha = 1;
        }
        menuTime = 0;
        while (menuTime < 1)
        {
            menuTime += speed * Time.unscaledDeltaTime;
            if (paused)
            {
                root.anchoredPosition = Vector2.Lerp(closePosition, openPosition, menuOpenCurve.Evaluate(menuTime));
            }
            else
            {
                root.anchoredPosition = Vector2.Lerp(openPosition, closePosition, menuCloseCurve.Evaluate(menuTime));
            }
            yield return null;
        }
        if (!paused)
        {
            trans.alpha = 0;
        }
    }


    public void GenerateFriends()
    {
        UserData[] myFriends = UserData.MyFriends;
        if(friendEntries.Count > 0)
        {
            for (int i = 0; i < friendEntries.Count; i++)
            {
                Destroy(friendEntries[i].gameObject);
            }
            friendEntries.Clear();
        }
        if(myFriends.Length < 1)
        {
            return;
        }
        foreach (var item in myFriends)
        {
            if(item.InThisGame)
            {
                CreateFriendEntry(item);
                continue;
            }
            if (item.InGame)
            {
                CreateFriendEntry(item);
                continue;
            }
            if(item.State != EPersonaState.k_EPersonaStateOffline)
            {
                CreateFriendEntry(item);
                continue;
            }
        }
    }
    void CreateFriendEntry(UserData item)
    {
        FriendMenuEntry fme = Instantiate(FME_Prefab, thisGameRoot);
        fme.Initialise(item);
        friendEntries.Add(fme);
    }
}
