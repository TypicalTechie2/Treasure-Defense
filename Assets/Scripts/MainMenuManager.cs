using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button controlsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitGameButton;
    [SerializeField] private GameObject controlsImage;
    [SerializeField] private GameObject creditsImage;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1.5f;

    // Start is called before the first frame update
    void Start()
    {
        // Disable fade image at the start
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
    public void StartGame()
    {
        // Start fading and load the game scene
        StartCoroutine(FadeAndLoadScene("Game Scene"));
    }

    // Coroutine for fading effect and scene loading
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        // Activate the fade image
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
        }

        // Fade from transparent to black
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        // Load the new scene
        SceneManager.LoadScene(sceneName);

        // Fade from black to transparent
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        // Deactivate the fade image
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    // Method to show controls Image and Text
    public void Controls()
    {
        // Disable buttons
        startGameButton.interactable = false;
        controlsButton.interactable = false;
        creditsButton.interactable = false;
        exitGameButton.interactable = false;

        // Enable controls image
        controlsImage.SetActive(true);
    }

    // Method to exit control screen
    public void ExitControlScreen()
    {
        // Disable controls image and enable buttons
        controlsImage.SetActive(false);
        controlsButton.interactable = true;
        startGameButton.interactable = true;
        creditsButton.interactable = true;
        exitGameButton.interactable = true;
    }

    // Method to show credits Text and Image
    public void ShowCredits()
    {
        // Disable buttons while showing credits
        startGameButton.interactable = false;
        controlsButton.interactable = false;
        creditsButton.interactable = false;
        exitGameButton.interactable = false;
        // Enable credits image
        creditsImage.SetActive(true);
    }

    // Method to exit credits screen
    public void ExitCreditsScreen()
    {
        // Disable credits image and enable buttons
        creditsImage.SetActive(false);
        controlsButton.interactable = true;
        startGameButton.interactable = true;
        creditsButton.interactable = true;
        exitGameButton.interactable = true;
    }

    public void ExitGame()
    {
        // Quit the application
        Application.Quit();
    }
}
