using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace HeurekaGames
{
    public static class ClassBuilder
    {
        const char const_slash = (char)'/';
        const char const_underscore = (char)'_';

        private static void saveNewClass(string path, string assemblyName, object myObject)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(path + "/" + assemblyName + ".cs", FileMode.OpenOrCreate);
            bf.Serialize(file, myObject);
            file.Close();
        }

        /// <summary>
        /// Test if the compiled enums match the source
        /// </summary>
        /// <typeparam name="T">System.Enum</typeparam>
        /// <param name="identifiers">A list of identifiers to match against</param>
        /// <returns></returns>
        public static bool IsUpToDate<T>(string[] identifiers)
        {
            if (typeof(T).BaseType != typeof(System.Enum))
            {
                UnityEngine.Debug.LogError("IsUpToDate only takes types of Enum as type parameter");
            }

            bool upToDate = true;

            foreach (string singleStat in identifiers)
            {
                if (!Enum.IsDefined(typeof(T), singleStat))
                    upToDate = false;
            }

            //Loop enum values
            foreach (var id in Enum.GetValues(typeof(T)))
                if (!identifiers.Any(val => val.Equals(id.ToString())))
                    upToDate = false;

            return upToDate;
        }
    
        public static void CreateEnum(string className, string assemblyName, Dictionary<string, int> list, string folderPath, string dbPath)
        {
            string classContent = string.Empty;
            classContent += getFileBeginning(className, assemblyName);

            foreach (KeyValuePair<string, int> pair in list)
                classContent += "\t\t" + pair.Key + " = " + pair.Value + "," + Environment.NewLine;

            classContent += getFileEnd(className, assemblyName);

            save(folderPath, assemblyName, classContent, dbPath);
        }

        public static void CreateEnum(string className, string assemblyName, string[] list, string folderPath, string dbPath)
        {
            string classContent = string.Empty;
            classContent += getFileBeginning(className, assemblyName);

            foreach (string str in list)
                classContent += "\t\t" + str + "," + Environment.NewLine;

            classContent += getFileEnd(className, assemblyName);

            save(folderPath, assemblyName, classContent, dbPath);

        }

        private static string getFileEnd(string className, string assemblyName)
        {
            string part = string.Empty;

            part += "\t}" + Environment.NewLine;
            //classContent += "}" + Environment.NewLine;

            if (!string.IsNullOrEmpty(className))
                part += "}" + Environment.NewLine;

            return part;
        }

        //Beginning of the new file
        private static string getFileBeginning(string className, string assemblyName)
        {
            string part = string.Empty;

            //If we want a new class
            if (!string.IsNullOrEmpty(className))
                part += "\tpublic partial class " + className + Environment.NewLine + "\t{" + Environment.NewLine;

            part += "\tpublic enum " + assemblyName + Environment.NewLine + "\t{" + Environment.NewLine;

            return part;
        }

        private static void save(string projectPath, string assemblyName, string classContent, string dbPath)
        {
            System.IO.File.WriteAllText(projectPath + assemblyName + ".cs", classContent);
            UnityEditor.AssetDatabase.ImportAsset(dbPath + assemblyName + ".cs");
        }

        public static string[] GetEnumFriendlyIdentifiers(string[] identifiers)
        {
            //Get lists of levels
            return (from scene in identifiers select scene.Replace(const_slash, const_underscore).Remove(scene.Length - (scene.Split('.').Last<string>().Length + 1))).ToArray();
        }

        public static string GetEnumFriendlyIdentifier(string assetPath)
        {
            return assetPath.Replace(const_slash, const_underscore).Remove(assetPath.Length - (assetPath.Split('.').Last<string>().Length + 1));
        }

        public static string GetResourceEnumIdentifier(string assetPath)
        {
            string[] splits = assetPath.Split('/');
            return splits.Last<string>().Split('.').First<string>();
        }

        internal static void CreateResourceClass(string RESOURCEENUM, Dictionary<Type, List<string>> typeDict, string FULLPATH, string DBPATH)
        {
            //foreach (KeyValuePair<Type, List<string>> pair in typeDict)
            {
                //EZResourceController.Instance.AddRessource(
                //TODO: It wont be good enough to create simple enums, Need to create actual classes with properties and lists to store Type, name and path in any meaningful way
                //create(RESOURCEENUM, pair.Key.Name, pair.Value);
                ClassBuilder.CreateResourceClass(RESOURCEENUM, typeDict, FULLPATH, DBPATH);
            }
        }
    }
}

    
