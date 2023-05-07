using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Backend.Models;
using UnityEngine.Networking;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Security.Cryptography;
using TMPro;

public class UIManagerScript : MonoBehaviour {

    private static string URL = "https://backendcorefunctionapp.azurewebsites.net/api/StartCore";
    private static string rootBundleURL = "https://pancakestorageaccount.blob.core.windows.net/1ht234586";
    private static string bundleURL = "";

    public float WaitTime = 3.0f;
    public int stopPeriod = 10;

    [SerializeField]
    private TMP_InputField nameInput;
    [SerializeField]
    private TMP_InputField passwordInput;
    [SerializeField]
    private GameObject loginPanel;
    [SerializeField]
    private GameObject loadingPanel;
    [SerializeField]
    private GameObject displayPanel;
    [SerializeField]
    private TMP_Text displayText;
    [SerializeField]
    private TMP_Text waitingText;
    private DurableTask task;
    private Status status;
    private Output output;

    UnityWebRequest _request;

    public void ClickHandler() {
        var name = nameInput.text;
        var password = passwordInput.text;

        print($"Name: {name}, \n Password: {password} \n");

        
        var user = new User {
            Username = name,
            Password = password
        };
        loginPanel.SetActive(false);
        loadingPanel.SetActive(true);
        StartCoroutine(CheckState(user));
    }
    public IEnumerator SendRequest(User user) {
        if (user != null) {
            var request = new UnityWebRequest(URL, "POST");

            var serializer = new DataContractJsonSerializer(typeof(User));
            byte[] jsonBytes;
            string jsonString;

            // create a stream to hold the serialized data
            using (var stream = new MemoryStream()) {
                serializer.WriteObject(stream, user);

                jsonBytes = stream.ToArray();
                jsonString = Encoding.UTF8.GetString(jsonBytes);
            }
            Debug.Log($"User JSON: {jsonString}");

            // Set the request headers
            request.SetRequestHeader("Content-Type", "application/json");
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log(request.error);
                request.Dispose();
            }
            else {
                Debug.Log("Request sent successfully");
                var json = request.downloadHandler.text;
                Debug.Log(json);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                    var taskSerializer = new DataContractJsonSerializer(typeof(DurableTask));
                    task = taskSerializer.ReadObject(stream) as DurableTask;
                }

                Debug.Log($"Task: \n {task.id}");
                request.Dispose();
            }
        }
        else {
           Debug.Log("User Model was null.");
        }
       
    }

    public IEnumerator CheckState(User user) {
        yield return StartCoroutine(SendRequest(user));
        Debug.Log("Request Sent. Response Recieved.");
        yield return StartCoroutine(GetFunctionState(task));
        Debug.Log("Core execution complete.");
    }

    public IEnumerator GetFunctionState(DurableTask task) {
        if (task != null) {
            var request = new UnityWebRequest(task.statusQueryGetUri, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

            // Send the request and wait for a response
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log(request.error);
                request.Dispose();
            }
            else {
                Debug.Log("( Function State Query ) Request sent successfully");
                var json = request.downloadHandler.text;
                Debug.Log(json);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(json))) {
                    var statusSerializer = new DataContractJsonSerializer(typeof(Status));
                    status = statusSerializer.ReadObject(stream) as Status;
                }

                Debug.Log($"Task: \n {status.runtimeStatus}");
                while(status.runtimeStatus == "Running" || status.runtimeStatus == "Pending") {
                    request.Dispose();
                    yield return new WaitForSeconds(WaitTime);
                    StartCoroutine(GetFunctionState(task));
                }
                if(status.runtimeStatus == "Failed") {
                    request.Dispose();
                    Debug.Log("Something went wrong. Try again.");
                    yield return null;
                }
                if(status.runtimeStatus == "Completed") {
                    Debug.Log($"Output recieved: {status.output}");

                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(status.output))) {
                        var outputSerializer = new DataContractJsonSerializer(typeof(Output));
                        output = outputSerializer.ReadObject(stream) as Output;
                    }
                    var platform = "Android";

                    Debug.Log($"Output Location: \n {output.location}");
                    if (Application.platform == RuntimePlatform.WindowsEditor || 
                        Application.platform == RuntimePlatform.WindowsPlayer) {

                        Debug.Log("Platform: StandaloneWindows64/32");
                        platform = "StandaloneWindows64";
                    }
                    else if (Application.platform == RuntimePlatform.Android) {

                        Debug.Log("Platform: Android");
                        platform = "Android";
                    }

                    bundleURL = rootBundleURL + $"/{output.data.content[0]}-{platform}";
                    Debug.Log(bundleURL);

                    loadingPanel.SetActive(false);
                    displayPanel.SetActive(true);
                    displayText.text = format(output);
                }

                request.Dispose();
                for (int i = stopPeriod; i > 0; i--) {
                    waitingText.text = $"Your session will start in {i} seconds.";
                    Thread.Sleep(1000);
                }
                StartCoroutine(LoadBundleScene());
            }
        }
        else {
            Debug.Log("Task model was null.");
        }        
    }

    private string format(Output output) {
        string formatted = "";
        if (output != null) {
            formatted += "Status: ";
            formatted += output.state;
            formatted += "\n";
            formatted += "Location: ";
            formatted += output.location;
            formatted += "\n\n";

            formatted += "Department: ";
            formatted += output.data.department;
            formatted += "\n";
            formatted += "School: ";
            formatted += output.data.school;
            formatted += "\n\n";

            formatted += "Subjects: {";
            foreach (var sub in output.data.subjects) {
                formatted += sub + ", ";
            }
            formatted += "}\n";

            formatted += "Chapters: {";
            foreach (var chap in output.data.chapters) {
                formatted += chap + ", ";
            }
            formatted += "}\n";
        }
        return formatted;
    }

    IEnumerator LoadBundleScene() {
        using (UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(bundleURL)) {
            Debug.Log($"Trying to load bundles at: {bundleURL}");
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success) {
                Debug.Log(request.error);
                yield break;
            }

            AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

            string[] scenePaths = bundle.GetAllScenePaths();
            if (scenePaths.Length == 0) {
                Debug.Log("No scenes found in the asset bundle");
                yield break;
            }

            string scenePath = scenePaths[0];

            AsyncOperation operation = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(scenePath);
            yield return operation;
        }
    }

  
}
