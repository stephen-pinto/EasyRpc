﻿namespace CommPeerServices.Base.Plugins
{
    public interface ICommPlugin
    {
        public void Init(IPluginConfiguration config);

        public void Load();

        public void Unload();
    }
}