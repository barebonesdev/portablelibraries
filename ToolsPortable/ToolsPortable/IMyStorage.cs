using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;

namespace ToolsPortable
{
    /// <summary>
    /// Extending class should have a static constructor that initializes _instance.
    /// </summary>
    public abstract class IMyStorage
    {
        private static readonly string BACKUP_FOLDER = "MyUniqueBackupFolder/";

        private static int _savingCount = 0;

        /// <summary>
        /// Returns true if there's still a save operation going on
        /// </summary>
        public static bool IsStillSaving
        {
            get { return _savingCount > 0; }
        }

        private static void incrementSavingCount()
        {
            lock (_lock)
            {
                if (_savingCount < 0)
                    _savingCount = 1;

                else
                    _savingCount++;
            }
        }

        private static void decrementSavingCount()
        {
            lock (_lock)
            {
                if (_savingCount <= 0)
                    return;

                _savingCount--;
            }
        }

        /// <summary>
        /// When the app or background service launches, it should set Instance = new MyStorage();
        /// </summary>
        public static IMyStorage Instance { get; set; }

        protected static object _lock = new object();

        # region Load

        public static Stream LoadStream(string fileName)
        {
            lock (_lock)
            {
                try
                {
                    if (FileExists(fileName)) //load the file
                        return Instance.loadStream(fileName);
                }

                catch (Exception e)
                {
                    string err = e.ToString();
                    string blah = err;
                }

                //if the file wasn't found, or if an error was thrown while loading, it'll load the backup
                try
                {
                    if (FileExists(BACKUP_FOLDER + fileName))
                    {
                        //first copy the backup into the normal file location. This will prevent
                        //any future saves from backing up an already corrupted file!
                        CopyFile(BACKUP_FOLDER + fileName, fileName);

                        //then load the file copied from backup
                        return Instance.loadStream(fileName);
                    }
                }

                catch { }

                return null;
            }
        }

        /// <summary>
        /// Loads the object. If file does not exist, returns default(T). If failed to deserialize, returns default(T). Will try loading from backup before returning default(T).
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        public static T Load<T>(string fileName, params Type[] knownTypes)
        {
            lock (_lock)
            {
                try
                {
                    if (FileExists(fileName)) //load the file
                        return Instance.load<T>(fileName, knownTypes);
                }

                catch (Exception e)
                {
                    string err = e.ToString();
                    string blah = err;
                }

                //if the file wasn't found, or if an error was thrown while loading, it'll load the backup
                try
                {
                    if (FileExists(BACKUP_FOLDER + fileName))
                    {
                        //first copy the backup into the normal file location. This will prevent
                        //any future saves from backing up an already corrupted file!
                        CopyFile(BACKUP_FOLDER + fileName, fileName);

                        //then load the file copied from backup
                        return Instance.load<T>(fileName, knownTypes);
                    }
                }

                catch { }

                return default(T);
            }
        }

        /// <summary>
        /// Assumes file exists. Does not use try/catch. Should simply load file.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        protected abstract Stream loadStream(string fileName);

        /// <summary>
        /// Assumes that file exists. Does not use try/catch. Should simply load the file using DataContractSerializer and cast it to the given type and return it.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fileName"></param>
        /// <param name="knownTypes"></param>
        /// <returns></returns>
        protected abstract T load<T>(string fileName, params Type[] knownTypes);

        # endregion

        # region DeleteDirectory

        /// <summary>
        /// Should be formatted like "Folder/Subfolder/". If directory doesn't exist, it doesn't do anything.
        /// </summary>
        /// <param name="folderToRemove"></param>
        public static void DeleteDirectory(string folderToRemove)
        {
            lock (_lock)
            {
                try
                {
                    if (DirectoryExists(folderToRemove))
                    {
                        Instance.deleteDirectory(folderToRemove);

                        //delete backup dir
                        if (DirectoryExists(BACKUP_FOLDER + folderToRemove))
                            Instance.deleteDirectory(BACKUP_FOLDER + folderToRemove);
                    }
                }

                catch (Exception e) { string error = e.ToString(); }
            }
        }

