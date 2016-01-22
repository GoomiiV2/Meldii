using System;
using Meldii.DataStructures;
using Meldii.Views;
using System.IO;
using LibGit2Sharp;
using Ionic.Zip;

namespace Meldii.AddonProviders
{
    class GitRepository : ProviderBase
    {
        private Info info;

        public GitRepository()
        {
        }

        private bool Init(string url)
        {
            info = new Info(url);
            return info.ok;
        }

        public override void PostDelete(AddonMetaData addon)
        {
            if (Init(addon.UpdateURL))
            {
                if (Directory.Exists(info.dlPath))
                {
                    ForceDeleteDirectory(info.dlPath);
                }
                if(File.Exists(info.packedFile))
                {
                    File.Delete(info.packedFile);
                }
            }
        }
        public override MelderInfo GetMelderInfo(string url)
        {       
            if (Init(url))
            {
                DownloadAddon();
                AddonMetaData meta = AddonManager.ParseZipForIni(info.packedFile);
                if (meta != null)
                {
                    MelderInfo mInfo = new MelderInfo();
                    mInfo.IsNotSuported = !IsSupported();
                    mInfo.Version = meta.Version;
                    mInfo.Patch = meta.Patch;
                    mInfo.ProviderType = meta.ProviderType;
                    mInfo.Dlurl = url;
                    return mInfo;
                }
            }
            return null;
        }
        public override void Update(AddonMetaData addon)
        {
            if(Init(addon.UpdateURL))
            {
                _Copy(Path.Combine(MeldiiSettings.Self.AddonLibaryPath, info.repoName) + ".zip", info.packedFile);
            }
        }
        public override void DownloadAddon(string url)
        {        
            if(!Init(url))
            {
                return;
            }
            DownloadAddon();
        }
        private void DownloadAddon()
        {
            bool updated = false;
            bool cloned = false;

            if (!Directory.Exists(info.packedPath))
            {
                Directory.CreateDirectory(info.packedPath);
            }

            if (Directory.Exists(info.dlPath) && Repository.IsValid(info.dlPath))
            {
                using (Repository repo = new Repository(info.dlPath))
                {
                    RepositoryStatus status = repo.RetrieveStatus();

                    string commit = repo.Head.Tip.Id.Sha;

                    // reset local changes
                    if (status.IsDirty)
                    {
                        repo.Reset(ResetMode.Hard);
                    }

                    // fetch & merge from origin
                    repo.Network.Fetch(repo.Network.Remotes["origin"]);
                    repo.MergeFetchedRefs(new Signature("meldii", "meldii@meldii.li", new DateTimeOffset()), new MergeOptions());

                    // updated
                    if (!repo.Head.Tip.Id.Sha.Equals(commit))
                    {
                        updated = true;
                    }
                }
            }
            else
            {
                if (Directory.Exists(info.dlPath))
                {
                    ForceDeleteDirectory(info.dlPath);
                }

                CloneOptions co = new CloneOptions();
                co.BranchName = info.branch;
                Repository.Clone(info.url, info.dlPath, co);
                updated = true;
                cloned = true;
            }

            //Pack & ship it
            if (updated)
            {
                if (File.Exists(info.packedFile))
                {
                    File.Delete(info.packedFile);
                }

                ZipFile zip = new ZipFile(info.packedFile);
                foreach (string file in Directory.EnumerateFiles(info.dlPath, "*.*", SearchOption.AllDirectories))
                {
                    string rPath = file.Replace(info.dlPath, "").TrimStart(new char[] { '\\', '/' });
                    if (!rPath.StartsWith(".git", StringComparison.CurrentCultureIgnoreCase) && !rPath.Equals("README.md", StringComparison.CurrentCultureIgnoreCase))
                    {
                        string[] split = rPath.Split(new char[] { '\\', '/' });
                        string zipPath = split.Length == 1 ? "" : string.Join("\\", split, 0, split.Length - 1);
                        zipPath = info.repoName + "\\" + zipPath;

                        zip.AddFile(file, zipPath);
                    }

                }
                zip.Save();
            }

            if (cloned)
            {
                _Copy(Path.Combine(MeldiiSettings.Self.AddonLibaryPath, info.repoName) + ".zip", info.packedFile);
            }
        }

        public void ForceDeleteDirectory(string path)
        {
            DirectoryInfo directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

            foreach (var info in directory.GetFileSystemInfos("*", SearchOption.AllDirectories))
            {
                info.Attributes = FileAttributes.Normal;
            }

            directory.Delete(true);
        }

        public void _Copy(string dest, string source)
        {
            if (File.Exists(dest))
                File.Delete(dest);

            File.Copy(source, dest);
        }
        public bool IsSupported()
        {
            foreach (string file in Directory.EnumerateFiles(info.dlPath, "*.*", SearchOption.AllDirectories))
            {
                if(file.Equals("not_supprted", StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }
            return true;
        }
        private class Info
        {
            public string branch;
            public string url;
            public string repoName;
            public string dlPath;
            public string packedPath;
            public string packedFile;
            public bool ok = true;

            public Info(string _url)
            {
                if (_url.EndsWith(".git"))
                {
                    url = _url;
                    branch = "master";
                    string[] uSplit = url.Split('@');
                    if (uSplit.Length > 1)
                    {
                        branch = uSplit[0];
                        url = uSplit[1];
                    }

                    repoName = Path.GetFileName(url);
                    repoName = repoName.Substring(0, repoName.Length - 4);

                    dlPath = Path.Combine(Path.Combine(tempDlDir, ".."), "git", "repos", repoName);
                    packedPath = Path.Combine(tempDlDir, "..", "git", "packed");
                    packedFile = Path.Combine(packedPath, repoName) + ".zip";

                    if(!Directory.Exists(Path.Combine(tempDlDir, "..", "git")))
                    {
                        Directory.CreateDirectory(Path.Combine(tempDlDir, "..", "git"));
                    }
                }
                else
                {
                    ok = false;
                }
            }
        }
    }
}
