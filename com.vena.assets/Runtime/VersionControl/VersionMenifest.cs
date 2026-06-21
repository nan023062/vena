// -----------------------------------------------------------------------------
// Vena Assets
// Unity asset pipeline toolkit (AssetBundle / VersionControl / OSS / Compression).
// Copyright (c) Nan Li.
// Licensed under the terms defined in the repository LICENSE file.
// -----------------------------------------------------------------------------

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
#pragma warning disable CS0618

namespace Vena.Assets
{

    [System.Serializable]
    public class VersionManifest
    {
        [System.Serializable]
        public class Line
        {
            public string name;
            public string md5;
            public long size;

            public override string ToString()
            {
                return string.Format("{0}|{1}@{2}\n", name, md5, size);
            }

            public Line(string name, string md5, long size)
            {
                this.name = name;
                this.md5 = md5;
                this.size = size;
            }

            public Line(string content)
            {
                string[] contents = content.Split('|', '@');
                name = contents[0];
                md5 = contents[1];
                size = long.Parse(contents[2]);
            }
        }

        public const string FileName = "VersionManifest.txt";

        [SerializeField] public DateTime dateTime;

        [SerializeField] public Version version;

        public readonly Dictionary<string, Line> Lines;

        public VersionManifest()
        {
            Lines = new Dictionary<string, Line>();
        }

        public void Add(string name, string md5, long size)
        {
            Lines[name] = new Line(name, md5, size);
        }

        public Line Get(string name)
        {
            Line line;
            Lines.TryGetValue(name, out line);
            return line;
        }

        public string Serialize()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("version={0},dataTime={1},count={2}\n", version, dateTime.ToLongDateString(), Lines.Count);
            var e = Lines.GetEnumerator();
            while (e.MoveNext()) {
                var line = e.Current.Value;
                stringBuilder.AppendFormat(line.ToString());
            }
            return stringBuilder.ToString();
        }

        public bool Deserialize(string content)
        {
            try
            {
                string[] contents = content.Split('\n');
                string[] mainDesc = contents[0].Split(',', '=');
                version = new Version(mainDesc[1]);
                dateTime = DateTime.Parse(mainDesc[3]);
                int count = int.Parse(mainDesc[5]);

                Lines.Clear();
                for (int i = 1; i < contents.Length; i++)
                {
                    string lineDesc = contents[i];
                    if (string.IsNullOrEmpty(lineDesc)) continue;
                    Line line = new Line(lineDesc);
                    Lines[line.name] = line;
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("Deserialize(), Error = {0}!", ex.Message);
            }
            return false;
        }

        public IEnumerator ReadFromFileByWWW(string fileDirUrl)
        {
            string fullUrl = Path.Combine(fileDirUrl, FileName);
            fullUrl = fullUrl.Replace('\\', '/');
            using var www = new WWW(fullUrl);
            yield return www;
            if (www.isDone)
            {
                if (string.IsNullOrEmpty(www.error))
                {
                    if (Deserialize(www.text))
                        Debug.LogFormat("ReadFromFileByWWW() Success! version = {0}, dateTime = {1} !", version, dateTime);
                }
                else
                {
                    Debug.LogErrorFormat("ReadFromFileByWWW() Failed ! url = {0}, msg = {1} !", fullUrl, www.error);
                }
            }
        }

        public void ReadFromFileIO(string fileDirUrl)
        {
            string fullUrl = Path.Combine(fileDirUrl, FileName);
            fullUrl = fullUrl.Replace('\\', '/');
            string content = FileIO.SafeReadAllText(fullUrl);
            if (Deserialize(content)) { }
                //Debug.LogFormat("ReadFromFileIO() Success! version = {0}, dateTime = {1} !", version, dateTime);
        }

        public void WriteToFile(string fileDirPath)
        {
            dateTime = DateTime.Now;
            string fullPath = Path.Combine(fileDirPath, FileName);
            File.WriteAllText(fullPath, Serialize(), Encoding.UTF8);
            //Debug.LogFormat("WriteToFile() Success! version = {0}, dateTime = {1} !",version, dateTime);
        }

        public class CompareInfo
        {
            public string[] addList;
            public string[] removeList;
            public string[] unchanges;
        }

        public CompareInfo CompareTo(VersionManifest target)
        {
            var result =  new CompareInfo();
            result.addList = new string[Lines.Count];
            Lines.Keys.CopyTo(result.addList, 0);
            return result;
        }
    }
}
