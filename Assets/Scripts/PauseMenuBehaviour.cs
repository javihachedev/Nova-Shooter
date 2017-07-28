using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuBehaviour : MainMenuBehaviour
{

    public static PauseMenuBehaviour instance; // Singleton
    public static bool isPaused;
    
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public GameObject gameOverMenu;
    public Slider volumeSlider;

    void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        // Pause = false & values of menu should be stablished
        isPaused = false;
        pauseMenu.SetActive(false);
        optionsMenu.SetActive(false);
        gameOverMenu.SetActive(false);
        UpdateQualityLabel();
        UpdateVolumeLabel();
        volumeSlider.value = AudioListener.volume;
    }

    public void Update()
    {
        // Opens pause menu
        if (Input.GetKeyUp("escape"))
        {
            if (!optionsMenu.activeInHierarchy)
            {
                // If false becomes true and vice-versa
                isPaused = !isPaused;
                // Pauses the game (timeScale)
                Time.timeScale = (isPaused) ? 0 : 1;
                pauseMenu.SetActive(isPaused);
            }
            // Comes back to main pause menu if options menu is active
            else
            {
                OpenPauseMenu();
            }
        }
    }

    // Resumes the game
    public void ResumeGame()
    {
        isPaused = false;
        pauseMenu.SetActive(false);
        Time.timeScale = 1;
    }

    // Restarts the game
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        ResumeGame();
    }

    // Increase the quality in one level
    public void IncreaseQuality()
    {
        QualitySettings.IncreaseLevel();
        UpdateQualityLabel();
    }

    // Decreases the quality in one level
    public void DecreaseQuality()
    {
        QualitySettings.DecreaseLevel();
        UpdateQualityLabel();
    }

    // Sets the volume to a "value"
    public void SetVolume(float value)
    {
        AudioListener.volume = value;
        UpdateVolumeLabel();
    }

    // Updates the quality label shown on pause menu
    private void UpdateQualityLabel()
    {
        int currentQuality = QualitySettings.GetQualityLevel();
        string qualityName = QualitySettings.names[currentQuality];
        optionsMenu.transform.Find("Quality Level").GetComponent<UnityEngine.UI.Text>().text = "Quality Level - " + qualityName;
    }

    // Updates the volume label shown on pause menu
    private void UpdateVolumeLabel()
    {
        optionsMenu.transform.Find("Master Volume").GetComponent<UnityEngine.UI.Text>().text = "Master Volume - " + (AudioListener.volume * 100).ToString("f2") + "%";
    }

    // Opens the options menu
    public void OpenOptions()
    {
        optionsMenu.SetActive(true);
        pauseMenu.SetActive(false);
    }

    // Opens the pause menu
    public void OpenPauseMenu()
    {
        optionsMenu.SetActive(false);
        pauseMenu.SetActive(true);
    }

    // Opens the game over menu
    public void OpenGameOverMenu()
    {
        gameOverMenu.SetActive(true);
    }

}
