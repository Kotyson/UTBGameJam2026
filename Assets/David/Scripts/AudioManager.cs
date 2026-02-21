using System;
using UnityEngine;
using Random = UnityEngine.Random;

// D�ky tomuto atributu se t��da zobraz� v Unity Inspectoru
[System.Serializable]
public class Sound
{
    public string name; // Kl��, podle kter�ho se bude zvuk volat
    public AudioClip[] clip;

    [Range(0f, 1f)]
    public float volume = 1f;
    [Range(0.1f, 3f)]
    public float pitch = 1f;

    public bool loop;

    // Skryjeme v Inspectoru, AudioSource se vytvo�� automaticky
    [HideInInspector]
    public AudioSource source;
}

public class AudioManager : MonoBehaviour
{
    // Statick� instance (Singleton)
    public static AudioManager Instance;

    // Pole v�ech zvuk�, kter� si nastav� v Inspectoru
    public Sound[] sounds;

    void Awake()
    {
        // Zaji�t�n� Singleton patternu (aby existoval v�dy jen jeden AudioManager)
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Zabr�n� zni�en� p�i na�ten� nov� sc�ny
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Pro ka�d� zvuk ze seznamu vytvo��me vlastn� AudioSource
        foreach (Sound s in sounds)
        {
            s.pitch = 1.0f;
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip[0];
            s.source.volume = s.volume;
            s.source.pitch = 1.0f;
            s.source.loop = s.loop;
        }
    }

    
    public void Play(string name)
    {
        // Najde zvuk podle zadan�ho jm�na
        Sound s = Array.Find(sounds, sound => sound.name == name);

        if (s == null)
        {
            Debug.LogWarning("Zvuk s n�zvem: " + name + " nebyl nalezen!");
            return;
        }
        Debug.Log("Playing sound " +  name);
        
        s.source.clip = s.clip[Random.Range(0, s.clip.Length)];
        s.source.Play();
    }
}