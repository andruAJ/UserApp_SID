using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;

public class AuthHandler : MonoBehaviour
{
    public string Token { get; set; }
    public string Username { get; set; }


    private string apiUrl = "https://sid-restapi.onrender.com";                   

    private UIDocument uiDocument;
    private VisualElement loginCard;
    private Button loginButton;
    private TextField usernameField;
    private TextField passwordField;
    private Button registrarse;                  //este es el botón para cambiar de carta

    private VisualElement scoreTable;

    private VisualElement signupCard;
    private Button signupButton;
    private TextField signupUsernameField;
    private TextField signupPasswordField;
    private Button iniciarSesion;               //este es el botón para cambiar de carta

    private User[] usuariosTop = new User[5] {
    new User { username = "", data = new UserData { score = int.MinValue } },
    new User { username = "", data = new UserData { score = int.MinValue } },
    new User { username = "", data = new UserData { score = int.MinValue } },
    new User { username = "", data = new UserData { score = int.MinValue } },
    new User { username = "", data = new UserData { score = int.MinValue } }
};


    void Start()
    {
        uiDocument = GetComponent<UIDocument>();
        loginCard = uiDocument.rootVisualElement.Q<VisualElement>("LogIn_Card");
        loginButton = loginCard.Q<Button>("LogIn_Button");
        usernameField = loginCard.Q<TextField>("Username_TextField");
        passwordField = loginCard.Q<TextField>("Password_TextField");
        scoreTable = uiDocument.rootVisualElement.Q<VisualElement>("ScoreTable");
        signupCard = uiDocument.rootVisualElement.Q<VisualElement>("SignUp_Card");
        signupButton = signupCard.Q<Button>("SignUp_Button");
        signupUsernameField = signupCard.Q<TextField>("Username_Register_TextField");
        signupPasswordField = signupCard.Q<TextField>("Password_Register_TextField");
        registrarse = loginCard.Q<Button>("Registrarse_Button");
        iniciarSesion = signupCard.Q<Button>("TienesCuenta_Button");

        Token = PlayerPrefs.GetString("token", "");
        Username = PlayerPrefs.GetString("username", "");

        loginButton.RegisterCallback<ClickEvent>(ev => Login());
        signupButton.RegisterCallback<ClickEvent>(ev => SignIn());
        registrarse.RegisterCallback<ClickEvent>(ev => { loginCard.style.display = DisplayStyle.None; signupCard.style.display = DisplayStyle.Flex; });
        iniciarSesion.RegisterCallback<ClickEvent>(ev => { signupCard.style.display = DisplayStyle.None; loginCard.style.display = DisplayStyle.Flex; });

        if (!string.IsNullOrEmpty(Token) && !string.IsNullOrEmpty(Username))
        {
            StartCoroutine(GetProfile());
        }
        else
        {
            Debug.Log("No token found, please log in.");
        }
    }

    ///////////////////////////////////////////////////////Métodos para SignIn////////////////////////////////////////////////////////

    public void SignIn()
    {
        Debug.Log("SignIn button clicked");
        if (uiDocument)
        {
            string username = signupUsernameField.text;
            string password = signupPasswordField.text;
            StartCoroutine(SignInCoroutine(username, password));
        }
        else { Debug.LogError("UIDocument not found!"); }
    }

    private IEnumerator SignInCoroutine(string username, string password)
    {

        string jsonData = JsonUtility.ToJson(new AuthData { username = username, password = password });

        string url = apiUrl + "/api/usuarios";

        UnityWebRequest www = UnityWebRequest.Post(url, jsonData, "application/json");

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("SignIn  successful");

            //aquí ya creo que cree el usuario

            signupCard.style.display = DisplayStyle.None; loginCard.style.display = DisplayStyle.Flex;
        }
        else
        {
            Debug.LogError("SignIn failed: " + www.error);
        }
    }

    ///////////////////////////////////////////////////////Métodos para LogIn////////////////////////////////////////////////////////
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

    ///////////////////////////////////////////////////////Métodos para autenticar////////////////////////////////////////////////////////

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
        }
        else
        {
            Debug.LogError("Login failed: " + www.error);
        }
    }

    ///////////////////////////////////////////////////////Métodos para acttualizar la data del score////////////////////////////////////////////////////////

    private IEnumerator UpdateData(int newScore)
    {
        return null;
    }


    ///////////////////////////////////////////////////////Métodos para mostrar la tabla de puntuaciones////////////////////////////////////////////////////////

    private IEnumerator SetUIForUserLogged()
    {
        if (loginCard != null) { loginCard.style.display = DisplayStyle.None; scoreTable.style.display = DisplayStyle.Flex; Debug.Log("Hola, ya debería haberse prendido la tabla"); }
        else { Debug.LogError("Login card or score table not found!"); }
        StartCoroutine(ShowTopScores());

        string url = apiUrl + "/api/usuarios/";            //hay que agregar aquí el endpoint para obtener la tabla de puntuaciones (los mejores puntajes)
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Score table fetch successful");
            foreach (var user in usuariosTop)
            {
                Debug.Log($"Username: {user.username}, Score: {user.data.score}");
                scoreTable.Q<Label>("User1NameText").text = user.username;
                scoreTable.Q<Label>("User1ScoreText").text = user.data.score.ToString();
            }




            
        }
        else
        {
            Debug.LogError("Score table fetch failed: " + www.error);
        }  
    }

    ///////////////////////////////////////////////////////Métodos para mostrar los mejores puntajes////////////////////////////////////////////////////////

    private IEnumerator ShowTopScores()
    {
        // Implementar la lógica para mostrar los mejores puntajes

        string url = apiUrl + "/api/usuarios" + "?limit=100";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.Success) 
        {
            AuthResponse[] users = JsonUtility.FromJson<AuthResponse[]>(www.downloadHandler.text);

            var topUsers = users
            .Select(u => u.usuario)
            .Where(u => u.data != null)
            .OrderByDescending(u => u.data.score)
            .Take(5)
            .ToArray();

            usuariosTop = topUsers;

            for (int i = 0; i < usuariosTop.Length; i++)
            {
                Debug.Log($"Top {i + 1}: {usuariosTop[i].username} - {usuariosTop[i].data.score}");
            }
        }
        else
        {
            Debug.LogError("Score table fetch failed: " + www.error);
        }
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
