using ChvjUnityInfra;
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
    public int stringID;
}

[Serializable]
public class AnimalData
{
    public int stringID;
}

public partial class JsonManager : CHSingletonStatic<JsonManager>
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

        CHMResource.Instance.Load<TextAsset>(CommonEnum.EJson.String, callback = (TextAsset textAsset) =>
        {
            if (textAsset == null)
            {
                Debug.LogError("[JsonManager] String json load failed (textAsset null)");
                taskCompletionSource.SetResult(null);
                return;
            }

            JsonData jsonData = JsonUtility.FromJson<JsonData>("{\"arrStringData\":" + textAsset.text + "}");
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

        CHMResource.Instance.Load<TextAsset>(CommonEnum.EJson.Country, callback = (TextAsset textAsset) =>
        {
            if (textAsset == null)
            {
                Debug.LogError("[JsonManager] Country json load failed (textAsset null)");
                taskCompletionSource.SetResult(null);
                return;
            }

            JsonData jsonData = JsonUtility.FromJson<JsonData>("{\"arrCountryData\":" + textAsset.text + "}");
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

        CHMResource.Instance.Load<TextAsset>(CommonEnum.EJson.Animal, callback = (TextAsset textAsset) =>
        {
            if (textAsset == null)
            {
                Debug.LogError("[JsonManager] Animal json load failed (textAsset null)");
                taskCompletionSource.SetResult(null);
                return;
            }

            JsonData jsonData = JsonUtility.FromJson<JsonData>("{\"arrAnimalData\":" + textAsset.text + "}");
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
        List<StringData> liString = GetStringDataList();
        StringData findData = liString.Find(_ => _.stringID == stringID);
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

    //# Country/Animal 데이터의 stringID 를 현재 언어 표시 문자열로 변환해 반환
    public List<string> GetCountryNameList()
    {
        List<string> result = new List<string>();
        foreach (CountryData data in GetCountryDataList())
        {
            result.Add(GetStringData(data.stringID));
        }

        return result;
    }

    public List<string> GetAnimalNameList()
    {
        List<string> result = new List<string>();
        foreach (AnimalData data in GetAnimalDataList())
        {
            result.Add(GetStringData(data.stringID));
        }

        return result;
    }
}