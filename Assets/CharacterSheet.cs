using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CharacterData
{
    public string Name;
    public byte[] Portrait;

    public string CurrentStr;
    public string MaxStr;

    public string CurrentDex;
    public string MaxDex;

    public string CurrentWil;
    public string MaxWil;

    public string CurrentHp;
    public string MaxHp;

    public string Armor;

    public bool Depraived;

    public string GP;
    public string SP;
    public string CP;

    public string Notes;

    public List<string> Inventory = new List<string>();
    public List<bool> HandToggle = new List<bool>();
    public List<bool> Fatigue = new List<bool>();
}

public class CharacterSheet : MonoBehaviour
{
    public TMP_InputField Name;

    public Image Portrait;
    private byte[] portraitTexture;

    public GameObject Str;
    public GameObject Dex;
    public GameObject Wil;
    public GameObject Hp;

    public TMP_InputField Armor;

    public TMP_InputField GP;
    public TMP_InputField SP;
    public TMP_InputField CP;

    public TMP_InputField Notes;

    public Toggle Depraived;

    public GameObject Inventory;
    public GameObject Hand;
    public GameObject Fatigue;

    private string DefaultCharactersDirectory
    {
        get
        {
            return $"{Application.dataPath}/../Characters/";
        }
    }

    public GameObject ButtonPrefab;
    public GameObject ContextMenu;

    public GameObject LoadFileMenu;
    public Transform LoadFileButtons;

    public GameObject SaveFileMenu;
    public TMP_InputField SaveFileName;

    private void Start()
    {
        Directory.CreateDirectory(DefaultCharactersDirectory);
        UnityEngine.Application.quitting += () => SaveCharacterAt(DefaultCharactersDirectory);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        { 
            ContextMenu.SetActive(!ContextMenu.activeInHierarchy);
            LoadFileMenu.SetActive(false);
            SaveFileMenu.SetActive(false);
        }
    }

    string GetSaveData()
    {
        CharacterData cd = new CharacterData();

        cd.Name = Name.text;
        cd.Portrait = portraitTexture;

        cd.CurrentStr = Str.transform.GetChild(0).GetComponent<TMP_InputField>().text;
        cd.MaxStr = Str.transform.GetChild(1).GetComponent<TMP_InputField>().text;

        cd.CurrentDex = Dex.transform.GetChild(0).GetComponent<TMP_InputField>().text;
        cd.MaxDex = Dex.transform.GetChild(1).GetComponent<TMP_InputField>().text;

        cd.CurrentWil = Wil.transform.GetChild(0).GetComponent<TMP_InputField>().text;
        cd.MaxWil = Wil.transform.GetChild(1).GetComponent<TMP_InputField>().text;

        cd.CurrentHp = Hp.transform.GetChild(0).GetComponent<TMP_InputField>().text;
        cd.MaxHp = Hp.transform.GetChild(1).GetComponent<TMP_InputField>().text;

        cd.Armor = Armor.text;

        cd.GP = GP.text;
        cd.SP = SP.text;
        cd.CP = CP.text;

        cd.Depraived = Depraived.isOn;

        foreach (var inventoryItem in Inventory.GetComponentsInChildren<TMP_InputField>())
            cd.Inventory.Add(inventoryItem.text);

        foreach (var hand in Hand.GetComponentsInChildren<Toggle>())
            cd.HandToggle.Add(hand.isOn);

        foreach (var fatigue in Fatigue.GetComponentsInChildren<Toggle>())
            cd.Fatigue.Add(fatigue.isOn);

        cd.Notes = Notes.text;

        return JsonUtility.ToJson(cd);
    }

    public void SaveCharacter()
    {
        SaveFileMenu.SetActive(true);
        SaveFileName.onSubmit.RemoveAllListeners();
        SaveFileName.onSubmit.AddListener((txt) =>
        {
            SaveCharacterAt(Path.Combine(DefaultCharactersDirectory, txt + ".json"));
            SaveFileMenu.SetActive(false);
        });
    }

    void SaveCharacterAt(string location)
    {
        if (string.IsNullOrEmpty(Name.text))
            return;

        if (location.EndsWith('/') || location.EndsWith('\\'))
        {
            string name = string.IsNullOrEmpty(Name.text) ? $"auto_save_character{DateTime.Now.Ticks}.json" : $"{Name.text}.json";
            location = Path.Combine(location, name);
        }

        File.WriteAllText(location, GetSaveData());
    }

    public void LoadCharacter()
    {
        LoadFileMenu.SetActive(true);

        foreach (var child in LoadFileButtons.GetComponentsInChildren<Transform>())
        {
            if (child == LoadFileButtons)
                continue;

            Destroy(child.gameObject);
        }

        foreach (var file in Directory.EnumerateFiles(DefaultCharactersDirectory))
        {
            GameObject bPrefab = Instantiate(ButtonPrefab, LoadFileButtons);
            var button = bPrefab.GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(file);
            button.onClick.AddListener(() =>
            {
                string readData = File.ReadAllText(file);
                CharacterData cd = JsonUtility.FromJson<CharacterData>(readData);

                Name.text = cd.Name;
                portraitTexture = cd.Portrait;
                SetImage();

                Str.transform.GetChild(0).GetComponent<TMP_InputField>().text = cd.CurrentStr;
                Str.transform.GetChild(1).GetComponent<TMP_InputField>().text = cd.MaxStr;

                Dex.transform.GetChild(0).GetComponent<TMP_InputField>().text = cd.CurrentDex;
                Dex.transform.GetChild(1).GetComponent<TMP_InputField>().text = cd.MaxDex;

                Wil.transform.GetChild(0).GetComponent<TMP_InputField>().text = cd.CurrentWil;
                Wil.transform.GetChild(1).GetComponent<TMP_InputField>().text = cd.MaxWil;

                Hp.transform.GetChild(0).GetComponent<TMP_InputField>().text = cd.CurrentHp;
                Hp.transform.GetChild(1).GetComponent<TMP_InputField>().text = cd.MaxHp;

                Armor.text = cd.Armor;

                GP.text = cd.GP;
                SP.text = cd.SP;
                CP.text = cd.CP;

                Depraived.isOn = cd.Depraived;

                int i = 0;
                foreach (var inventoryItem in Inventory.GetComponentsInChildren<TMP_InputField>())
                {
                    inventoryItem.text = cd.Inventory[i];
                    i++;
                }

                i = 0;
                foreach (var hand in Hand.GetComponentsInChildren<Toggle>())
                {
                    hand.isOn = cd.HandToggle[i];
                    i++;
                }

                i = 0;
                foreach (var fatigue in Fatigue.GetComponentsInChildren<Toggle>())
                {
                    fatigue.isOn = cd.Fatigue[i];
                    i++;
                }

                Notes.text = cd.Notes;
                
                LoadFileMenu.SetActive(false);
                ContextMenu.SetActive(false);
            });
        }
    }

    public void SelectPortrait()
    {
        SaveFileMenu.SetActive(true);
        SaveFileName.onSubmit.RemoveAllListeners();
        SaveFileName.onSubmit.AddListener((text) =>
        {
            portraitTexture = File.ReadAllBytes(text);
            SetImage();
        });
    }

    void SetImage()
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(portraitTexture);
        Portrait.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 100f);
    }
}
