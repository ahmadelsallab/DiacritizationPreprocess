using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Preprocessing
{
    class NormalFeaturesFormatter  : FeaturesFormatter
    {
        // Constructor
        public NormalFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word [] words)
              : base(configManager, logger, words)
        {
            stringLength = ComputeStringLengthNormal(configManager);

            // Form the empty feature string
            emptyFeatureString = EmptyFeatureString();
        }

        // Method to put the features in their format
        protected override void FormatMrfWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            /*wordFeatures.features = ((double)word.mrfType / (double)Parser.maxIDs.mrfType).ToString() + "," +
                                    ((double)word.p / (double)Parser.maxIDs.p).ToString() + "," +
                                    ((double)word.r / (double)Parser.maxIDs.r).ToString() + "," +
                                    ((double)word.f / (double)Parser.maxIDs.f).ToString() + "," +
                                    ((double)word.s / (double)Parser.maxIDs.s).ToString() + ",";*/

            if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = ((double)word.mrfType / (double)Parser.maxIDs.mrfType).ToString() + ",";
            }

            if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ((double)word.p / (double)Parser.maxIDs.p).ToString() + ",";
            }

            if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ((double)word.r / (double)Parser.maxIDs.r).ToString() + ",";
            }

            if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ((double)word.f / (double)Parser.maxIDs.f).ToString() + ",";
            }

            if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ((double)word.s / (double)Parser.maxIDs.s).ToString() + ",";
            }
        }

        // Method to put the features in their format
        protected override void FormatWordIDWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            if ((String)configManager.suppressFeaturesHashTable["vocabularyWordID"] != "Suppress")
            {
                wordFeatures.features = wordFeatures.features + ((double)word.vocabularyWordID / (double)Parser.maxIDs.vocabularyWordID).ToString() + ",";
            }

        }

        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
        {
            String centralWordFeatures = features;
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += ((double)word.mrfType / (double)Parser.maxIDs.mrfType).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += ((double)word.p / (double)Parser.maxIDs.p).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += ((double)word.r / (double)Parser.maxIDs.r).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += ((double)word.f / (double)Parser.maxIDs.f).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += ((double)word.s / (double)Parser.maxIDs.s).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["vocabularyWordID"] == "Add")
            {
                centralWordFeatures += ((double)word.vocabularyWordID / (double)Parser.maxIDs.vocabularyWordID).ToString() + ",";
            }

            return centralWordFeatures;
        }

        // Override the POS word features format
        protected override void FormatPOSWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            wordFeatures.features = wordFeatures.features +
                                    (word.equivalentPOS_ID / Parser.maxIDs.equivalentPOS_ID).ToString() + 
                                    ",";
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
       /* protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // Split the features string
            String[] features = wordFeature.features.Split(",".ToCharArray());

            switch (configManager.outputFeatures)
            {
                // -1 to remove the last split value after the last , in the features
                case "MrfAndPOS":
                    return (features.Length - 1 == stringLength) ? true : false;
                    //break;
                case "MrfOnly":
                    return (features.Length - 1 == stringLength) ? true : false;
                    //break;
                case "POSOnly":
                    return (features.Length - 1 == stringLength) ? true : false;
                    //break;
                default:
                    return false;

            }// end switch
            // TODO: check if features without the mrf part conforms to Parser.maxIDs.POS_IDs[0] + 1 or not

        }// end IsConformantStringLen*/
        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthNormal(ConfigurationManager configManager)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "All":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    stringLength += 1;

                    // For Word Only
                    stringLength += 1;

                    break;

                case "POSAndWord":

                    // For POS
                    stringLength += 1;

                    // For Word Only
                    stringLength += 1;

                    break;

                case "MrfAndWord":

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }


                    // For Word Only
                    stringLength += 1;

                    break;

                case "WordOnly":

                    stringLength += 1;

                    break;

                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    stringLength = 0;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    stringLength += 1;
                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;
                    stringLength = 0;

                    if ((String)configManager.suppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.suppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }
                    break;
                case "POSOnly":
                    // POS = 1 number
                    stringLength = 1;
                    break;

            }// end switch

            return stringLength;
        }// end ComputeStringLength

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
        {
            // *2 to account for "," after each number
            return ((stringLength * 2) == wordFeature.features.Length ? true : false);

        }// end IsConformantStringLen

    }
}
