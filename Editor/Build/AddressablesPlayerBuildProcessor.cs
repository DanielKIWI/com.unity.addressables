using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Maintains Addresssables build data when processing a player build.
/// </summary>
public class AddressablesPlayerBuildProcessor : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    /// <summary>
    /// Returns the player build processor callback order.
    /// </summary>
    public int callbackOrder
    {
        get { return 1; }
    }

    /// <summary>
    /// Restores temporary data created as part of a build.
    /// </summary>
    /// <param name="report">Stores temporary player build data.</param>
    public void OnPostprocessBuild(BuildReport report)
    {
        CleanTemporaryPlayerBuildData();

        var addressableTargetDir = GetFinalBuildAddressableDirectory(report.summary);
        if (addressableTargetDir != null)
        {
            addressableTargetDir += "/" + Addressables.StreamingAssetsSubFolder;// + "/" + PlatformMappingService.GetPlatform();
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
    internal static void CleanTemporaryPlayerBuildData()
    {
        if (Directory.Exists(Addressables.PlayerBuildDataPath))
        {
            DirectoryUtility.DirectoryMove(Addressables.PlayerBuildDataPath, Addressables.BuildPath);
            DirectoryUtility.DeleteDirectory(Application.streamingAssetsPath, onlyIfEmpty: true);
        }
    }

    ///<summary>
    /// Initializes temporary build data.
    /// </summary>
    /// <param name="report">Contains build data information.</param>
    public void OnPreprocessBuild(BuildReport report)
    {
        if (GetFinalBuildAddressableDirectory(report.summary) != null)
            return;
        CopyTemporaryPlayerBuildData();
    }

    internal static void CopyTemporaryPlayerBuildData()
    {
        if (Directory.Exists(Addressables.BuildPath))
        {
            Debug.Log(string.Format(
                "Copying Addressables data from {0} to {1}.  These copies will be deleted at the end of the build.",
                Addressables.BuildPath, Addressables.PlayerBuildDataPath));
            if (Directory.Exists(Addressables.PlayerBuildDataPath))
            {
                Debug.LogWarning($"Found and deleting directory \"{Addressables.PlayerBuildDataPath}\", directory is managed through Addressables.");
                DirectoryUtility.DeleteDirectory(Addressables.PlayerBuildDataPath, false);
            }

            string parentDir = Path.GetDirectoryName(Addressables.PlayerBuildDataPath);
            if (!string.IsNullOrEmpty(parentDir) && !Directory.Exists(parentDir))
                Directory.CreateDirectory(parentDir);
            Directory.Move(Addressables.BuildPath, Addressables.PlayerBuildDataPath );
        }
    }

    private string GetFinalBuildAddressableDirectory(BuildSummary buildSummary)
    {
        switch (buildSummary.platform)
        {
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                return Path.Combine(Path.GetDirectoryName(buildSummary.outputPath), Application.productName + "_Data", "StreamingAssets");
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
}
