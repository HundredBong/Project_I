using System;

public static class LanguageManager 
{
    public static LanguageType CurrentLanguage { get; private set; }

    public static event Action OnLanguageChanged;

    public static void SetLanguage(LanguageType newLanguage)
    {
        if (CurrentLanguage == newLanguage) { return; }

        CurrentLanguage = newLanguage;
        OnLanguageChanged?.Invoke();
    }
}
