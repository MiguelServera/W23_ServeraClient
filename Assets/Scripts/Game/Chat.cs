using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Chat : MonoBehaviour
{
    public Button button;
    public Player player;
    public InputField messageInput;
    public Text messageOnChat;
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    void Update()
    {
        if(messageInput.text.Equals(""))
        {
            button.interactable = false;
        }
        else
        {
            button.interactable = true;
        }
    }
    public void onPostButtonClicked()
    {
        StartCoroutine(tryPost());
        StartCoroutine(tryGet());
    }

    public void onGetButtonClicked()
    {
        StartCoroutine(tryGet());
    }

    public IEnumerator tryPost()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Message/RegisterMessage", "POST");

        MessageSerializable newMessage = new MessageSerializable();
        newMessage.Id = player.Id;
        newMessage.Message = messageInput.text;

        string jsonData = JsonUtility.ToJson(newMessage);
        byte[] dataToSend = Encoding.UTF8.GetBytes(jsonData);
        httpClient.uploadHandler = new UploadHandlerRaw(dataToSend);
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.SetRequestHeader("Content-Type", "application/json");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("OnPostButtonClick: Error > " + httpClient.error);
        }

        httpClient.Dispose();
    }

    public IEnumerator tryGet()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Message/GetMessage", "GET");

        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SetRequestHeader("Accept", "application/json");
        httpClient.certificateHandler = new ByPassCertificate();
        httpClient.downloadHandler = new DownloadHandlerBuffer();

        yield return httpClient.SendWebRequest();

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Chat > GetMessage: " + httpClient.error);
        }
        else
        {
            string jsonResponse = httpClient.downloadHandler.text;
            MessageSerializable messageJson = JsonUtility.FromJson<MessageSerializable>(jsonResponse);
            messageOnChat.text = messageOnChat.text + "\n" + messageJson.Id.Substring(0, 3) + ">" + messageJson.Message;
        }

        httpClient.Dispose();
    }
}

