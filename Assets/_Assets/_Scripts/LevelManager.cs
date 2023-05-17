using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [Header("Car")]
    [SerializeField] private GameObject Cars;
    [SerializeField] private Color[] carColorList;
    
    [HideInInspector] public List<GameObject> CarList;

    private UIManager UIManager;
    private AudioManager audioManager;

    private void Awake()
    {
        UIManager = FindObjectOfType<UIManager>(); // Initialize the reference to UIManager
        audioManager = FindObjectOfType<AudioManager>(); // Initialize the reference to AudioManager
        CarList = new List<GameObject>(); // Initialize the CarList

        AddCars();
    }

    private void AddCars()
    {
        int carColorListLength = carColorList.Length;

        foreach (Transform child in Cars.transform)
        {
            GameObject grandChild = child.GetChild(0).gameObject;
            Renderer grandChildRenderer = grandChild.GetComponent<Renderer>();

            grandChildRenderer.material.color = carColorList[Random.Range(0, carColorListLength)];
            CarList.Add(child.gameObject);
        }
    }

    public void LevelCompleted()
    {
        Cars.SetActive(false);
        StartCoroutine(ChangeSceneSmoothly());
    }

    private IEnumerator ChangeSceneSmoothly()
    {
        audioManager.PlayOneShotSFX("Win");

        yield return UIManager.FadeOut(); // Fade out

        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
            yield return UIManager.FadeIn(); // Fade in
        }
        else
        {
            SceneManager.LoadScene(0);
            Debug.LogWarning("No more scenes!");
        }
    }
}
