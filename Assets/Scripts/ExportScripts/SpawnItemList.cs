using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;


[CreateAssetMenu(fileName ="SpawnItemList.asset", menuName = "Export/SpawnItemList")]
public class SpawnItemList : ScriptableObject
{
    [SerializeField] private AssetReferenceGameObject[] m_assetReferenceList = new AssetReferenceGameObject[0];

    public int AssetReferenceCount => m_assetReferenceList.Length;

    public AssetReferenceGameObject GetAssetReferenceAtIndex(int index) 
    {
        return m_assetReferenceList[index];
    }   

}