        /// <summary>
        /// Should recursively delete all files in the directory and delete the directory itself. Assumed that directory exists. Does NOT try/catch.
        /// </summary>
        /// <param name="folderToRemove"></param>
        /// <returns></returns>
        protected abstract void deleteDirectory(string folderToRemove);

        # endregion

        # region DeleteFile

        /// <summary>
        /// Deletes the file if it exists.
        /// </summary>
        /// <param name="fileName"></param>
        public static void DeleteFile(string fileName, bool removeBackup)
        {
            lock (_lock)
            {
                if (FileExists(fileName))
                    Instance.deleteFile(fileName);

                //remove backup
                try
                {
                    if (removeBackup && FileExists(BACKUP_FOLDER + fileName))
                        Instance.deleteFile(BACKUP_FOLDER + fileName);
                }

                catch { }
            }
        }

        /// <summary>
        /// Simply deletes file, assumes that it exists.
        /// </summary>
        /// <param name="fileName"></param>
        protected abstract void deleteFile(string fileName);

        # endregion

        # region MoveDirectory

        /// <summary>
        /// Will delete the destination directory and copy the folderToMove directory to the destination. Returns false if an exception was thrown.
        /// </summary>
        /// <param name="folderToMove"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static bool MoveDirectory(string folderToMove, string destination)
        {
            try
            {
                lock (_lock)
                {
                    //delete desination directory
                    DeleteDirectory(destination);

                    //if the folderToMove exists, move it
                    if (DirectoryExists(folderToMove))
                        Instance.moveDirectory(folderToMove, destination);

                    //else create a blank directory
                    else if (destination.EndsWith("/"))
                        CreateDir(destination + "blah");
                    else
                        CreateDir(destination + "/blah");

                    return true;
                }
            }

            catch { return false; }
        }

        /// <summary>
        /// Moves the folder. Assumes that the folderToMove exists and that the desination isn't already a directory. Does not handle exceptions.
        /// </summary>
        /// <param name="folderToMove"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected abstract void moveDirectory(string folderToMove, string destination);

        # endregion

        # region MoveFile

        /// <summary>
        /// First will delete any file that exists at newFileName, and then it'll mvoe oldFileName to the new file name
        /// </summary>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        public static void MoveFile(string oldFileName, string newFileName)
        {
            lock (_lock)
            {
                CreateDir(newFileName);

                //delete existing file at newFileName
                DeleteFile(newFileName, false);

                //execute the move
                if (FileExists(oldFileName))
                    Instance.moveFile(oldFileName, newFileName);
            }
        }

        /// <summary>
        /// Assumes that FileExists(newFileName) is false. Should simply execute store.MoveFile(old, new) or something similar
        /// </summary>
        /// <param name="oldFileName"></param>
        /// <param name="newFileName"></param>
        protected abstract void moveFile(string oldFileName, string newFileName);

        # endregion

        # region CopyFile

        public static void CopyFile(string existingFileName, string newFileName)
        {
            lock (_lock)
            {
                if (FileExists(existingFileName))
                {
                    CreateDir(newFileName);

                    Instance.copyFile(existingFileName, newFileName);
                }
            }
        }

        protected abstract void copyFile(string existingFileName, string newFileName);

        # endregion

        # region CreateDir

        /// <summary>
        /// Should end with a '/'
        /// </summary>
        /// <param name="fileName"></param>
        public static void CreateDir(string fileName)
        {
            lock (_lock)
            {
                int index = fileName.IndexOf('/');

                if (index >= 0)
                {
                    while (index >= 0)
                    {
                        if (!DirectoryExists(fileName.Substring(0, index)))
                        {
                            Instance.createDirectory(fileName.Substring(0, index));
                        }

                        index = fileName.IndexOf('/', index + 1);
                    }
                }
            }
        }

