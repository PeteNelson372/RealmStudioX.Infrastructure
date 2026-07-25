using RealmStudioShapeRenderingLib;
using SkiaSharp;

namespace RealmStudioX.Infrastructure
{
    public sealed class AssetBrowser
    {
        private readonly AssetManager _assetManager;
        private readonly List<AssetDescriptor> _assets;

        private int _index;

        // -------------------------------------------------
        // Single-type convenience constructor
        // -------------------------------------------------

        public AssetBrowser(
            AssetManager manager,
            AssetType type)
            : this(manager, [type])
        {
        }

        // -------------------------------------------------
        // Multi-type constructor
        // -------------------------------------------------

        public AssetBrowser(
            AssetManager manager,
            IEnumerable<AssetType> types)
        {
            _assetManager = manager;

            _assets =
            [
                .. types
                    .SelectMany(t => manager.GetByType(t))
                    .DistinctBy(a => a.Name)
                    .OrderBy(a => a.Name)
            ];
        }

        // -------------------------------------------------
        // Properties
        // -------------------------------------------------

        public IReadOnlyList<AssetDescriptor> Assets => _assets;

        public AssetDescriptor? Current =>
            _assets.Count == 0
                ? null
                : _assets[_index];

        public SKImage? CurrentImage =>
            Current == null
                ? null
                : _assetManager.GetImage(Current.Id);

        public bool HasAssets => _assets.Count > 0;

        public int Count => _assets.Count;

        public int CurrentIndex => _index;

        // -------------------------------------------------
        // Navigation
        // -------------------------------------------------

        public void Next()
        {
            if (!HasAssets)
            {
                return;
            }

            _index = (_index + 1) % _assets.Count;
        }

        public void Previous()
        {
            if (!HasAssets)
            {
                return;
            }

            _index = (_index - 1 + _assets.Count) % _assets.Count;
        }

        // -------------------------------------------------
        // Selection
        // -------------------------------------------------

        public bool SelectById(string? id)
        {
            if (!HasAssets || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            int index = _assets.FindIndex(a =>
                a.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

            if (index < 0)
            {
                return false;
            }

            if (index == _index)
            {
                return true;
            }

            _index = index;

            return true;
        }

        public bool SelectByIndex(int index)
        {
            if (!HasAssets)
            {
                return false;
            }

            if (index < 0 || index >= _assets.Count)
            {
                return false;
            }

            if (index == _index)
            {
                return true;
            }

            _index = index;

            return true;
        }
    }
}