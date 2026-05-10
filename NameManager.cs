namespace RealmStudioX.Infrastructure
{
    public static class NameManager
    {
        public static string GenerateRandomPlaceName(List<INameGenerator> generators)
        {
            string generatedName = string.Empty;

            if (generators.Count > 0)
            {
                int selectedGeneratorIndex = -1;
                int guardCount = 0;
                int maxTries = generators.Count * generators.Count;

                while (guardCount < maxTries && (selectedGeneratorIndex < 0 || generators[selectedGeneratorIndex] is NameBaseLanguage))
                {
                    guardCount++;
                    selectedGeneratorIndex = Random.Shared.Next(0, generators.Count);
                }

                Random.Shared.Next(0, generators.Count);

                if (generators[selectedGeneratorIndex] is NameGenerator nameGen)
                {
                    generatedName = GenerateName(nameGen, generators);
                }
                else if (generators[selectedGeneratorIndex] is NameBase nameBase)
                {
                    generatedName = GenerateName(nameBase, generators);
                }
            }

            return generatedName;
        }

        public static string GenerateRandomWaterFeatureName()
        {
            string generatedName = string.Empty;
            List<INameGenerator> generators = [];
            NameGenerator? nameGen = null;

            foreach (NameGenerator ng in AssetManager.NameGenerators)
            {
                if (ng.NameGeneratorName.Contains("Bodies of Water"))
                {
                    nameGen = ng;
                }
            }

            foreach (NameBase nameBase in AssetManager.NameBases)
            {
                if (nameBase.IsNameBaseSelected)
                {
                    foreach (NameBaseLanguage language in nameBase.Languages)
                    {
                        if (language.IsLanguageSelected)
                        {
                            generators.Add(language);
                        }
                    }
                }
            }

            if (nameGen != null)
            {
                generatedName = GenerateName(nameGen, generators);
            }
            return generatedName;
        }

        private static string GenerateRandomNameBaseName()
        {
            string generatedName = string.Empty;
            List<INameGenerator> generators = [];

            foreach (NameBase nameBase in AssetManager.NameBases)
            {
                if (nameBase.IsNameBaseSelected)
                {
                    foreach (NameBaseLanguage language in nameBase.Languages)
                    {
                        if (language.IsLanguageSelected)
                        {
                            generators.Add(language);
                        }
                    }
                }
            }

            if (generators.Count > 0)
            {
                int selectedGeneratorIndex = Random.Shared.Next(0, generators.Count);
                if (generators[selectedGeneratorIndex] is NameBase nameBase)
                {
                    generatedName = GenerateName(nameBase, generators);
                }
            }

            return generatedName;
        }

        private static string GenerateRandomNameForLanguage(List<INameGenerator> selectedGenerators)
        {
            if (selectedGenerators.Count > 0)
            {
                List<NameBaseLanguage> SelectedNameLanguages = [];

                foreach (INameGenerator gen in selectedGenerators)
                {
                    if (gen is NameBaseLanguage l && l.IsLanguageSelected)
                    {
                        SelectedNameLanguages.Add(l);
                    }
                }

                if (SelectedNameLanguages.Count > 0)
                {
                    // select a random language from the namebase
                    int languageIndex = Random.Shared.Next(0, SelectedNameLanguages.Count);

                    // simplified version of namebase name generation for now; select a name from the language
                    return SelectedNameLanguages[languageIndex].NameStrings[Random.Shared.Next(0, SelectedNameLanguages[languageIndex].NameStrings.Count)];

                }
                else
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        private static string GenerateName(NameGenerator nameGen, List<INameGenerator> selectedGenerators)
        {
            string generatedName;

            int column1Index = Random.Shared.Next(0, nameGen.Column1.Count);
            string column1Value = nameGen.Column1[column1Index];

            if (nameGen.Column2.Count > 0)
            {
                int column2Index = Random.Shared.Next(0, nameGen.Column2.Count);
                string column2Value = nameGen.Column2[column2Index];

                if (!string.IsNullOrEmpty(column2Value))
                {
                    generatedName = column1Value.Replace("%", column2Value);
                }
                else
                {
                    string nameBaseName = GenerateRandomNameBaseName();

                    if (!string.IsNullOrEmpty(nameBaseName))
                    {
                        generatedName = column1Value.Replace("%", nameBaseName);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            else
            {
                string name = GenerateRandomNameForLanguage(selectedGenerators);

                if (!string.IsNullOrEmpty(name))
                {
                    generatedName = column1Value.Replace("%", name);
                }
                else
                {
                    return string.Empty;
                }
            }

            return generatedName;
        }

        private static string GenerateName(NameBase nameBase, List<INameGenerator> selectedGenerators)
        {
            List<NameBaseLanguage> SelectedNameLanguages = [];

            foreach (INameGenerator gen in selectedGenerators)
            {
                if (gen is NameBaseLanguage l && l.IsLanguageSelected)
                {
                    foreach (NameBaseLanguage nbl in nameBase.Languages)
                    {
                        if (nbl.Language == l.Language)
                        {
                            SelectedNameLanguages.Add(nbl);
                        }
                    }
                }
            }

            if (SelectedNameLanguages.Count > 0)
            {
                // select a random language from the namebase
                int languageIndex = Random.Shared.Next(0, SelectedNameLanguages.Count);

                // simplified version of namebase name generation for now; select a name from the language
                return SelectedNameLanguages[languageIndex].NameStrings[Random.Shared.Next(0, SelectedNameLanguages[languageIndex].NameStrings.Count)];
            }

            return string.Empty;
        }
    }
}
