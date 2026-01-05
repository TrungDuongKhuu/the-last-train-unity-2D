using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MyClick : MonoBehaviour, IPointerClickHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SceneManager.LoadScene("BedroomScene", LoadSceneMode.Additive);
        SceneManager.UnloadSceneAsync("MenuScene");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
