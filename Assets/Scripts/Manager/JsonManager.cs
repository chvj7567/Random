using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System;
using UnityEngine;

[Serializable]
public class StringData
{
    public int stringID;
    public string korean;
    public string english;
}

[Serializable]
public class CountryData
{
    public string korName;
    public string engName;
}

[Serializable]
public class AnimalData
{
    public string korName;
    public string engName;
}

public partial class JsonManager : SingletoneStatic<JsonManager>
{
    [Serializable]
    private class JsonData
    {
        public StringData[] arrStringData;
        public CountryData[] arrCountryData;
        public AnimalData[] arrAnimalData;
    }

    private int _loadCompleteFileCount = 0;
    private int _loadingFileCount = 0;
    private List<Action<TextAsset>> _liJsonData = new List<Action<TextAsset>>();

    private List<StringData> _liStringData = new List<StringData>();
    public List<StringData> GetStringDataList() => _liStringData;

    private List<CountryData> _liCountryData = new List<CountryData>();
    public List<CountryData> GetCountryDataList() => _liCountryData;

    private List<AnimalData> _liAnimalData = new List<AnimalData>();
    public List<AnimalData> GetAnimalDataList() => _liAnimalData;

    public async Task Init()
    {
        await LoadJsonData();
    }

    public void Clear()
    {
        _liJsonData.Clear();
        _liStringData.Clear();
        _liCountryData.Clear();
        _liAnimalData.Clear();
    }

    private async Task LoadJsonData()
    {
        Debug.Log("LoadJsonData");
        _loadCompleteFileCount = 0;
        _liJsonData.Clear();

        await LoadStringData();
        await LoadCountryData();
        await LoadAnimalData();

        _loadingFileCount = _loadCompleteFileCount;
    }

    public float GetJsonLoadingPercent()
    {
        if (_loadingFileCount == 0 || _loadCompleteFileCount == 0)
        {
            return -1;
        }

        return ((float)_loadCompleteFileCount) / _loadingFileCount * 100f;
    }

    private async Task<TextAsset> LoadStringData()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        _liStringData.Clear();

        ResourceManager.Instance.LoadJson(CommonEnum.EJson.String, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"arrStringData\":" + textAsset.text + "}");
            foreach (var data in jsonData.arrStringData)
            {
                _liStringData.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++_loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    private async Task<TextAsset> LoadCountryData()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        _liCountryData.Clear();

        ResourceManager.Instance.LoadJson(CommonEnum.EJson.Country, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"arrCountryData\":" + textAsset.text + "}");
            foreach (var data in jsonData.arrCountryData)
            {
                _liCountryData.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++_loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }

    private async Task<TextAsset> LoadAnimalData()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        _liAnimalData.Clear();

        ResourceManager.Instance.LoadJson(CommonEnum.EJson.Animal, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"arrAnimalData\":" + textAsset.text + "}");
            foreach (var data in jsonData.arrAnimalData)
            {
                _liAnimalData.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++_loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }
}

public partial class JsonManager
{
    public string GetStringData(int stringID)
    {
        var liString = GetStringDataList();
        var findData = liString.Find(_ => _.stringID == stringID);
        if (findData == null)
            return string.Empty;

        if (GameManagement.Instance.Language == SystemLanguage.Korean)
        {
            return findData.korean;
        }
        else
        {
            return findData.english;
        }
    }
}