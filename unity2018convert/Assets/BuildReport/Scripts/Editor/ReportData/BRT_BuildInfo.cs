using UnityEditor;
using System;
using System.Xml.Serialization;

namespace BuildReportTool
{

// class for holding a build report
// this is the class that is serialized when saving a build report to a file
[System.Serializable, XmlRoot("BuildInfo")]
public class BuildInfo
{
	// General Info
	// ==================================================================================
	public string ProjectName;
	public string BuildType; // type of build as reported by the Unity editor log

	public string SuitableTitle
	{
		get
		{
			if (UnityBuildSettings != null && UnityBuildSettings.HasValues && !string.IsNullOrEmpty(UnityBuildSettings.ProductName))
			{
				return UnityBuildSettings.ProductName;
			}
			return ProjectName;
		}
	}

	
	System.TimeSpan _reportGenerationTime;
	
	[XmlIgnore]
	public TimeSpan ReportGenerationTime
    {
		get { return _reportGenerationTime; }
		set { _reportGenerationTime = value; }
    }
	
	[XmlElement("ReportGenerationTime")]
    public long ReportGenerationTimeInTicks
    {
		get { return _reportGenerationTime.Ticks; }
		set { _reportGenerationTime = new TimeSpan(value); }
    }
	

	public DateTime TimeGot;
	public string TimeGotReadable;

	public string GetTimeReadable()
	{
		if (!string.IsNullOrEmpty(TimeGotReadable))
		{
			return TimeGotReadable;
		}
		return TimeGot.ToString(BuildReportTool.ReportGenerator.TIME_OF_BUILD_FORMAT);
	}



	public string UnityVersion = "";


	public string UnityVersionDisplayed
	{
		get
		{
			if (UnityBuildSettings != null && UnityBuildSettings.HasValues)
			{
				return UnityVersion + (UnityBuildSettings.UsingAdvancedLicense ? " Pro" : "");
			}
			return UnityVersion;
		}
	}

	public string EditorAppContentsPath = "";
	public string ProjectAssetsPath = "";
	public string BuildFilePath = "";

	public ApiCompatibilityLevel MonoLevel;
	public StrippingLevel CodeStrippingLevel;


	// Build Settings
	// ==================================================================================
	public UnityBuildSettings UnityBuildSettings;
	public bool HasUnityBuildSettings;


	// unity/os environment values at time of build report creation
	// ==================================================================================

	public string[] ScenesIncludedInProject;
	public string[] PrefabsUsedInScenes;

	public bool AndroidUseAPKExpansionFiles;
	public bool AndroidCreateProject;




	// sizes
	// ==================================================================================

	public BuildReportTool.SizePart[] BuildSizes;




	// total sizes
	// ==================================================================================

	// old size values were only TotalBuildSize and CompressedBuildSize

	public bool HasOldSizeValues
	{
		get
		{
			return
				string.IsNullOrEmpty(UnusedTotalSize) &&
				string.IsNullOrEmpty(UsedTotalSize) &&
				string.IsNullOrEmpty(StreamingAssetsSize) &&
				string.IsNullOrEmpty(WebFileBuildSize) &&
				string.IsNullOrEmpty(AndroidApkFileBuildSize) &&
				string.IsNullOrEmpty(AndroidObbFileBuildSize);
		}
	}

	public string CompressedBuildSize = ""; // not used anymore

	public string UnusedTotalSize = "";
	public string UsedTotalSize = "";
	public string StreamingAssetsSize = "";

	public bool HasStreamingAssets
	{
		get
		{
			return StreamingAssetsSize != "0 B";
		}
	}

	public string TotalBuildSize = "";


	// per-platform sizes
	public string WebFileBuildSize = "";
	public string AndroidApkFileBuildSize = "";
	public string AndroidObbFileBuildSize = "";




	// file entries
	// ==================================================================================

	public BuildReportTool.SizePart[] MonoDLLs;
	public BuildReportTool.SizePart[] ScriptDLLs;

	public FileFilterGroup FileFilters = null;

	public AssetList UsedAssets = null;
	public AssetList UnusedAssets = null;

	public bool HasUsedAssets
	{
		get
		{
			return UsedAssets != null;
		}
	}
	public bool HasUnusedAssets
	{
		get
		{
			return UnusedAssets != null;
		}
	}





