using System;
using ScriptableObjects;
using Server;
using UnityEngine;

public class UserSessionHandler : MonoBehaviour
{
    public bool showDebugLogs = false;
    // Using Xampp's apache to create a localhost server and be able to debug PHP scripts in VS Code using XDebug (PHP debugger kernel)
    public bool useLocalHost = false; 
    public UserSO user;
    public User userData;

    public Action<UInt64> OnSessionStart;

    private const string apiUrlAddUser = "https://citmalumnes.upc.es/~brandonam/addUser.php/POST";
    private const string apiUrlAddSession = "https://citmalumnes.upc.es/~brandonam/addSession.php/POST";
    private const string localHostUrlAddUser = "http://localhost/delivery3/addUser.php";
    private const string localHostUrlAddSession = "http://localhost/delivery3/addSession.php";
    
    private void Start()
    {
        InitUser();
    }
    // It is called before OnDisable, because this when inactive cannot start coroutines
    private void OnApplicationQuit()
    {
        EndSession();
    }
    private void InitUser()
    {
        userData.Id = UInt64.MaxValue;
        userData.Name = user.Name;
        userData.Sex = user.Sex;
        userData.Age = user.Age;
        userData.Country = user.Country;
        userData.DateCreated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = userData.ToJson();
        Debug.LogWarning("InitUser");
        PHP_Sender.Instance.SendData(useLocalHost ? localHostUrlAddUser : apiUrlAddUser, json, OnUserLogin);
    }

    private void OnUserLogin(uint newId)
    {
        userData.Id = newId;
        InitSession();
    }

    public Session session;

    private void InitSession()
    {
        session.SessionId = UInt64.MaxValue;
        session.PlayerId = userData.Id;
        session.Start = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        session.End = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = session.ToJson();
        Debug.LogWarning("InitSession");
        PHP_Sender.Instance.SendData(useLocalHost ? localHostUrlAddSession : apiUrlAddSession, json, OnSessionLogin);
    }

    private void OnSessionLogin(uint newId)
    {
        session.SessionId = newId;
        OnSessionStart(newId);
    }

    private void EndSession()
    {
        if (session.SessionId == UInt64.MaxValue)
        {
            session.Start = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        session.PlayerId = userData.Id;
        session.End = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        string json = session.ToJson();
        Debug.LogWarning(json);
        PHP_Sender.Instance.SendData(useLocalHost ? localHostUrlAddSession : apiUrlAddSession, json, OnSessionLogout);
    }

    private void OnSessionLogout(uint newId)
    {
        Debug.LogWarning("Session has ended");
    }
}