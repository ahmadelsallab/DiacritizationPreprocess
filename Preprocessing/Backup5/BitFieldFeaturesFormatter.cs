using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class BitFieldFeaturesFormatter : FeaturesFormatter
    {
        // Constructor
        public BitFieldFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word[] words)
            : base(configManager, logger, words)
        {
            stringLength = ComputeStringLengthBitfield(configManager);
            
            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();

        }


        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = ConvertToBitfieldString(word.mrfType, (uint)Parser.maxIDs.mrfType);
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.p, (uint)Parser.maxIDs.p);
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.r, (uint)Parser.maxIDs.r);
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.f, (uint)Parser.maxIDs.f);
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ConvertToBitfieldString(word.s, (uint)Parser.maxIDs.s);
            }


            /*wordFeatures.features = ConvertToBitfieldString(word.mrfType, (uint)Parser.maxIDs.mrfType) +
                                    ConvertToBitfieldString(word.p, (uint)Parser.maxIDs.p) +
                                    ConvertToBitfieldString(word.r, (uint)Parser.maxIDs.r) +
                                    ConvertToBitfieldString(word.f, (uint)Parser.maxIDs.f) +
                                    ConvertToBitfieldString(word.s, (uint)Parser.maxIDs.s);*/
        }

        // Method to put the features in their format
        protected override void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                wordFeatures.features += ConvertToBitfieldString(word.vocabularyWordID, (uint)Parser.maxIDs.vocabularyWordID);
            }

        }

        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
        {
            String centralWordFeatures = features;       
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.mrfType, (uint)Parser.maxIDs.mrfType);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.p, (uint)Parser.maxIDs.p);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.r, (uint)Parser.maxIDs.r);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.f, (uint)Parser.maxIDs.f);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.s, (uint)Parser.maxIDs.s);
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                centralWordFeatures += ConvertToBitfieldString(word.vocabularyWordID, (uint)Parser.maxIDs.vocabularyWordID);
            }

            return centralWordFeatures;
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // *2 to account for "," after each number
            return ((stringLength * 2) == wordFeature.features.Length ? true : false);

        }// end IsConformantStringLen

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthBitfield(ConfigurationManager configManager)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":
                    
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    stringLength += (Parser.maxIDs.vocabularyWordID);

                    break;
                    
                case "POSAndWord":
                    
                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    stringLength += (Parser.maxIDs.vocabularyWordID);

                    break;

                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.vocabularyWordID);

                    break;

                case "WordOnly":

                    stringLength += (Parser.maxIDs.vocabularyWordID);

                    break;

                case "MrfAndPOS":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    //stringLength = (Parser.maxIDs.mrfType + 1) + (Parser.maxIDs.p + 1) + (Parser.maxIDs.r + 1) + (Parser.maxIDs.f + 1) + (Parser.maxIDs.s + 1) + (Parser.maxIDs.POS_IDs[0] + 1);                    
                    break;
                case "MrfOnly":
                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += (Parser.maxIDs.s + 1);
                    }

                    stringLength += (Parser.maxIDs.POS_IDs[0] + 1);
                    //stringLength = (Parser.maxIDs.mrfType + 1) + (Parser.maxIDs.p + 1) + (Parser.maxIDs.r + 1) + (Parser.maxIDs.f + 1) + (Parser.maxIDs.s + 1);                    
                    break;
                case "POSOnly":
                    stringLength = (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch
            return stringLength;
        }// end ComputeStringLength
    }
}
