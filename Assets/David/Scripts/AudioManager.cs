using System;
using UnityEngine;

// Díky tomuto atributu se tøída zobrazí v Unity Inspectoru
[System.Serializable]
public class Sound
{
    public string name; // Klíè, podle kterého se bude zvuk volat
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    // Skryjeme v Inspectoru, AudioSource se vytvoøí automaticky
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    // Statická instance (Singleton)
    public static AudioManager Instance;

    // Pole všech zvukù, které si nastavíš v Inspectoru
    public Sound[] sounds;

    void Awake()
    {
        // Zajištìní Singleton patternu (aby existoval vždy jen jeden AudioManager)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Zabrání znièení pøi naètení nové scény
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pro každý zvuk ze seznamu vytvoøíme vlastní AudioSource
        foreach (Sound s in sounds)
        {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    // Metoda, kterou bude volat tvùj kamarád
    public void Play(string name)
    {
        // Najde zvuk podle zadaného jména
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Zvuk s názvem: " + name + " nebyl nalezen!");
            return;
        }

        s.source.Play();
    }
}