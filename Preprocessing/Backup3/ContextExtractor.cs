using System;
using System.Collections.Generic;
using System.Text;

namespace Preprocessing
{
    class ContextExtractor
    {
        // Reference to the words to extract the context from
        private WordFeatures[] wordsFeatures;

        // Length of the context
        private int contextLength;

        // Context features
        public WordFeatures[] contextFeatures;

        // Reference to logger
        private Logger logger;

        // Reference to the configuration manager
        private ConfigurationManager configManager;

        // Number of words with context features
        private int numWordsWithContextFeatures;

        // Constructor
        public ContextExtractor(WordFeatures[] wordsFeatures, Logger logger, ConfigurationManager configManager)
        {
            this.wordsFeatures = wordsFeatures;
            this.contextLength = configManager.contextLength;
            numWordsWithContextFeatures = wordsFeatures.Length / contextLength * contextLength;
            switch (configManager.contextType)
            {
                case "Type 1":
                    this.contextFeatures = new WordFeatures[wordsFeatures.Length];
                    break;
                case "Type 2":
                    this.contextFeatures = new WordFeatures[numWordsWithContextFeatures];
                    break;

            }// end switch
           
            this.logger = logger;
            this.configManager = configManager;

        }// end constructor

        // Method to start forming the context features
        public void ContextExtract()
        {

            logger.LogTrace("Extracting Context...");

            switch (configManager.contextType)
            {
                case "Type 1":
                    ContextExtractType1();
                    break;
                case "Type 2":
                    ContextExtractType2();
                    break;
                default:
                    Console.WriteLine("Wrong ContextType {0}. Valid configurations are Type 1 or Type 2", configManager.contextType);
                    break;
            }// end switch
            logger.LogTrace("Context Extraction done successfuly");
        }// end ContextExtract()

        // Method to start forming the context features for Type 1
        public void ContextExtractType1()
        {

            // Loop on all words features
            for (int i = 0; i < wordsFeatures.Length; i++)
            {
                // Initialize the context features for this word
                contextFeatures[i] = new WordFeatures();

                // Set the target
                contextFeatures[i].target = wordsFeatures[i].target;

                // Fill in the BEFORE context words
                for (int j = configManager.contextBeforeLength; j > 0; j--)
                {
                    if(i > j)
                    {
                        contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i - j].features;
                    }
                    else
                    {
                        contextFeatures[i].features = contextFeatures[i].features + FeaturesFormatter.emptyFeatureString;
                    }
                } // end for BEFORE

                // Put the concerned word in its context
                if (configManager.addFeaturesToCentralContextWord.Count != 0)
                {
                    // There exists special request for the central word
                    contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i].centralContextWordFeatures;
                }
                else
                {
                    // Just add the normal word
                    contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i].features;
                }

                // Always add the central word features. If no specific request, then all Mrf must be marked Suppress in the Configurations.xml file and hence central = wordfeatures string normally
                //contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i].centralContextWordFeatures;
                

                // Insert the last characters features
                if (wordsFeatures[i].lastCharFeatures != "")
                {
                    contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i].lastCharFeatures;
                }

                // Fill in the AFTER context words
                for (int j = 1; j <= configManager.contextAfterLength; j++ )
                {
                    if ((i + j) < wordsFeatures.Length)
                    {
                        contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i + j].features;
                    }
                    else
                    {
                        contextFeatures[i].features = contextFeatures[i].features + FeaturesFormatter.emptyFeatureString;
                    }
                    
                }// end for AFTER

            } // end for wordsFeatures.Length

        }// end ContextExtractType1()

        // Method to start forming the context features for Type 2
        public void ContextExtractType2()
        {
            // Temporary array to hold the context words
            WordFeatures[] contextWords = new WordFeatures[contextLength];

            // Loop on all words features
            for (int i = 0; i < numWordsWithContextFeatures; i += contextLength)
            {
                // Fill in the context words
                for (int j = 0; j < contextLength; j++)
                {
                    contextWords[j] = wordsFeatures[i + j];
                } // end for contextLength

                // Get the word context features
                for (int m = 0; m < contextLength; m++)
                {
                    contextFeatures[i + m] = GetWordContextType2(contextWords, m);

                } // end for contextLength

            } // end for wordsFeatures.Length

        }// end ContextExtractType2()


        
        // Method to form the context features of one a word for Type 2
        private WordFeatures GetWordContextType2(WordFeatures[] contextWords, int wordPosition)
        {
            WordFeatures contextFeatures = new WordFeatures();
            
            // Set the target
            contextFeatures.target = contextWords[wordPosition].target;

            // Put the word at the begnining of the formed string
            contextFeatures.features = contextWords[wordPosition].features;

            // Add the other words features next
            for (int i = 0; i < contextWords.Length; i++ )
            {
                // Don't add the wordPosition to the context, it's aleady the first one
                if (i != wordPosition)
                {
                    contextFeatures.features = contextFeatures.features + contextWords[i].features;
                }
            }// end for
            
            return contextFeatures;

        }// end GetWordContextType2

        ~ContextExtractor()
        {
            // Free the context features
            for (int i = 0; i < contextFeatures.Length; i++)
            {
                contextFeatures[i] = null;
            }
            contextFeatures = null;
        }
    }
}
