using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider; // Reference to the slider UI element
    public Gradient gradient; // Gradient used to change the color of the health bar
    public Image healthFillColor; // Reference to the image component responsible for filling the health bar

    // Method to set the maximum health of the slider
    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;

        healthFillColor.color = gradient.Evaluate(1f);
    }

    // Method to update the current health of the slider
    public void SetHealth(int health)
    {
        slider.value = health; // Set the current value of the slider to the specified health value

        // Set the color of the health bar based on the normalized value of the slider
        // Normalized value is the current value divided by the maximum value, ranging from 0 to 1
        healthFillColor.color = gradient.Evaluate(slider.normalizedValue);
    }
}
