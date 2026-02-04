// Flashlight.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("Flashlight Objects")]
    public GameObject ON;  // The GameObject representing the flashlight when ON
    public GameObject OFF; // The GameObject representing the flashlight when OFF (optional)

    [Header("Audio")]
    public AudioSource flashlightAudioSource; // Drag an AudioSource component here
    public AudioClip toggleSound;             // Drag the sound clip to play for both ON/OFF toggle

    private bool isOn;

    void Start()
    {
        // Ensure the flashlight starts in the OFF state
        ON.SetActive(false);
        OFF.SetActive(true);
        isOn = false;

        // Optional: Get AudioSource component if not assigned, assuming it's on the same GameObject
        if (flashlightAudioSource == null)
        {
            flashlightAudioSource = GetComponent<AudioSource>();
            if (flashlightAudioSource == null)
            {
                Debug.LogWarning("Flashlight on " + gameObject.name + ": No AudioSource found or assigned. Add an AudioSource component if you want sound.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            // --- Play the sound first, as soon as 'Q' is pressed ---
            if (flashlightAudioSource != null && toggleSound != null)
            {
                flashlightAudioSource.PlayOneShot(toggleSound);
            }
            else if (flashlightAudioSource == null)
            {
                Debug.LogWarning("Flashlight on " + gameObject.name + ": AudioSource is not assigned. Cannot play toggle sound.");
            }
            else if (toggleSound == null)
            {
                Debug.LogWarning("Flashlight on " + gameObject.name + ": Toggle sound clip is not assigned. Cannot play sound.");
            }
            // --- End Play sound ---


            if (isOn)
            {
                // Turn OFF flashlight
                ON.SetActive(false);
                OFF.SetActive(true);
            }
            else // if (!isOn)
            {
                // Turn ON flashlight
                ON.SetActive(true);
                OFF.SetActive(false);
            }

            // Toggle the state
            isOn = !isOn;
        }
    }
}