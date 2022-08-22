using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private string URL;

    [SerializeField] Text[] players;

    public GameObject gameOverCanvas;

    private string token;

    void Start()
    {
        token = PlayerPrefs.GetString("token");
        Time.timeScale = 1;
    }

    public void GameOver()
    {
        gameOverCanvas.SetActive(true);

        int actualScore = Score.score;

        ScoreData data = new ScoreData();

        if (actualScore > PlayerPrefs.GetInt("HighScore", 0))
        {
            PlayerPrefs.SetInt("HighScore", actualScore);

            data.username = PlayerPrefs.GetString("username");
            data.score = PlayerPrefs.GetInt("HighScore");

            string postData = JsonUtility.ToJson(data);

            StartCoroutine(ActualizarScore(postData));
        }

        Time.timeScale = 0;
    }
    public void Replay()
    {
        SceneManager.LoadScene("Game");
    }

    public void ClickGetScores()
    {
        StartCoroutine(ListarUsuarios());
    }

    IEnumerator ActualizarScore(string postData)
    {
        string url = URL + "/api/usuarios";
        UnityWebRequest www = UnityWebRequest.Put(url, postData);
        www.method = "PATCH";
        www.SetRequestHeader("Content-Type", "application/json");
        www.SetRequestHeader("x-token", token);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
        }
        else if (www.responseCode == 200)
        {
            AuthData resData = JsonUtility.FromJson<AuthData>(www.downloadHandler.text);

            Debug.Log("Nuevo record de " + resData.usuario.username + " Score: " + resData.usuario.score);
        }
        else
        {
            Debug.Log(www.error);
            Debug.Log(www.downloadHandler.text);
        }
    }

    IEnumerator ListarUsuarios()
    {
        string url = URL + "/api/usuarios?limit=5&sort=true";
        UnityWebRequest www = UnityWebRequest.Get(url);
        www.SetRequestHeader("x-token", token);

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR " + www.error);
        }
        else if (www.responseCode == 200)
        {
            Scores resData = JsonUtility.FromJson<Scores>(www.downloadHandler.text);

            for (int i = 0; i < resData.usuarios.Length; i++)
            {
                players[i].text = (i + 1) + ". " + resData.usuarios[i].username + " = " + resData.usuarios[i].score;
            }
        }
        else
        {
            Debug.Log(www.error);
        }
    }
}

[System.Serializable]
public class ScoreData
{
    public string username;
    public int score;
}

[System.Serializable]
public class Scores
{
    public UserData[] usuarios;
}
