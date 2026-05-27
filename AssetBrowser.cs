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
                ..types
                    .SelectMany(
                        t => manager.GetByType(t))
                    .DistinctBy(
                        a => a.Name)
                    .OrderBy( a => a.Name)
            ];
        }

        // -------------------------------------------------
        // Properties
        // -------------------------------------------------

        public AssetDescriptor? Current =>
            _assets.Count == 0
                ? null
                : _assets[_index];

        public bool HasAssets =>
            _assets.Count > 0;

        public int Count =>
            _assets.Count;

        public int CurrentIndex =>
            _index;

        // -------------------------------------------------
        // Navigation
        // -------------------------------------------------

        public void Next()
        {
            if (_assets.Count == 0)
            {
                return;
            }

            _index =
                (_index + 1) % _assets.Count;
        }

        public void Previous()
        {
            if (_assets.Count == 0)
            {
                return;
            }

            _index =
                (_index - 1 + _assets.Count)
                % _assets.Count;
        }

        // -------------------------------------------------
        // Selection
        // -------------------------------------------------

        public bool SelectById(string id)
        {
            if (_assets.Count == 0)
            {
                return false;
            }

            int index =
                _assets.FindIndex(
                    a => a.Id == id);

            if (index < 0 ||
                index == _index)
            {
                return false;
            }

            _index = index;

            return true;
        }

        public bool SelectByIndex(int index)
        {
            if (_assets.Count == 0)
            {
                return false;
            }

            if (index < 0 ||
                index >= _assets.Count)
            {
                return false;
            }

            _index = index;

            return true;
        }

        // -------------------------------------------------
        // Asset access
        // -------------------------------------------------

        public AssetDescriptor? GetCurrentAsset()
        {
            return Current;
        }

        public SKImage? GetCurrentImage()
        {
            if (Current == null)
            {
                return null;
            }

            return _assetManager.GetImage(
                Current.Id);
        }

        public IReadOnlyList<AssetDescriptor> GetAssets()
        {
            return _assets;
        }
    }
}