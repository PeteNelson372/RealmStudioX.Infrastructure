using System.Collections.Specialized;
using System.ComponentModel;

namespace RealmStudioX.Infrastructure;

public interface IApplicationPreferences : INotifyPropertyChanged
{
    #region General

    string DefaultThemeName { get; }

    StringCollection MeasurementUnits { get; }

    string DefaultMeasurementUnits { get; set; }

    #endregion

    #region Directory Settings

    string RootRealmStudioXDirectory { get; set; }

    string RootRealmsDirectory { get; set; }

    string RootAssetDirectory { get; set; }

    string DefaultModelsDirectory { get; set; }

    string DefaultSymbolDirectory { get; set; }

    string SymbolTagsFilePath { get; set; }

    string StructureSynonymsFilePath { get; set; }

    string TerrainSynonymsFilePath { get; set; }

    string VegetationSynonymsFilePath { get; set; }

    string LastFileDirectory { get; set; }

    #endregion

    #region Map Defaults

    int DefaultMapWidth { get; set; }

    int DefaultMapHeight { get; set; }

    string SelectedMapAreaUnits { get; set; }

    #endregion

    #region Autosave

    bool AutoSaveEnabled { get; set; }

    string AutosaveDirectory { get; set; }

    int AutosaveInterval { get; set; }

    int MinSaveInterval { get; }

    int MaxSaveInterval { get; }

    #endregion

    #region Export and Notifications

    bool PlaySoundEnabled { get; set; }

    string SelectedSaveSound { get; set; }

    string DefaultExportFormat { get; set; }

    #endregion

    #region Updates and World Anvil

    bool CheckUpdatesEnabled { get; set; }

    bool EnableWAIntegration { get; set; }

    string WorldAnvilApiToken { get; set; }

    #endregion

    void Save();
}