	// build report tool options values at time of building
	// ==================================================================================

	public bool IncludedSvnInUnused;
	public bool IncludedGitInUnused;

	public bool UsedAssetsIncludedInCreation;
	public bool UnusedAssetsIncludedInCreation;
	public bool UnusedPrefabsIncludedInCreation;

	public int UnusedAssetsEntriesPerBatch;




	// temp variables that are not serialized into the XML file
	// ==================================================================================

	// only used while generating the build report or opening one

	int _unusedAssetsBatchNum = 0;

	public int UnusedAssetsBatchNum { get{ return _unusedAssetsBatchNum; } }
	public void MoveUnusedAssetsBatchNumToNext()
	{
		++_unusedAssetsBatchNum;
	}
	public void MoveUnusedAssetsBatchNumToPrev()
	{
		if (_unusedAssetsBatchNum == 0)
		{
			return;
		}
		--_unusedAssetsBatchNum;
	}
	
	// ---------------------------------------

	string _savedPath;

	public string SavedPath { get{ return _savedPath; } }
	public void SetSavedPath(string val)
	{
		_savedPath = val;
	}
	
	// ---------------------------------------

	BuildTarget _buildTarget;

	public BuildTarget BuildTargetUsed { get{ return _buildTarget; } }
	public void SetBuildTargetUsed(BuildTarget val)
	{
		_buildTarget = val;
	}

	// ---------------------------------------

	bool _refreshRequest;

	public void FlagOkToRefresh()
	{
		_refreshRequest = true;
	}

	public void FlagFinishedRefreshing()
	{
		_refreshRequest = false;
	}

	public bool RequestedToRefresh { get{ return _refreshRequest; } }




	// Queries
	// ==================================================================================

	public bool HasContents
	{
		get
		{
			// build sizes can't be empty (they are always there when you build)
			// script dlls can't be empty (the project is sure to have some scripts in it)
			return !string.IsNullOrEmpty(ProjectName) && (BuildSizes != null && BuildSizes.Length > 0) && (ScriptDLLs != null && ScriptDLLs.Length > 0);
		}
	}




	// Commands
	// ==================================================================================

	public string GetDefaultFilename()
	{
		return ProjectName + "-" + BuildType + TimeGot.ToString("-yyyyMMMdd-HHmmss") + ".xml";
	}

	public void UnescapeAssetNames()
	{
		if (UsedAssets != null)
		{
			UsedAssets.UnescapeAssetNames();
		}

		if (UnusedAssets != null)
		{
			UnusedAssets.UnescapeAssetNames();
		}
	}

	public void RecategorizeAssetLists()
	{
		FileFilterGroup fileFiltersToUse = FileFilters;

		if (BuildReportTool.Options.ShouldUseConfiguredFileFilters())
		{
			fileFiltersToUse = BuildReportTool.FiltersUsed.GetProperFileFilterGroupToUse();
			//Debug.Log("going to use configured file filters instead... loaded: " + (fileFiltersToUse != null));
		}

		if (UsedAssets != null)
		{
			UsedAssets.AssignPerCategoryList( BuildReportTool.ReportGenerator.SegregateAssetSizesPerCategory(UsedAssets.All, fileFiltersToUse) );

			UsedAssets.RefreshFilterLabels(fileFiltersToUse);
		}

		if (UnusedAssets != null)
		{
			UnusedAssets.AssignPerCategoryList( BuildReportTool.ReportGenerator.SegregateAssetSizesPerCategory(UnusedAssets.All, fileFiltersToUse) );

			UnusedAssets.RefreshFilterLabels(fileFiltersToUse);
		}
	}

	void CalculateUsedAssetsDerivedSizes()
	{
		if (UsedAssets != null)
		{
			for (int n = 0, len = UsedAssets.All.Length; n < len; ++n)
			{
				UsedAssets.All[n].DerivedSize = BuildReportTool.Util.GetApproxSizeFromString(UsedAssets.All[n].Size);
			}
		}
	}




	// Events
	// ==================================================================================

	public void OnDeserialize()
	{
		if (HasContents)
		{
			CalculateUsedAssetsDerivedSizes();
			UnescapeAssetNames();
			RecategorizeAssetLists();
		}
	}
}

} // namespace BuildReportTool
