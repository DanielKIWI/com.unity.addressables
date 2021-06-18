Clone from Unity addressable source version 1.8.3 with a minor adjustment that skips copying addressable bundles into Assets/StreamingAssets and instead copies them directly into the build folder avoiding Asset Import.

Implemented exception case for xbox and ps4 package builds, where the addressable bundle files get bundled into the package file and therefore need to be inside Assets/StreamingAssets to be bundled by unity build process. 

Put the following line into your Packages/manifest.json file to use this addressable fork.

`com.unity.addressables": "https://github.com/DanielKIWI/com.unity.addressables.git#1.8.3",`

If you notice issues with any other build platform feel free to add an exception for that in \Editor\Build\AddressablesPlayerBuildProcessor.cs GetFinalBuildAddressableDirectory function and create a pull request.
