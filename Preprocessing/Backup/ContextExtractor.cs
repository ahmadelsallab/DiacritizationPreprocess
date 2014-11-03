using System;
using System.Collections.Generic;
using System.Text;

namespace Preprocessing
{
    class ContextExtractor
    {
        // Reference to the items to extract the context from
        private Feature[] wordsFeatures;

        // Length of the context
        private int contextLength;

        // Context features
        public Feature[] contextFeatures;

        // Reference to logger
        private Logger logger;

        // Reference to the configuration manager
        private ConfigurationManager configManager;

        // Number of items with context features
        private int numWordsWithContextFeatures;

        // Constructor
        public ContextExtractor(Feature[] wordsFeatures, Logger logger, ConfigurationManager configManager)
        {
            this.wordsFeatures = wordsFeatures;
            this.contextLength = configManager.contextLength;
            numWordsWithContextFeatures = wordsFeatures.Length / contextLength * contextLength;
            switch (configManager.contextType)
            {
                case "Type 1":
                    this.contextFeatures = new Feature[wordsFeatures.Length];
                    break;
                case "Type 2":
                    this.contextFeatures = new Feature[numWordsWithContextFeatures];
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
        // Format of word in case of Type 1:
        // <Before Word 1><Target 1><Before Word 2><Target 2>...<Before Word n><Target 2><Word><Specific Mrf features><Last Char features><After Word 1><Target 1><After Word 2><Target 2>...<After Word n><Target n>
        public void ContextExtractType1()
        {
            String targetString = "";
            // Loop on all items features
            for (int i = 0; i < wordsFeatures.Length; i++)
            {
                // Initialize the context features for this word
                contextFeatures[i] = new Feature();

                // Get original word reference
                contextFeatures[i].originalItem = wordsFeatures[i].originalItem;

                // Set the target
                contextFeatures[i].target = wordsFeatures[i].target;

                // Fill in the BEFORE context items
                for (int j = configManager.contextBeforeLength; j > 0; j--)
                {
                    if(i > j)
                    {
                        contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i - j].features;
                        targetString = wordsFeatures[i - j].targetString;
                    }
                    else
                    {
                        contextFeatures[i].features = contextFeatures[i].features + FeaturesFormatter.emptyFeatureString;
                        targetString = FeaturesFormatter.emptyTargetString;
                    }

                    // Add the CRF before target
                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {
                        if(j <= configManager.contextTargetBeforeLength)
                        {
                            contextFeatures[i].features += targetString;
                        }
                    }// end if CRF before targets
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
                //contextFeatures[i].features = contextFeatures[i].features + features[i].centralContextWordFeatures;
                

                // Insert the last characters features
                if (wordsFeatures[i].lastCharFeatures != "")
                {
                    contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i].lastCharFeatures;
                }

                // Fill in the AFTER context items
                targetString = "";
                for (int j = 1; j <= configManager.contextAfterLength; j++ )
                {
                    if ((i + j) < wordsFeatures.Length)
                    {
                        contextFeatures[i].features = contextFeatures[i].features + wordsFeatures[i + j].features;
                        targetString = wordsFeatures[i + j].targetString;
                    }
                    else
                    {
                        contextFeatures[i].features = contextFeatures[i].features + FeaturesFormatter.emptyFeatureString;
                        targetString = FeaturesFormatter.emptyTargetString;
                    }


                    // Add the CRF before target
                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {
                        if (j <= configManager.contextTargetAfterLength)
                        {
                            contextFeatures[i].features += targetString;
                        }
                    }// end if CRF before targets
                    
                }// end for AFTER

                //String [] contextFeaturesParts = contextFeatures[i].features.Split(",".ToCharArray());

                /*if (contextFeaturesParts.Length != 52)
                {
                    int x;
                    x = 1;
                }*/

                //if(contextFeatures[i].features.Le)
            } // end for features.Length

        }// end ContextExtractType1()

        // Method to start forming the context features for Type 2
        public void ContextExtractType2()
        {
            // Temporary array to hold the context items
            Feature[] contextWords = new Feature[contextLength];

            // Loop on all items features
            for (int i = 0; i < numWordsWithContextFeatures; i += contextLength)
            {
                // Fill in the context items
                for (int j = 0; j < contextLength; j++)
                {
                    contextWords[j] = wordsFeatures[i + j];
                } // end for contextLength

                // Get the word context features
                for (int m = 0; m < contextLength; m++)
                {
                    contextFeatures[i + m] = GetWordContextType2(contextWords, m);

                } // end for contextLength

            } // end for features.Length

        }// end ContextExtractType2()


        
        // Method to form the context features of one a word for Type 2
        // Format of Type 2:
        // <Concerned word>+<Context word 1><Target 1><Context word 2><Target 2>...<Context word n><Target n>
        private Feature GetWordContextType2(Feature[] contextWords, int wordPosition)
        {
            Feature contextFeatures = new Feature();
            
            // Set the target
            contextFeatures.target = contextWords[wordPosition].target;

            // Put the word at the begnining of the formed string
            contextFeatures.features = contextWords[wordPosition].features;

            // Get refrence to original word
            contextFeatures.originalItem = contextWords[wordPosition].originalItem;


            // Add the other items features next
            for (int i = 0; i < contextWords.Length; i++ )
            {
                // Don't add the wordPosition to the context, it's aleady the first one
                if (i != wordPosition)
                {
                    contextFeatures.features = contextFeatures.features + contextWords[i].features;
                    // Add the CRF target
                    if ((String)configManager.suppressFeaturesHashTable["ContextTargets"] != "Suppress")
                    {
                        contextFeatures.features += contextWords[i].targetString;
                    }
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
