using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System;
using UnityEngine;

[Serializable]
public class FoodData
{
    public string food;
}

public class JsonManager : Singletone<JsonManager>
{
    [Serializable]
    private class JsonData
    {
        public FoodData[] arrFoodData;
    }

    private int _loadCompleteFileCount = 0;
    private int _loadingFileCount = 0;
    private List<Action<TextAsset>> _liJsonData = new List<Action<TextAsset>>();

    private List<FoodData> _liFoodData = new List<FoodData>();
    public List<FoodData> GetFoodDataList() => _liFoodData;

    public async Task Init()
    {
        await LoadJsonData();
    }

    public void Clear()
    {
        _liJsonData.Clear();
        _liFoodData.Clear();
    }

    async Task LoadJsonData()
    {
        Debug.Log("LoadJsonData");
        _loadCompleteFileCount = 0;
        _liJsonData.Clear();

        await LoadFoodData();

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

    async Task<TextAsset> LoadFoodData()
    {
        TaskCompletionSource<TextAsset> taskCompletionSource = new TaskCompletionSource<TextAsset>();

        Action<TextAsset> callback;
        _liFoodData.Clear();

        ResourceManager.Instance.LoadJson(CommonEnum.EJson.Food, callback = (TextAsset textAsset) =>
        {
            var jsonData = JsonUtility.FromJson<JsonData>("{\"arrFoodData\":" + textAsset.text + "}");
            foreach (var data in jsonData.arrFoodData)
            {
                _liFoodData.Add(data);
            }

            taskCompletionSource.SetResult(textAsset);
            ++_loadCompleteFileCount;
        });

        return await taskCompletionSource.Task;
    }
}
