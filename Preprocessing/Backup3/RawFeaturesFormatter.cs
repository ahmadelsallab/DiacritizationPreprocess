using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Preprocessing
{
    class RawFeaturesFormatter : FeaturesFormatter
    {

        // Constructor
        public RawFeaturesFormatter(ConfigurationManager configManager, Logger logger, Word[] words)
            : base(configManager, logger, words)
        {

            stringLength = ComputeStringLength(configManager);

            // mrfType + p + r + f + s = 5, each 1 number
            //stringLength = (5 + Parser.maxIDs.POS_IDs[0] + 1);

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

            if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
            {
                wordFeatures.features = (offset + word.mrfType + 1).ToString() + ",";
                offset += Parser.maxIDs.mrfType + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
            {
                wordFeatures.features += (offset + word.p + 1).ToString() + ",";
                offset += Parser.maxIDs.p + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
            {
                wordFeatures.features += (offset + word.r + 1).ToString() + ",";
                offset += Parser.maxIDs.r + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
            {
                wordFeatures.features += (offset + word.f + 1).ToString() + ",";
                offset += Parser.maxIDs.f + 1;
                addedFeatures += 1;
            }

            if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
            {
                wordFeatures.features += (offset + word.s + 1).ToString() + ",";
                offset += Parser.maxIDs.s + 1;
                addedFeatures += 1;
            }
        }

        // Override the POS word features format
        protected override void FormatPOSWordFeatures(Word word, ref WordFeatures wordFeatures)
        {
            // Add POS ID's
            foreach (int ID in word.POS_IDs)
            {
                wordFeatures.features += (offset + ID + 1).ToString() + ",";
                addedFeatures += 1;
            }

            // Now Pad the rest of stringLength with zeros
            for (int i = addedFeatures + 1; i <= stringLength; i++)
            {
                wordFeatures.features += "0,";
            }//end for

            offset += Parser.maxIDs.POS_IDs[0] + 1;
        }

        // Method to check if the wordFeature string conforms to the expected string length or not
        protected override bool IsConformantStringLen(WordFeatures wordFeature)
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

        }// end IsConformantStringLen

        
        // Method to format the central context word feature
        protected override String FormatCentralContextWordFeatures(Word word, String features)
        {
            String centralWordFeatures = features;
            if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
            {
                centralWordFeatures += (word.mrfType + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
            {
                centralWordFeatures += (word.p + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
            {
                centralWordFeatures += (word.r + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
            {
                centralWordFeatures += (word.f + 1).ToString() + ",";
            }

            if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
            {
                centralWordFeatures += (word.s + 1).ToString() + ",";
            }

            return centralWordFeatures;
        }
      

        // Utility to compute the required string length in case of Raw features
        public static int [] ComputeChunksLength(ConfigurationManager configManager)
        {
            ArrayList chunksList = new ArrayList();
            int chunkOffset = 0;
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    break;
                case "POSOnly":
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];
                    break;

            }// end switch

            // Format of word in case of Type 1:
            // <Before Word 1><Before Word 2>...<Before Word n><Word><Specific Mrf features><Last Char features><After Word 1><After Word 2>...<After Word n>


            switch (configManager.contextType)
            {
                case "Type 1":

                    // First add the before context offsets
                    for (int i = 0; i < configManager.contextBeforeLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;
                    }// end for

                    // Add the word itself
                    chunksList.Add(chunkOffset);
                    chunkOffset += stringLength;

                    // Add any specific Mrf features
                    if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += 1;
                    }
                    
                    // Add the last character offset
                    if (configManager.lastCharFeaturesDepth != 0)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += configManager.lastCharFeaturesDepth;                        
                    }

                    // Last add the after context offsets
                    for (int i = 0; i < configManager.contextAfterLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;
                    }// end for

                    break;
                case "Type 2":
                    
                    for (int i = 0; i < configManager.contextLength; i++)
                    {
                        chunksList.Add(chunkOffset);
                        chunkOffset += stringLength;
                    }//end for
                    break;
            }// end switch

            return (int[])chunksList.ToArray(chunksList[0].GetType());

        }// end ComputeChunksLength

        // Utility to compute the required bitfield length in case of Raw features
        public static int ComputeBitfieldLengthAndOffset(ConfigurationManager configManager, out int[] offset)
        {
            int bitfieldLength = 0;
            int contextBitfieldLength = 0;
            ArrayList offsetList = new ArrayList();

            int bitfieldStartBoundary = 0;

            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }


                    // The Matlab bitfield is related to the maximum ID value not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        bitfieldLength = (Parser.maxIDs.mrfType + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.p + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.r + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.f + 1);
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        bitfieldLength += (Parser.maxIDs.s + 1);
                    }

                    break;
                case "POSOnly":

                    // The Matlab bitfield is related to the maximum ID value not to the max. number of IDs
                    bitfieldLength += (Parser.maxIDs.POS_IDs[0] + 1);
                    break;

            }// end switch

            // Format of word in case of Type 1:
            // <Before Word 1><Before Word 2>...<Before Word n><Word><Specific Mrf features><Last Char features><After Word 1><After Word 2>...<After Word n>

            // Adjust context length and offset
            switch (configManager.contextType)
            {
                case "Type 1":

                    // Add before and after bitfield lengths
                    //contextBitfieldLength = (configManager.contextBeforeLength + 1 + configManager.contextAfterLength) * bitfieldLength;

                    // Adjust the before boundary
                    for (int i = 0; i < configManager.contextBeforeLength; i++)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += bitfieldLength;
                    }

                    // Insert the word itself boundary
                    offsetList.Add(bitfieldStartBoundary);
                    bitfieldStartBoundary += bitfieldLength;


                    // Add any specific Mrf features
                    if ((String)configManager.addFeaturesToCentralContextWord["mrfType"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.mrfType + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["p"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.p + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["r"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.r + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["f"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.f + 1);                        
                    }

                    if ((String)configManager.addFeaturesToCentralContextWord["s"] == "Add")
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (Parser.maxIDs.s + 1);                        
                    }

                    // Add the last character offset
                    if (configManager.lastCharFeaturesDepth != 0)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += (int)FeaturesFormatter.LAST_CHAR_FEATURE_BITFIELD_LEN;                        
                    }

                    // Last add the after bitfield offsets
                    for (int i = 0; i < configManager.contextAfterLength; i++)
                    {
                        offsetList.Add(bitfieldStartBoundary);
                        bitfieldStartBoundary += bitfieldLength;
                        
                    }// end for

                    //contextBitfieldLength = bitfieldStartBoundary + bitfieldLength;
                    contextBitfieldLength = bitfieldStartBoundary;

                    break;

                case "Type 2":
                    contextBitfieldLength = configManager.contextLength * bitfieldLength;
                    break;
            }// end switch

            offset =(int[]) offsetList.ToArray(offsetList[0].GetType());

            return contextBitfieldLength;

        }// end ComputeBitfieldLengthAndOffset

        // Utility to compute the required string length in case of Raw features
        public static int ComputeStringLengthRaw(ConfigurationManager configManager)
        {
            int stringLength = 0;
            switch (configManager.outputFeatures)
            {
                case "MrfAndPOS":
                    // mrfType + p + r + f + s = 5, each 1 number
                    // POS = 1 number
                    //stringLength = 6;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    // For POS
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];

                    break;
                case "MrfOnly":
                    // mrfType + p + r + f + s = 5, each 1 number
                    //stringLength = 5;

                    if ((String)configManager.mrfSuppressFeaturesHashTable["mrfType"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["p"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["r"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["f"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    if ((String)configManager.mrfSuppressFeaturesHashTable["s"] != "Suppress")
                    {
                        stringLength += 1;
                    }

                    break;
                case "POSOnly":
                    // Add the max. number of POS fields. If less ID's exist the rest are padded zeros
                    stringLength += Parser.maxIDs.POS_IDs[1];
                    break;

            }// end switch
            return stringLength;
        }


    }


}
