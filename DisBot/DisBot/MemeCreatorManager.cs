using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
using System.Net;
using System.Linq;
using System.Drawing;

namespace DisBot
{
    enum MemeCreatorStatus
    {
        Success,
        InvalidExtension,
        FileNotAvaiable,
        Other
    }

    class MemeCreatorManager
    {
        public readonly string Path;
        public readonly string[] AllowedExtensions;

        #region Constructors

        public MemeCreatorManager(string path, string[] allowedExtensions)
        {
            Path = path;
            AllowedExtensions = allowedExtensions;
        }

        #endregion

        public (string path, MemeCreatorStatus status) CreateMeme(Meme m, string upper, string lower)
        {
            string inPath = Path + "\\Meme.in";
            string outPath = Path + "\\Meme.jpg";

            if (AllowedExtensions != null && !AllowedExtensions.Any(x => m.URL.EndsWith(x)))
                return (null, MemeCreatorStatus.InvalidExtension);

            MemeCreatorStatus status = MemeCreatorStatus.Other;
            using (var wc = new WebClient())
            {
                using (var st = wc.OpenRead(m.URL))
                {
                    Bitmap bm = new Bitmap(st);
                    if (bm != null)
                        bm.Save(inPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                    else
                        status = MemeCreatorStatus.FileNotAvaiable;
                }
            }

            if (status == MemeCreatorStatus.FileNotAvaiable)
                return (null, status);

            MemeBuilder.Build(inPath, outPath, upper, lower);
            return (outPath, MemeCreatorStatus.Success);
        }
    }
}
