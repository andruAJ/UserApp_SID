using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AuthHandler : MonoBehaviour
{
    public string Token { get; set; }
    public string Username { get; set; }


    private string apiUrl = "https://sid-restapi.onrender.com";                   //Crear el servidor y cambiar la URL


    private UIDocument uiDocument;
    private VisualElement loginCard;
    private Button loginButton;
    private TextField usernameField;
    private TextField passwordField;
    private VisualElement scoreTable;


    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        loginCard = uiDocument.rootVisualElement.Q<VisualElement>("LogIn_Card");
        loginButton = loginCard.Q<Button>("LogIn_Button");
        usernameField = loginCard.Q<TextField>("Username_TextField");
        passwordField = loginCard.Q<TextField>("Password_TextField");
        scoreTable = uiDocument.rootVisualElement.Q<VisualElement>("ScoreTable");
        Token = PlayerPrefs.GetString("token", "");
        Username = PlayerPrefs.GetString("username", "");

        loginButton.RegisterCallback<ClickEvent>(ev => Login());

        if (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Username))
        {
            StartCoroutine(GetProfile());
        }
        else
        {
            Debug.Log("No token found, please log in.");
        }
    }
    public void Login()
    {
        Debug.Log("Login button clicked");
        if (uiDocument) 
        {
            string username = usernameField.text;
            string password = passwordField.text;
            StartCoroutine(LoginCoroutine(username, password));
        }
        else {Debug.LogError("UIDocument not found!");}
    }

    private IEnumerator LoginCoroutine(string username, string password)
    {

        string jsonData = JsonUtility.ToJson(new AuthData { username = username, password = password });

        string url = apiUrl + "/api/auth/login";

        UnityWebRequest www = UnityWebRequest.Post(url, jsonData, "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login successful");
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);

            Token = response.token;
            Username = response.usuario.username;

            PlayerPrefs.SetString("token", Token);
            PlayerPrefs.SetString("username", Username);
            Debug.Log("Token and username saved to PlayerPrefs for " + Username);

            SetUIForUserLogged();
        }
        else
        {
            Debug.LogError("Login failed: " + www.error);
        }
    }

    private IEnumerator GetProfile()
    {
        Debug.Log("Fetching profile for user: " + Username);
        string url = apiUrl + "/api/usuarios/" + Username;

        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Login (score) successful");
            AuthResponse response = JsonUtility.FromJson<AuthResponse>(www.downloadHandler.text);

            SetUIForUserLogged();

            Debug.Log("Username: " + response.usuario.username);        
            Debug.Log("Score: " + response.usuario.data.score);

            scoreTable.Q<Label>("User1NameText").text = response.usuario.username;
            scoreTable.Q<Label>("User1ScoreText").text = response.usuario.data.score.ToString();
            //Poner los datos del otro usuario si el profe lo pone


        }
        else
        {
            Debug.LogError("Login failed: " + www.error);
        }
    }
    public void SetUIForUserLogged()
    {
        if (loginCard != null) { loginCard.style.display = DisplayStyle.None; scoreTable.style.display = DisplayStyle.Flex; }
    }
}
class AuthData
{
    public string username;
    public string password;
}

[System.Serializable]
class AuthResponse
{
    public User usuario;
    public string token;
}
[System.Serializable]
class User
{
    public string _id;
    public string username;
    public UserData data;
}
[System.Serializable]
class UserData
{
    public int score;
}