        /// <summary>
        /// This should simply execute store.CreateDirectory or something similar.
        /// </summary>
        /// <param name="fileName"></param>
        protected abstract void createDirectory(string fileName);

        # endregion

        # region DirectoryExists

        public static bool DirectoryExists(string folder)
        {
            lock (_lock)
            {
                return Instance.directoryExists(folder);
            }
        }

        /// <summary>
        /// This should execute store.DirectoryExists or something similar
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        protected abstract bool directoryExists(string folder);

        # endregion

        # region FileExists

        public static bool FileExists(string fileName)
        {
            lock (_lock)
            {
                return Instance.fileExists(fileName);
            }
        }

        protected abstract bool fileExists(string fileName);

        # endregion

        # region CreateSafeFileName

        public static string CreateSafeFileName(string originalFileName)
        {
            return StringTools.CreateSafeFileName(originalFileName);
        }

        # endregion

        #region Serialize

        public static void Serialize(Stream stream, object data, params Type[] knownTypes)
        {
            new DataContractSerializer(data.GetType(), knownTypes).WriteObject(stream, data);
        }

        #endregion

        # region Save

        public static void Save(string fileName, object data, params Type[] knownTypes)
        {
            incrementSavingCount();

            try
            {
                lock (_lock)
                {
                    //if data is null, do nothing
                    if (data == null)
                    {
                        //not necessary to decrement here, because the "finally" does it!!!
                        //decrementSavingCount();
                        return;
                    }

                    CreateDir(fileName);

                    bool copyFailed = false;

                    try
                    {
                        //first copy existing file to backup folder
                        CopyFile(fileName, BACKUP_FOLDER + fileName);
                    }

                    catch { copyFailed = true; }

                    //now do the save
                    Instance.save(fileName, data, knownTypes);


                    if (copyFailed)
                        throw new Exception("Exception in CopyFile(), fileName = \"" + fileName + '"');
                }
            }

            finally
            {
                //this is called before any exceptions are sent out
                decrementSavingCount();
            }
        }

        public static void Save(string fileName, Stream data)
        {
            incrementSavingCount();

            try
            {
                lock (_lock)
                {
                    //if data is null, do nothing
                    if (data == null)
                    {
                        //not necessary to decrement here, because the "finally" does it!!!
                        //decrementSavingCount();
                        return;
                    }

                    CreateDir(fileName);

                    bool copyFailed = false;

                    try
                    {
                        //first copy existing file to backup folder
                        CopyFile(fileName, BACKUP_FOLDER + fileName);
                    }

                    catch { copyFailed = true; }

                    //now do the save
                    Instance.save(fileName, data);


                    if (copyFailed)
                        throw new Exception("Exception in CopyFile(), fileName = \"" + fileName + '"');
                }
            }

            finally
            {
                //this is called before any exceptions are sent out
                decrementSavingCount();
            }
        }

        /// <summary>
        /// Simply saves, assumes directory exists and data is not null. No try/catch.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="data"></param>
        /// <param name="knownTypes"></param>
        protected abstract void save(string fileName, object data, params Type[] knownTypes);

        protected abstract void save(string fileName, Stream data);

        # endregion

        # region GetDirectoryNames

        /// <summary>
        /// Returns an array of the directory names, or an empty array if directory wasn't found. It returns JUST the directory name, no slashes or anything.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] GetDirectoryNames(string searchPattern)
        {
            try
            {
                return Instance.getDirectoryNames(searchPattern);
            }

            catch { return new string[0]; }
        }

        protected abstract string[] getDirectoryNames(string searchPattern);

        # endregion

        # region GetFileNames

        /// <summary>
        /// Returns the file names, or an empty array if not found.
        /// </summary>
        /// <param name="searchPattern"></param>
        /// <returns></returns>
        public static string[] GetFileNames(string searchPattern)
        {
            try
            {
                return Instance.getFileNames(searchPattern);
            }

            catch { return new string[0]; }
        }

        protected abstract string[] getFileNames(string searchPattern);

        # endregion
    }
}
