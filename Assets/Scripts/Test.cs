using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Test : MonoBehaviour {
    
    public TMP_InputField nameInput;
    public TMP_InputField passInput;
    public Button loginButton;
 
    void Start() {
        nameInput.text = "1ht234586";
        passInput.text = "Password@1";
        StartCoroutine(DelayedInput());
    }

    IEnumerator DelayedInput() {
        yield return new WaitForSeconds(2f);
        loginButton.onClick.Invoke();
    }

}
