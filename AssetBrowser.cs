using RealmStudioShapeRenderingLib;
using SkiaSharp;

namespace RealmStudioX.Infrastructure
{
    public sealed class AssetBrowser(AssetManager manager, AssetType type)
    {
        private readonly AssetManager _assetManager = manager;
        private readonly List<AssetDescriptor> _assets = [.. manager.GetByType(type)];
        private int _index;

        public AssetDescriptor? Current =>
            _assets.Count == 0 ? null : _assets[_index];

        public bool HasAssets => _assets.Count > 0;

        public void Next()
        {
            if (_assets.Count == 0) return;
            _index = (_index + 1) % _assets.Count;
        }

        public void Previous()
        {
            if (_assets.Count == 0) return;
            _index = (_index - 1 + _assets.Count) % _assets.Count;
        }

        public SKImage? GetCurrentImage()
        {
            if (Current == null)
                return null;

            return _assetManager.GetImage(Current.Id);
        }

        public AssetDescriptor? GetCurrentAsset()
        {
            return Current; 
        }

        public bool SelectById(string id)
        {
            if (_assets.Count == 0)
                return false;

            int index = _assets.FindIndex(a => a.Id == id);
            if (index < 0 || index == _index)
                return false;

            _index = index;
            return true;
        }

        public bool SelectByIndex(int index)
        {
            if (_assets.Count == 0)
                return false;

            if (index < 0 || index > _assets.Count - 1)
                return false;

            _index = index;

            return true;
        }
    }

}
