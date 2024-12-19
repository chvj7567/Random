using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;

public class ResourceManager : Singletone<ResourceManager>
{
    const string LableName = "Resource";
    bool _initialize = false;

    //# 에셋 이름, 에셋 경로
    Dictionary<string, IResourceLocation> _dicAssetInfo = new Dictionary<string, IResourceLocation>();

    public async Task<bool> Init()
    {
        if (_initialize)
            return false;

        _initialize = true;

        TaskCompletionSource<bool> initComplete = new TaskCompletionSource<bool>();

        Addressables.InitializeAsync().Completed += (handle) =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                initComplete.TrySetResult(false);
            }
            else
            {
                initComplete.TrySetResult(true);
            }

            Addressables.Release(handle);
        };

        await initComplete.Task;

        return await SaveLocationInfo();
    }

    async Task<bool> SaveLocationInfo()
    {
        TaskCompletionSource<bool> saveComplete = new TaskCompletionSource<bool>();

        Addressables.LoadResourceLocationsAsync(LableName).Completed += (handle) =>
        {
            if (handle.Status != AsyncOperationStatus.Succeeded)
            {
                saveComplete.TrySetResult(false);
            }
            else
            {
                foreach (var pathInfo in handle.Result)
                {
                    Debug.Log(pathInfo);
                    _dicAssetInfo.Add(pathInfo.ToString().Split('/').Last().Split('.').First(), pathInfo);
                }

                saveComplete.TrySetResult(true);
            }

            Addressables.Release(handle);
        };

        return await saveComplete.Task;
    }

    void LoadAsset<T>(string assetName, Action<T> callback = null) where T : UnityEngine.Object
    {
        if (_dicAssetInfo.TryGetValue(assetName, out var pathInfo) == false)
            return;

        Addressables.LoadAssetAsync<T>(pathInfo).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                callback?.Invoke(handle.Result);
            }
        };
    }

    public void LoadGameObject(string assetName, Action<GameObject> callback = null)
    {
        LoadAsset<GameObject>(assetName, (obj) =>
        {
            if (obj == null)
                return;

            callback?.Invoke(Instantiate(obj));
        });
    }

    public void LoadUI(CommonEnum.EUI uiType, Action<GameObject> callback = null)
    {
        LoadGameObject(uiType.ToString(), callback);
    }
}
