using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Antigen.Screens;
using System;
using Antigen.Util;

namespace Antigen.GameManagement
{
    /// <summary>
    /// This class provides the methods for saving and loading the game state.
    /// </summary>
    sealed class SaveGameManager {
        private readonly ScreenManager mScreenManager;

        /// <summary>
        /// Creates a new SaveGameManager.
        /// </summary>
        /// <param name="screenManager">The screen manager of the game.</param>
        public SaveGameManager(ScreenManager screenManager)
        {
            mScreenManager = screenManager;
        }

        /// <summary>
        /// Public method for saving the gamescreen.
        /// </summary>
        /// <param name="name">Name of the gamesave.</param>
        public void Save(string name)
        {
            Serialize(mScreenManager.GetGameState(), Functions.GetFolderPath() + "\\gameSave_" + name + ".bin");
        }

        /// <summary>
        /// Public method for loading the gamescreen.
        /// </summary>
        /// <param name="name">Name of the gamesave to be loaded.</param>
        public static GameState Load(string name)
        {
            return (GameState)Deserialize(Functions.GetFolderPath() + "\\gameSave_" + name + ".bin");
        }

        /// <summary>
        /// This method serializes any Object which is passed as argument.
        /// The object has to be marked as serializable.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="fileName">The file the object shall be serialied to.</param>
        public static void Serialize(Object obj, String fileName)

        {
            IFormatter formatter = new BinaryFormatter();
            Stream stream = new FileStream(fileName,
                                     FileMode.Create,
                                     FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, obj);
            stream.Close();
        }

        /// <summary>
        /// This method allows to load the content of the saved file.
        /// </summary>
        /// <param name="fileName">The file containing the object to be deserialized.</param>
        /// <returns></returns>
        public static Object Deserialize(String fileName)
        {
            IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(fileName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);
                var obj = formatter.Deserialize(stream);
                stream.Close();
                return obj;
        }
    }
}
