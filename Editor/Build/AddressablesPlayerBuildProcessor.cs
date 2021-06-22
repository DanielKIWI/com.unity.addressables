using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class AddressablesPlayerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    public int callbackOrder
    {
        get { return 1; }
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        CleanTemporaryPlayerBuildData();
        var addressableTargetDir = GetFinalBuildAddressableDirectory(report.summary);
        if (addressableTargetDir != null)
        {
            addressableTargetDir += "/" + Addressables.StreamingAssetsSubFolder + "/" + PlatformMappingService.GetPlatform();
            if (Directory.Exists(addressableTargetDir))
            {
                Debug.Log(string.Format("Deleting Addressables data from {0}.", addressableTargetDir));
                Directory.Delete(addressableTargetDir, true);
            }
            Debug.Log(string.Format(
                "Copying Addressables data from {0} to {1}.  Bypassing Editor StreamingAssets.",
                Addressables.BuildPath, addressableTargetDir));

            DirectoryUtility.DirectoryCopy(Addressables.BuildPath, addressableTargetDir, true);
        }
    }
    
    [InitializeOnLoadMethod]
    static void CleanTemporaryPlayerBuildData()
    {
        string addressablesStreamingAssets = Path.Combine(Application.streamingAssetsPath, Addressables.StreamingAssetsSubFolder);
        if (Directory.Exists(addressablesStreamingAssets))
        {
            Debug.Log(string.Format("Deleting Addressables data from {0}.", addressablesStreamingAssets));
            Directory.Delete(addressablesStreamingAssets, true);
            //Will delete the directory only if it's empty
            DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath);
        }
    }

    public void OnPreprocessBuild(BuildReport report)
    {
        if (GetFinalBuildAddressableDirectory(report.summary) != null)
            return;
        if (ManuallyCopiedAddressablesToStreamingAssets)
            return;

        if (Directory.Exists(Addressables.BuildPath))
        {
            Debug.Log(string.Format(
                "Copying Addressables data from {0} to {1}.  These copies will be deleted at the end of the build.",
                Addressables.BuildPath, Addressables.PlayerBuildDataPath));

            DirectoryUtility.DirectoryCopy(Addressables.BuildPath, Addressables.PlayerBuildDataPath, true);
        }
    }
    
    private string GetFinalBuildAddressableDirectory(BuildSummary buildSummary)
    {
        switch (buildSummary.platform)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(buildSummary.outputPath, Application.productName + "_Data", "StreamingAssets");
            case BuildTarget.XboxOne:
                if (EditorUserBuildSettings.xboxOneDeployMethod != XboxOneDeployMethod.Push && EditorUserBuildSettings.xboxOneDeployMethod != XboxOneDeployMethod.RunFromPC)
                    return null;
                return Path.Combine(buildSummary.outputPath, Application.productName, "Data", "StreamingAssets");
            case BuildTarget.PS4:
#if UNITY_PS4
                if (EditorUserBuildSettings.ps4BuildSubtarget != PS4BuildSubtarget.PCHosted)
                    return null;
#endif
                return Path.Combine(buildSummary.outputPath, "Media", "StreamingAssets");
            default:
                return null;
        }
    }


    public const string kMenuText = "Assets/Manually copied Addressables to StreamingAssets";

    private static bool ManuallyCopiedAddressablesToStreamingAssets
    {
        get => EditorPrefs.GetBool(kMenuText, false);
        set => EditorPrefs.SetBool(kMenuText, value);
    }

    /// <summary>
    /// Raises the initialize on load method event.
    /// </summary>
    [InitializeOnLoadMethod]
    static void OnInitializeOnLoadMethod()
    {
        EditorApplication.delayCall += () => Valid();
    }

    /// <summary>
    /// Toggles the menu.
    /// </summary>
    [MenuItem(kMenuText)]
    static void OnClickMenu()
    {
        // Check/Uncheck menu.
        bool isChecked = !Menu.GetChecked(kMenuText);
        Menu.SetChecked(kMenuText, isChecked);

        // Save to EditorPrefs.
        ManuallyCopiedAddressablesToStreamingAssets = isChecked;
    }

    [MenuItem(kMenuText, true)]
    static bool Valid()
    {
        // Check/Uncheck menu from EditorPrefs.
        Menu.SetChecked(kMenuText, ManuallyCopiedAddressablesToStreamingAssets);
        return true;
    }
}
