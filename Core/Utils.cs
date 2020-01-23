using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.NetCore.Core
{
    public class Utils
    {
        internal bool GetFileExtension(string name)
        {
            var arr = name.Split('.');
            string ext = arr[arr.Length - 1].ToLower();
            string[] extension = { "jpg", "png", "jpeg", "tiff", "tif", "bmp" };

            // Full Syntax:: if (extension.Any(s => ext.Equals(s)))
            if (extension.Any(ext.Equals))
            {
                return true;
            }
            #region Alternative approach
            // foreach (string s in extension)
            // {
            //    if (ext.Equals(s))
            //        return true;
            // }
            #endregion
            return false;
        }
        // Process all files in the directory passed in, recurse on any directories 
        // that are found, and process the files they contain.
        internal string[] ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            List<string> result = new List<string>();
            foreach (string fileName in fileEntries)
            {
                if (GetFileExtension(Path.GetExtension(fileName)))
                {
                    // TODO: adding allowd extension only
                    Debug.Print(fileName);
                    result.Add(Path.GetFileName(fileName));
                }
            }
            return result.ToArray();
            // Recurse into subdirectories of this directory.
            //string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            //foreach (string subdirectory in subdirectoryEntries)
            //    ProcessDirectory(subdirectory);
        }
    }
}
