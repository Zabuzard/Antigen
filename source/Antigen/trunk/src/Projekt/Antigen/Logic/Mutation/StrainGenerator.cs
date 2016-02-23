using System;
using System.Collections.Generic;

namespace Antigen.Logic.Mutation
{
    /// <summary>
    /// Generator for strains as singleton.
    /// </summary>
    sealed class StrainGenerator
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        private static StrainGenerator sInstance;

        /// <summary>
        /// Maximal tries to generate a new strain.
        /// </summary>
        private const int MaxTries = 50;

        /// <summary>
        /// List of all strains.
        /// </summary>
        private readonly List<string> mStrains = new List<string>();
        /// <summary>
        /// List of already generated strains.
        /// </summary>
        private readonly List<string> mGeneratedStrains = new List<string>();
        /// <summary>
        /// Random object to generate random numbers.
        /// </summary>
        private readonly Random mRnd = new Random();

        /// <summary>
        /// Gets an instance of the strain generator.
        /// </summary>
        /// <returns>Instance of the strain generator</returns>
        public static StrainGenerator GetInstance()
        {
            return sInstance ?? (sInstance = new StrainGenerator());
        }

        /// <summary>
        /// Destroys the current instance of the strain generator.
        /// </summary>
        public static void Destroy()
        {
            sInstance = null;
        }

        /// <summary>
        /// Generates a new strain.
        /// </summary>
        /// <returns>Generated strain</returns>
        public string GenerateStrain()
        {
            string strain;
            int counter = 0;
            bool wasGeneratedBefore;
            do
            {
                strain = GenerateFamily();
                wasGeneratedBefore = mGeneratedStrains.Contains(strain);
                counter++;
            } while (wasGeneratedBefore && counter <= MaxTries);

            if (!wasGeneratedBefore)
            {
                mGeneratedStrains.Add(strain);
            }
            return strain;
        }

        /// <summary>
        /// Gets the index of a given family.
        /// </summary>
        /// <param name="strain">Strain to get index from family</param>
        /// <returns>Infex of family</returns>
        public int GetFamilyIndex(String strain)
        {
            return mStrains.IndexOf(strain);
        }

        /// <summary>
        /// Returns the count of the families
        /// </summary>
        /// <returns></returns>
        public int GetFamiliyCount()
        {
            return mStrains.Count;
        }

        /// <summary>
        /// Creates a new strain generator.
        /// </summary>
        private StrainGenerator()
        {
            Initialize();
        }

        /// <summary>
        /// Generates a random family.
        /// </summary>
        /// <returns>Random family string</returns>
        private string GenerateFamily()
        {
            return mStrains[mRnd.Next(0, mStrains.Count)];
        }

        /// <summary>
        /// Initializes the strain generator.
        /// </summary>
        private void Initialize()
        {
            mStrains.Add("Adeno");
            mStrains.Add("Arena");
            mStrains.Add("Arteri");
            mStrains.Add("Asco");
            mStrains.Add("Asfar");
            mStrains.Add("Baculo");
            mStrains.Add("Birna");
            mStrains.Add("Borna");
            mStrains.Add("Bunya");
            mStrains.Add("Calici");
            /*
            mStrains.Add("Caulimo");
            mStrains.Add("Circo");
            mStrains.Add("Clostero");
            mStrains.Add("Corona");
            mStrains.Add("Cortico");
            mStrains.Add("Cysto");
            mStrains.Add("Dicistro");
            mStrains.Add("Filo");
            mStrains.Add("Flavi");
            mStrains.Add("Gemini");
            mStrains.Add("Gutta");
            mStrains.Add("Hepadna");
            mStrains.Add("Hepe");
            mStrains.Add("Herpes");
            mStrains.Add("Lipothrix");
            mStrains.Add("Malacoherpes");
            mStrains.Add("Micro");
            mStrains.Add("Myo");
            mStrains.Add("Orthomyxo");
            mStrains.Add("Papilloma");
            mStrains.Add("Paramyxo");
            mStrains.Add("Parvo");
            mStrains.Add("Picorna");
            mStrains.Add("Plasma");
            mStrains.Add("Podo");
            mStrains.Add("Polydna");
            mStrains.Add("Polyoma");
            mStrains.Add("Poty");
            mStrains.Add("Pox");
            mStrains.Add("Reo");
            mStrains.Add("Retroviren");
            mStrains.Add("Rhabdo");
            mStrains.Add("Rudi");
            mStrains.Add("Sipho");
            mStrains.Add("Tecti");
            mStrains.Add("Toga");
            mStrains.Add("Toti");
            */
        }
    }
}
