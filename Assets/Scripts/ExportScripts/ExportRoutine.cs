using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Windows;

public class ExportRoutine : MonoBehaviour
{   
    [Header("References")]
    [SerializeField] private SpawnItemList m_itemList = null;

    [Header("Config")]
    [SerializeField] private int dimensions = 512;
    [SerializeField] private float screenshotEdgeSpace = 0.1f;

    private AssetReferenceGameObject m_assetLoadedAsset;
    private GameObject m_instanceObject = null;

    private Camera tempCam;
    private string rootPath;
    private int currentAssetIndex = 0;

    private void Start()
    {
        if (m_itemList == null || m_itemList.AssetReferenceCount == 0) 
        {
            Debug.LogError("Spawn list not setup correctly");
        }

        // Set up root path and output directory
        rootPath = System.IO.Directory.GetCurrentDirectory() + "/Output";
        if (!Directory.Exists(rootPath))
            Directory.CreateDirectory(rootPath);

        // Create a dummy camera that will be used for screenshotting, so that the regular game view can still be seen in the default cam
        tempCam = new GameObject().AddComponent(typeof(Camera)).GetComponent<Camera>();
        tempCam.CopyFrom(Camera.main);
        if (tempCam.targetTexture == null)
        {
            tempCam.targetTexture = new RenderTexture(dimensions, dimensions, 24);
        }

        LoadItemAtIndex(m_itemList, 0);
    }

    private void LoadItemAtIndex(SpawnItemList itemList, int index) 
    {        
        m_assetLoadedAsset = itemList.GetAssetReferenceAtIndex(index);

        var loadRoutine = m_assetLoadedAsset.LoadAssetAsync();
        loadRoutine.Completed += LoadRoutine_Completed;

        void LoadRoutine_Completed(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<GameObject> obj)
        {
            m_instanceObject = Instantiate(obj.Result, new Vector3(), Quaternion.identity, transform);
            StartCoroutine(TakeCamScreenshots());
        }
    }

    /// <summary>
    /// Take 16 screenshots of the loaded asset, rotated 22.5 degrees each frame
    /// </summary>
    private IEnumerator TakeCamScreenshots()
    {
        // Prepare the export path and directory to store screenshots
        string exportPath = rootPath + "/" + m_assetLoadedAsset.Asset.name + "/frame";
        if (!Directory.Exists(rootPath + "/" + m_assetLoadedAsset.Asset.name))
            Directory.CreateDirectory(rootPath + "/" + m_assetLoadedAsset.Asset.name);

        // Calculate the maximum bounds of the asset, encapsulating all of the object renderers
        Bounds bounds = new();
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        // Find the maxium size of the asset bounds to properly configure the camera size
        float maxSize = Mathf.Max(new float[] { bounds.size.x, bounds.size.y, bounds.size.z });
        tempCam.orthographicSize = maxSize / 2 + screenshotEdgeSpace;
        Camera.main.orthographicSize = maxSize / 2 + screenshotEdgeSpace;

        // Move the gameObject vertically to be in the camera center
        m_instanceObject.transform.position = new Vector3(0, 0 - bounds.size.y / 2, 0);

        // Camera screenshots
        for (int i = 0; i < 16; i++)
        {
            yield return new WaitForEndOfFrame();

            // Store original render texture
            RenderTexture originalRT = RenderTexture.active;
            RenderTexture.active = tempCam.targetTexture;

            // Render cam view
            tempCam.Render();

            // Create a new texture2d to store the active render texure
            Texture2D renderImg = new Texture2D(dimensions, dimensions);
            renderImg.ReadPixels(new Rect(0, 0, dimensions, dimensions), 0, 0);
            renderImg.Apply();

            // Restore the original render texture
            RenderTexture.active = originalRT;

            // Encode and export the texture data to a png file
            byte[] imgBytes = renderImg.EncodeToPNG();
            Destroy(renderImg);
            File.WriteAllBytes(exportPath + i.ToString("D4") + ".png", imgBytes);

            // Prepare the object for the next screenshot
            m_instanceObject.transform.Rotate(new Vector3(0, 22.5f, 0));
        }

        // Destroy the object, then either load the next item or exit play mode
        Destroy(m_instanceObject);
        if (currentAssetIndex < m_itemList.AssetReferenceCount - 1)
        {
            currentAssetIndex++;
            LoadItemAtIndex(m_itemList, currentAssetIndex);
        }
        else
        {
            Destroy(tempCam.gameObject);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
