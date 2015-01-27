using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Meldii.AddonProviders;

namespace Meldii.DataStructures
{
    public class MelderInfo
    {
        public string Version;
        public string Patch;
        public string Dlurl;
        public AddonProviderType ProviderType;
        public bool IsNotSuported;
    }
}
